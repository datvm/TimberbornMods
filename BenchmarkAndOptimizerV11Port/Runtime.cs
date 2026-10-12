using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using HarmonyLib;
using Timberborn.ModManagerScene;

namespace BenchmarkAndOptimizerV11;

internal static class Runtime
{
    private static readonly object Sync = new();
    private static readonly ConditionalWeakTable<object, CounterState> Counters = new();
    private static readonly ConditionalWeakTable<object, OwnerDispatchPlans> DispatchPlans = new();
    private static readonly Dictionary<string, ItemAdapter> AdapterCache = new(StringComparer.Ordinal);
    private static readonly HashSet<string> UnsupportedAdapters = new(StringComparer.Ordinal);
    private static readonly HashSet<string> LoggedWarnings = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BenchStat> BenchStats = new(StringComparer.Ordinal);
    private static readonly HashSet<string> Discovered = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> OriginByType = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> AssemblyNameByType = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> OriginByAssemblyPath = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> OriginByAssemblyName = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Type> DiscoveredTypes = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, MethodInfo> DirectMethodsByType = new(StringComparer.Ordinal);
    private static readonly HashSet<MethodBase> DirectPatchedMethods = new();
    private static Harmony? _harmony;
    private static bool _benchmarkRunning;
    private static int _settingsGeneration;

    private static Settings _settings = new();
    private static DateTime _nextReloadUtc = DateTime.MinValue;
    private static DateTime _configWriteUtc = DateTime.MinValue;
    private static long _benchmarkStart;
    private static bool _benchmarkCompleted;

    public static string ModPath { get; private set; } = "";
    public static string ConfigPath => Path.Combine(ModPath, "optimizer-v11.json");
    private static string DiscoveredPath => Path.Combine(ModPath, "optimizer-v11-discovered.txt");
    private static string FreezeLogPath => Path.Combine(ModPath, "optimizer-v11-freezes.log");

    public const int BenchmarkDurationSeconds = 120;

    // These systems own or synchronize authoritative simulation state. Skipping
    // their updates can break ordering assumptions and leave the world in a
    // partially-updated state, so they are permanently pinned to vanilla cadence.
    private static readonly HashSet<string> ProtectedTypeNames = new(StringComparer.Ordinal)
    {
        "TickableBucketService",
        "TickableEntity",
        "TickableSingletonService",
        "SingletonLifecycleService",
        "PhysicsSimulator",
        "BehaviorManager",
        "NavMeshObserver",
        "NavigationSynchronizer",
        "AutomationRunner",
        // Work dispatch and soil-contamination state must be updated at vanilla cadence.
        // Never suppress state transitions in these gameplay-authoritative services.
        "PlantingControler",
        "PlantMonitor",
        "SoilContaminationService",
        "WaterSimulator",
        "SpeedManager",
        "ConstructionSite",
        // Preserve FPS and read-only simulation throughput measurement cadence.
        "FramesPerSecondPanel",
        "FramesPerSecondCounter",
    };

    public static bool Enabled => _settings.Enabled;
    public static int DefaultInterval => _settings.DefaultInterval;
    public static bool IsProtected(string typeName) =>
        ProtectedTypeNames.Contains(ShortTypeName(typeName));
    public static IReadOnlyCollection<string> KnownTypes
    {
        get
        {
            lock (Sync)
            {
                return Discovered.Select(ShortTypeName).Distinct(StringComparer.Ordinal).OrderBy(x => x).ToArray();
            }
        }
    }

    public static string GetOrigin(string typeName)
    {
        lock (Sync)
        {
            if (OriginByType.TryGetValue(typeName, out var direct))
            {
                return direct;
            }

            var full = Discovered.FirstOrDefault(x => ShortTypeName(x) == typeName);
            return full is not null && OriginByType.TryGetValue(full, out var origin)
                ? origin
                : "UNKNOWN";
        }
    }

    public static string? GetThrottleOwnershipNote(string typeName)
    {
        Type? type;
        lock (Sync)
        {
            type = ResolveDiscoveredTypeLocked(typeName);
        }

        if (type is null)
        {
            return null;
        }

        var method = FindDirectTickMethod(type);
        var declaringType = method?.DeclaringType;
        if (method is null || declaringType is null || declaringType == type)
        {
            return null;
        }

        var depth = InheritanceDepth(type, declaringType);
        var depthText = depth > 0
            ? $" ({depth} inheritance level{(depth == 1 ? "" : "s")})"
            : "";

        return $"Inherited {method.Name}{depthText}. Effective throttle target: {FormatMethodTarget(method)}. " +
               "Shared base method is patched once; this concrete type keeps its own interval.";
    }

    public static string Mode =>
        BenchmarkActive ? "Benchmarking" :
        HasActiveThrottles() ? "Throttling" :
        "Vanilla fast path";

    public static bool IsBenchmarking => BenchmarkActive;
    public static double BenchmarkRemainingSeconds
    {
        get
        {
            if (!BenchmarkActive) return 0;
            var elapsed = (System.Diagnostics.Stopwatch.GetTimestamp() - _benchmarkStart)
                          / (double)System.Diagnostics.Stopwatch.Frequency;
            return Math.Max(0, BenchmarkDurationSeconds - elapsed);
        }
    }

    public static void SetEnabled(bool value)
    {
        _settings.Enabled = value;
        _settingsGeneration++;
        SaveSettings(_settings);
        RefreshDirectThrottlePatches();
    }

    public static void SetDefaultInterval(int value)
    {
        _settings.DefaultInterval = Math.Clamp(value, 1, 1000);
        _settingsGeneration++;
        SaveSettings(_settings);
        RefreshDirectThrottlePatches();
    }

    public static int GetInterval(string typeName)
    {
        if (IsProtected(typeName))
        {
            return 1;
        }

        if (_settings.Intervals.TryGetValue(typeName, out var value))
        {
            return Math.Clamp(value, 1, 1000);
        }

        var full = Discovered.FirstOrDefault(x => ShortTypeName(x) == typeName);
        if (full is not null && _settings.Intervals.TryGetValue(full, out value))
        {
            return Math.Clamp(value, 1, 1000);
        }

        return _settings.DefaultInterval;
    }

    public static void SetInterval(string typeName, int value)
    {
        var key = Discovered.FirstOrDefault(x => ShortTypeName(x) == typeName) ?? typeName;

        if (IsProtected(typeName))
        {
            _settings.Intervals[key] = 1;
            _settingsGeneration++;
            SaveSettings(_settings);
            RefreshDirectThrottlePatches();
            LogOnce($"protected-set:{ShortTypeName(typeName)}",
                $"protected system {typeName} is permanently pinned to interval=1");
            return;
        }

        value = Math.Clamp(value, 1, 1000);
        _settings.Intervals[key] = value;
        _settingsGeneration++;
        SaveSettings(_settings);
        RefreshDirectThrottlePatches();
    }

    public static void ResetIntervals()
    {
        _settings.DefaultInterval = 1;
        _settings.Intervals.Clear();
        _settingsGeneration++;
        SaveSettings(_settings);
        RefreshDirectThrottlePatches();
        Log("all optimizer intervals reset to vanilla (1)");
    }

    public static void StartBenchmark()
    {
        if (BenchmarkActive) return;
        lock (Sync) BenchStats.Clear();
        // Benchmark dispatcher is observational at protected vanilla cadence:
        // the per-class interval shortcut is suppressed during benchmarks.
        RemoveDirectThrottlePatches();
        if (_harmony is not null)
        {
            DispatcherPatcher.Patch(_harmony);
            TimberPhysicsBenchmarkPatcher.Patch(_harmony);
            FrameProfileSampler.Patch(_harmony);
        }
        _benchmarkStart = System.Diagnostics.Stopwatch.GetTimestamp();
        _benchmarkCompleted = false;
        _benchmarkRunning = true;
    }

    private static string ShortTypeName(string value)
    {
        var i = value.LastIndexOf('.');
        return i < 0 ? value : value[(i + 1)..];
    }

    private static Type? ResolveDiscoveredTypeLocked(string typeName)
    {
        if (DiscoveredTypes.TryGetValue(typeName, out var exact))
        {
            return exact;
        }

        foreach (var pair in DiscoveredTypes)
        {
            if (string.Equals(ShortTypeName(pair.Key), typeName, StringComparison.Ordinal))
            {
                return pair.Value;
            }
        }

        return null;
    }

    private static int InheritanceDepth(Type concreteType, Type declaringType)
    {
        var depth = 0;
        for (var current = concreteType; current is not null; current = current.BaseType)
        {
            if (current == declaringType)
            {
                return depth;
            }

            depth++;
        }

        return -1;
    }

    private static string FormatMethodTarget(MethodInfo method)
    {
        var declaring = method.DeclaringType;
        return declaring is null
            ? method.Name
            : $"{FormatTypeName(declaring)}.{method.Name}";
    }

    private static string FormatTypeName(Type type)
    {
        if (type.IsByRef)
        {
            return FormatTypeName(type.GetElementType()!) + "&";
        }

        if (type.IsArray)
        {
            return FormatTypeName(type.GetElementType()!) + "[]";
        }

        if (!type.IsGenericType)
        {
            return type.FullName?.Replace('+', '.') ?? type.Name;
        }

        var genericDefinition = type.GetGenericTypeDefinition();
        var baseName = genericDefinition.FullName ?? genericDefinition.Name;
        var tick = baseName.IndexOf((char)96);
        if (tick >= 0)
        {
            baseName = baseName[..tick];
        }

        baseName = baseName.Replace('+', '.');
        var arguments = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
        return $"{baseName}<{arguments}>";
    }

    public static void Initialize(string modPath, Harmony harmony)
    {
        _harmony = harmony;
        ModPath = modPath;
        Directory.CreateDirectory(ModPath);
        // CSV and frame samples are written only for an explicitly started benchmark.

        if (!File.Exists(ConfigPath))
        {
            _settings = Settings.CreateDefault();
            SaveSettings(_settings);
        }

        ReloadConfig(force: true);

        // Benchmarks are session-only and must never be resumed from persisted settings.
        _settings.BenchmarkSeconds = 0;
        _benchmarkRunning = false;
        _benchmarkCompleted = true;
        _benchmarkStart = System.Diagnostics.Stopwatch.GetTimestamp();

        // Benchmark-only startup: no game optimizers, startup type-discovery
        // hooks, GC interventions, save hooks or broad profiler scans.
        DiscoverLoadedOptimizableTypes();
        RefreshDirectThrottlePatches();
    }

    public static void RefreshDirectThrottlePatches()
    {
        if (_harmony is null || BenchmarkActive)
        {
            return;
        }

        RemoveDirectThrottlePatches();

        if (!_settings.Enabled)
        {
            return;
        }

        foreach (var pair in DiscoveredTypes)
        {
            var type = pair.Value;
            if (IsProtected(type) || GetInterval(type) <= 1)
            {
                continue;
            }

            var method = FindDirectTickMethod(type);
            if (method is null)
            {
                LogOnce(
                    $"no-direct-method:{type.FullName}",
                    $"warning: no directly patchable Tick/Update method found for {type.FullName}; leaving vanilla");
                continue;
            }

            method = ResolveImplementedMethod(method);
            var declaringType = method.DeclaringType;
            var inherited = declaringType is not null && declaringType != type;

            if (inherited)
            {
                var depth = InheritanceDepth(type, declaringType!);
                var effectiveTarget = FormatMethodTarget(method);
                LogOnce(
                    $"inherited-direct-method:{type.FullName}",
                    $"nested throttle mapping: {type.FullName}.{method.Name} is inherited" +
                    $"{(depth > 0 ? $" through {depth} inheritance level(s)" : "")}; " +
                    $"effective throttle target={effectiveTarget}; sibling runtime types keep their own intervals");
            }

            try
            {
                var prefix = AccessTools.Method(typeof(Runtime), nameof(DirectThrottlePrefix));

                // Several concrete types can share the same inherited method. Patch the
                // effective method only once; DirectThrottlePrefix uses __instance.GetType()
                // so each concrete sibling still gets its own configured interval.
                if (DirectPatchedMethods.Add(method))
                {
                    _harmony.Patch(
                        method,
                        prefix: new HarmonyMethod(prefix) { priority = Priority.First });

                    Log(
                        inherited
                            ? $"direct throttle patch installed: effective target {FormatMethodTarget(method)}"
                            : $"direct throttle patch installed: {type.FullName}.{method.Name}");
                }

                DirectMethodsByType[pair.Key] = method;
                Log(
                    inherited
                        ? $"direct throttle binding: {type.FullName} interval={GetInterval(type)} -> {FormatMethodTarget(method)}"
                        : $"direct throttle binding: {type.FullName} interval={GetInterval(type)}");
            }
            catch (Exception ex)
            {
                DirectPatchedMethods.Remove(method);
                LogOnce(
                    $"direct-patch-failed:{type.FullName}:{method.Name}",
                    $"warning: direct throttle patch failed for {type.FullName}.{method.Name}: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static void RemoveDirectThrottlePatches()
    {
        if (_harmony is null || DirectPatchedMethods.Count == 0)
        {
            return;
        }

        foreach (var method in DirectPatchedMethods.ToArray())
        {
            try
            {
                _harmony.Unpatch(method, HarmonyPatchType.Prefix, _harmony.Id);
            }
            catch
            {
                // A stale direct patch must never break settings changes.
            }
        }

        DirectPatchedMethods.Clear();
        DirectMethodsByType.Clear();
    }

    public static bool DirectThrottlePrefix(object __instance, MethodBase __originalMethod)
    {
        if (!_settings.Enabled || BenchmarkActive || IsProtected(__instance.GetType()))
        {
            return true;
        }

        return ShouldRun(__instance);
    }

    private static MethodInfo ResolveImplementedMethod(MethodInfo method)
    {
        var declaringType = method.DeclaringType;
        if (declaringType is null || method.ReflectedType == declaringType)
        {
            return method;
        }

        var parameterTypes = method.GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        return AccessTools.DeclaredMethod(declaringType, method.Name, parameterTypes)
            ?? method;
    }

    private static MethodInfo? FindDirectTickMethod(Type type)
    {
        foreach (var methodName in new[] { "Tick", "UpdateSingleton", "LateUpdateSingleton", "StartAndTick" })
        {
            var method = AccessTools.Method(type, methodName, Type.EmptyTypes);
            if (method is not null && method.ReturnType == typeof(void))
            {
                return method;
            }
        }

        return null;
    }

    private static void DiscoverLoadedOptimizableTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(x => x is not null).Cast<Type>().ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                var method = FindDirectTickMethod(type);
                if (method is null)
                {
                    continue;
                }

                var interfaces = type.GetInterfaces()
                    .Select(i => i.FullName ?? i.Name)
                    .ToArray();

                var looksOptimizable =
                    interfaces.Any(n =>
                        n == "Timberborn.TickSystem.ITickableSingleton" ||
                        n == "Timberborn.SingletonSystem.IUpdatableSingleton" ||
                        n == "Timberborn.SingletonSystem.ILateUpdatableSingleton" ||
                        n.Contains("ITickable", StringComparison.Ordinal)) ||
                    type.BaseType?.FullName == "Timberborn.TickSystem.TickableComponent";

                if (looksOptimizable)
                {
                    Discover(type);
                }
            }
        }

        Log($"direct throttle discovery complete: {DiscoveredTypes.Count} type(s)");
    }

    public static bool TryDispatch(
        object owner,
        string listFieldName,
        string? actualFieldName,
        string invokeMethodName,
        string? enabledMemberName = null)
    {
        ReloadConfig(force: false);

        if (!_settings.Enabled)
        {
            return true;
        }

        if (!BenchmarkActive && !HasActiveThrottles())
        {
            return true;
        }

        try
        {
            var listField = AccessTools.Field(owner.GetType(), listFieldName);
            if (listField?.GetValue(owner) is not IList list)
            {
                LogOnce(
                    $"missing-list:{owner.GetType().FullName}:{listFieldName}",
                    $"warning: {owner.GetType().FullName}.{listFieldName} unavailable; falling back to vanilla");
                return true;
            }

            var plan = GetDispatchPlan(
                owner,
                list,
                listFieldName,
                actualFieldName,
                invokeMethodName,
                enabledMemberName);

            if (plan is null)
            {
                return true;
            }

            // Critical fast path: an active optimization elsewhere must not force this
            // owner through our dispatcher. Let Timberborn run it natively unless this
            // exact owner contains a throttled type.
            if (!BenchmarkActive && !plan.HasThrottle)
            {
                return true;
            }

            foreach (var entry in plan.Entries)
            {
                if (!entry.Adapter.IsEnabled(entry.Item))
                {
                    continue;
                }

                // Benchmarking must never skip a tick, even if an old config
                // contains non-vanilla intervals.
                if (!BenchmarkActive && !ShouldRun(entry.Actual))
                {
                    continue;
                }

                if (BenchmarkActive)
                {
                    var started = System.Diagnostics.Stopwatch.GetTimestamp();
                    entry.Adapter.Invoke(entry.Item);
                    RecordBenchmark(entry.ActualType, System.Diagnostics.Stopwatch.GetTimestamp() - started);
                }
                else
                {
                    entry.Adapter.Invoke(entry.Item);
                }
            }

            FinishBenchmarkIfNeeded();
            return false;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
        catch (Exception ex)
        {
            LogOnce(
                $"dispatch:{owner.GetType().FullName}:{listFieldName}:{ex.GetType().FullName}:{ex.Message}",
                $"warning: optimizer dispatch failed before safe completion: {ex.GetType().Name}: {ex.Message}; falling back to vanilla");
            return true;
        }
    }

    private static DispatchPlan? GetDispatchPlan(
        object owner,
        IList list,
        string listFieldName,
        string? actualFieldName,
        string invokeMethodName,
        string? enabledMemberName)
    {
        var ownerPlans = DispatchPlans.GetOrCreateValue(owner);

        lock (ownerPlans.Gate)
        {
            if (ownerPlans.Plans.TryGetValue(listFieldName, out var cached)
                && cached.SettingsGeneration == _settingsGeneration
                && cached.ListCount == list.Count
                && SameBoundaryItems(cached, list))
            {
                return cached;
            }

            var entries = new List<DispatchEntry>(list.Count);
            var hasThrottle = false;

            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item is null)
                {
                    continue;
                }

                var adapter = GetAdapter(
                    item.GetType(),
                    actualFieldName,
                    invokeMethodName,
                    enabledMemberName);

                if (adapter is null)
                {
                    LogOnce(
                        $"unsupported:{item.GetType().FullName}:{listFieldName}",
                        $"warning: cannot adapt {item.GetType().FullName} in {listFieldName}; falling back to vanilla");
                    return null;
                }

                var actual = adapter.GetActual(item);
                if (actual is null)
                {
                    continue;
                }

                var actualType = actual.GetType();
                Discover(actualType);

                if (GetInterval(actualType) > 1)
                {
                    hasThrottle = true;
                }

                entries.Add(new DispatchEntry(item, adapter, actual, actualType));
            }

            var plan = new DispatchPlan(
                _settingsGeneration,
                list.Count,
                list.Count > 0 ? list[0] : null,
                list.Count > 1 ? list[list.Count - 1] : null,
                hasThrottle,
                entries.ToArray());

            ownerPlans.Plans[listFieldName] = plan;
            return plan;
        }
    }

    private static bool SameBoundaryItems(DispatchPlan plan, IList list)
    {
        if (list.Count == 0)
        {
            return true;
        }

        if (!ReferenceEquals(plan.FirstItem, list[0]))
        {
            return false;
        }

        return list.Count <= 1 || ReferenceEquals(plan.LastItem, list[list.Count - 1]);
    }

    private static ItemAdapter? GetAdapter(Type itemType, string? actualFieldName, string methodName, string? enabledMemberName)
    {
        var cacheKey = $"{itemType.AssemblyQualifiedName}|{actualFieldName}|{methodName}|{enabledMemberName}";

        lock (Sync)
        {
            if (AdapterCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            if (UnsupportedAdapters.Contains(cacheKey))
            {
                return null;
            }

            FieldInfo? actualField = null;
            if (!string.IsNullOrEmpty(actualFieldName))
            {
                actualField = AccessTools.Field(itemType, actualFieldName);
                if (actualField is null)
                {
                    UnsupportedAdapters.Add(cacheKey);
                    return null;
                }
            }

            var method = AccessTools.Method(itemType, methodName, Type.EmptyTypes);
            if (method is null && methodName == "StartAndTick")
            {
                method = AccessTools.Method(itemType, "Tick", Type.EmptyTypes);
            }

            if (method is null)
            {
                UnsupportedAdapters.Add(cacheKey);
                return null;
            }

            try
            {
                var itemParameter = Expression.Parameter(typeof(object), "item");
                var typedItem = Expression.Convert(itemParameter, itemType);

                Func<object, object?> getActual;
                if (actualField is not null)
                {
                    var fieldAccess = Expression.Field(typedItem, actualField);
                    var boxedField = Expression.Convert(fieldAccess, typeof(object));
                    getActual = Expression.Lambda<Func<object, object?>>(boxedField, itemParameter).Compile();
                }
                else
                {
                    getActual = item => item;
                }

                var call = Expression.Call(typedItem, method);
                var invoke = Expression.Lambda<Action<object>>(call, itemParameter).Compile();

                Func<object, bool> enabled = _ => true;
                if (!string.IsNullOrEmpty(enabledMemberName))
                {
                    var prop = AccessTools.Property(itemType, enabledMemberName);
                    var field = AccessTools.Field(itemType, enabledMemberName);
                    Expression? enabledExpression = null;

                    if (prop?.PropertyType == typeof(bool) && prop.GetMethod is not null)
                    {
                        enabledExpression = Expression.Property(typedItem, prop);
                    }
                    else if (field?.FieldType == typeof(bool))
                    {
                        enabledExpression = Expression.Field(typedItem, field);
                    }

                    if (enabledExpression is not null)
                    {
                        enabled = Expression.Lambda<Func<object, bool>>(enabledExpression, itemParameter).Compile();
                    }
                }

                var adapter = new ItemAdapter(
                    actualFieldName,
                    enabledMemberName,
                    getActual,
                    invoke,
                    enabled);

                AdapterCache[cacheKey] = adapter;
                return adapter;
            }
            catch
            {
                UnsupportedAdapters.Add(cacheKey);
                return null;
            }
        }
    }

    private static bool ShouldRun(object actual)
    {
        if (IsProtected(actual.GetType()))
        {
            return true;
        }

        var interval = GetInterval(actual.GetType());
        if (interval <= 1)
        {
            return true;
        }

        var state = Counters.GetOrCreateValue(actual);
        if (state.Remaining <= 0)
        {
            state.Remaining = interval - 1;
            return true;
        }

        state.Remaining--;
        return false;
    }

    private static bool IsProtected(Type type) =>
        ProtectedTypeNames.Contains(type.Name);

    private static bool NormalizeProtectedIntervals(Settings settings)
    {
        var changed = false;

        foreach (var key in settings.Intervals.Keys.ToArray())
        {
            if (IsProtected(key) && settings.Intervals[key] != 1)
            {
                settings.Intervals[key] = 1;
                changed = true;
            }
        }

        return changed;
    }

    private static bool HasActiveThrottles()
    {
        if (_settings.DefaultInterval > 1)
        {
            return true;
        }

        return _settings.Intervals.Values.Any(value => value > 1);
    }

    private static int GetInterval(Type type)
    {
        if (IsProtected(type))
        {
            return 1;
        }

        var full = type.FullName ?? type.Name;

        if (_settings.Intervals.TryGetValue(full, out var exact))
        {
            return Math.Clamp(exact, 1, 1000);
        }

        if (_settings.Intervals.TryGetValue(type.Name, out var shortName))
        {
            return Math.Clamp(shortName, 1, 1000);
        }

        return Math.Clamp(_settings.DefaultInterval, 1, 1000);
    }

    private static void Discover(Type type)
    {
        var name = type.FullName ?? type.Name;

        lock (Sync)
        {
            if (Discovered.Contains(name))
            {
                return;
            }
        }

        // Origin resolution can touch paths/manifests. Do it only once per discovered type,
        // never on the per-frame hot path.
        var origin = ResolveOrigin(type);
        var assemblyName = type.Assembly.GetName().Name ?? "";

        lock (Sync)
        {
            if (!Discovered.Add(name))
            {
                return;
            }

            OriginByType[name] = origin;
            AssemblyNameByType[name] = assemblyName;
            DiscoveredTypes[name] = type;

            // Type discovery remains in-memory for live settings; no text dump.
        }
    }

    public static void RegisterModAssemblyOrigin(string assemblyName, string origin)
    {
        if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(origin))
        {
            return;
        }

        lock (Sync)
        {
            OriginByAssemblyName[assemblyName] = origin;

            foreach (var pair in AssemblyNameByType)
            {
                if (string.Equals(pair.Value, assemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    OriginByType[pair.Key] = origin;
                }
            }
        }
    }

    public static void RegisterModPathOrigin(string path, string origin)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(origin))
        {
            return;
        }

        try
        {
            path = Path.GetFullPath(path);
        }
        catch
        {
            return;
        }

        lock (Sync)
        {
            OriginByAssemblyPath[path] = origin;
        }
    }

    private static string ResolveOrigin(Type type)
    {
        string location;
        try
        {
            location = type.Assembly.Location ?? "";
        }
        catch
        {
            location = "";
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            var assemblyName = type.Assembly.GetName().Name ?? "";
            lock (Sync)
            {
                return OriginByAssemblyName.TryGetValue(assemblyName, out var mapped)
                    ? mapped
                    : $"UNKNOWN: {assemblyName}";
            }
        }

        var fullPath = Path.GetFullPath(location);
        lock (Sync)
        {
            if (OriginByAssemblyPath.TryGetValue(fullPath, out var cached))
            {
                return cached;
            }
        }

        var normalized = fullPath.Replace('\\', '/');
        string origin;

        lock (Sync)
        {
            if (OriginByAssemblyPath.TryGetValue(fullPath, out var exactPathOrigin))
            {
                return exactPathOrigin;
            }

            var assemblyName = type.Assembly.GetName().Name ?? "";
            if (OriginByAssemblyName.TryGetValue(assemblyName, out var assemblyOrigin))
            {
                return assemblyOrigin;
            }
        }

        if (normalized.Contains("/Timberborn_Data/Managed/", StringComparison.OrdinalIgnoreCase))
        {
            origin = "CORE";
        }
        else
        {
            var manifest = FindNearestManifest(fullPath);
            if (manifest is not null)
            {
                origin = DescribeModOrigin(manifest, normalized);
            }
            else
            {
                var workshopMarker = "/steamapps/workshop/content/1062090/";
                var workshopIndex = normalized.IndexOf(workshopMarker, StringComparison.OrdinalIgnoreCase);
                if (workshopIndex >= 0)
                {
                    var rest = normalized[(workshopIndex + workshopMarker.Length)..];
                    var workshopId = rest.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "?";
                    origin = $"MOD: Workshop {workshopId}";
                }
                else
                {
                    var localMarker = "/Timberborn/Mods/";
                    var localIndex = normalized.IndexOf(localMarker, StringComparison.OrdinalIgnoreCase);
                    if (localIndex >= 0)
                    {
                        var rest = normalized[(localIndex + localMarker.Length)..];
                        var folder = rest.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "local";
                        origin = $"MOD: {folder} (local)";
                    }
                    else
                    {
                        origin = $"OTHER: {type.Assembly.GetName().Name}";
                    }
                }
            }
        }

        lock (Sync)
        {
            OriginByAssemblyPath[fullPath] = origin;
        }

        return origin;
    }

    private static string? FindNearestManifest(string assemblyPath)
    {
        try
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(assemblyPath)!);
            for (var depth = 0; directory is not null && depth < 6; depth++, directory = directory.Parent)
            {
                var manifest = Path.Combine(directory.FullName, "manifest.json");
                if (File.Exists(manifest))
                {
                    return manifest;
                }
            }
        }
        catch
        {
            // Origin enrichment is diagnostic only.
        }

        return null;
    }

    private static string DescribeModOrigin(string manifestPath, string normalizedAssemblyPath)
    {
        try
        {
            var json = File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<Dictionary<string, object?>>(json);
            manifest ??= new Dictionary<string, object?>();

            var name = manifest.TryGetValue("Name", out var nameValue)
                ? Convert.ToString(nameValue)
                : null;
            var id = manifest.TryGetValue("Id", out var idValue)
                ? Convert.ToString(idValue)
                : null;

            var workshopMarker = "/steamapps/workshop/content/1062090/";
            var workshopIndex = normalizedAssemblyPath.IndexOf(workshopMarker, StringComparison.OrdinalIgnoreCase);
            string? suffix = null;

            if (workshopIndex >= 0)
            {
                var rest = normalizedAssemblyPath[(workshopIndex + workshopMarker.Length)..];
                var workshopId = rest.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(workshopId))
                {
                    suffix = $"Workshop {workshopId}";
                }
            }
            else if (normalizedAssemblyPath.Contains("/Timberborn/Mods/", StringComparison.OrdinalIgnoreCase))
            {
                suffix = "local";
            }

            var display = !string.IsNullOrWhiteSpace(name)
                ? name!
                : !string.IsNullOrWhiteSpace(id) ? id! : Path.GetFileName(Path.GetDirectoryName(manifestPath));

            if (!string.IsNullOrWhiteSpace(id) && !string.Equals(display, id, StringComparison.Ordinal))
            {
                display += $" ({id})";
            }

            return suffix is null
                ? $"MOD: {display}"
                : $"MOD: {display} [{suffix}]";
        }
        catch
        {
            return normalizedAssemblyPath.Contains("/Timberborn/Mods/", StringComparison.OrdinalIgnoreCase)
                ? "MOD: local"
                : "MOD";
        }
    }

    private static bool BenchmarkActive =>
        _benchmarkRunning && !_benchmarkCompleted;

    private static void RecordBenchmark(Type type, long elapsedTicks)
    {
        var name = type.FullName ?? type.Name;

        lock (Sync)
        {
            if (!BenchStats.TryGetValue(name, out var stat))
            {
                stat = new BenchStat { Origin = ResolveOrigin(type) };
                BenchStats[name] = stat;
            }

            stat.Count++;
            stat.TotalTicks += elapsedTicks;
            if (elapsedTicks < stat.MinTicks) stat.MinTicks = elapsedTicks;
            if (elapsedTicks > stat.MaxTicks) stat.MaxTicks = elapsedTicks;
        }

    }

    public static string ResolveDiagnosticOrigin(Type type) => ResolveOrigin(type);

    public static void RecordDiagnosticBenchmark(string name, string origin, long elapsedTicks)
    {
        if (!BenchmarkActive || elapsedTicks < 0)
        {
            return;
        }

        lock (Sync)
        {
            if (!BenchStats.TryGetValue(name, out var stat))
            {
                stat = new BenchStat { Origin = origin };
                BenchStats[name] = stat;
            }

            stat.Count++;
            stat.TotalTicks += elapsedTicks;
            if (elapsedTicks < stat.MinTicks) stat.MinTicks = elapsedTicks;
            if (elapsedTicks > stat.MaxTicks) stat.MaxTicks = elapsedTicks;
        }

    }

    private static void FinishBenchmarkIfNeeded()
    {
        if (!BenchmarkActive)
        {
            return;
        }

        var elapsed = (System.Diagnostics.Stopwatch.GetTimestamp() - _benchmarkStart)
                      / (double)System.Diagnostics.Stopwatch.Frequency;
        if (elapsed < BenchmarkDurationSeconds)
        {
            return;
        }

        _benchmarkCompleted = true;
        _benchmarkRunning = false;

        var file = Path.Combine(ModPath, $"optimizer-v11-benchmark-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        var lines = new List<string> { "Type,Origin,TotalMs,Count,AverageMs,MinMs,MaxMs" };

        lock (Sync)
        {
            foreach (var pair in BenchStats.OrderByDescending(x => x.Value.TotalTicks))
            {
                var s = pair.Value;
                var totalMs = TicksToMs(s.TotalTicks);
                var minMs = s.Count == 0 ? 0 : TicksToMs(s.MinTicks);
                var maxMs = s.Count == 0 ? 0 : TicksToMs(s.MaxTicks);
                var avgMs = s.Count == 0 ? 0 : totalMs / s.Count;
                lines.Add($"{Csv(pair.Key)},{Csv(s.Origin)},{totalMs:F6},{s.Count},{avgMs:F6},{minMs:F6},{maxMs:F6}");
            }
        }

        File.WriteAllLines(file, lines);

        FrameProfileSampler.StopAndWrite(_harmony, ModPath);

        if (_harmony is not null)
        {
            TimberPhysicsBenchmarkPatcher.Unpatch(_harmony);
            DispatcherPatcher.Unpatch(_harmony);

            // Return to the configured interval patches after observational CSV benchmarking.
            RefreshDirectThrottlePatches();
        }

        Log($"benchmark complete: {file}");
    }

    private static double TicksToMs(long ticks) =>
        ticks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;

    private static string Csv(string value) =>
        "\"" + value.Replace("\"", "\"\"") + "\"";

    private static void ReloadConfig(bool force)
    {
        var now = DateTime.UtcNow;
        if (!force && now < _nextReloadUtc)
        {
            return;
        }

        _nextReloadUtc = now.AddSeconds(Math.Max(1, _settings.ReloadSeconds));

        try
        {
            if (!File.Exists(ConfigPath))
            {
                return;
            }

            var writeUtc = File.GetLastWriteTimeUtc(ConfigPath);
            if (!force && writeUtc == _configWriteUtc)
            {
                return;
            }

            var json = File.ReadAllText(ConfigPath);
            var loaded = JsonConvert.DeserializeObject<Settings>(json);
            if (loaded is null)
            {
                return;
            }

            loaded.Normalize();
            var soilGcReliefApplied = ApplySoilContaminationGcReliefMigration(loaded);
            var soilGcReliefV2Applied = ApplySoilContaminationGcReliefV2Migration(loaded);
            var protectedValuesCorrected = NormalizeProtectedIntervals(loaded);
            _settings = loaded;
            _settingsGeneration++;
            _configWriteUtc = writeUtc;

            if (protectedValuesCorrected || soilGcReliefApplied || soilGcReliefV2Applied)
            {
                SaveSettings(_settings);
                if (protectedValuesCorrected)
                {
                    Log("protected optimizer intervals found in config and reset to 1");
                }
                if (soilGcReliefApplied)
                {
                    Log("v1.1.62 soil-GC relief migration applied as compatibility step");
                }
                if (soilGcReliefV2Applied)
                {
                    var interval = GetInterval("Timberborn.SoilContaminationSystem.SoilContaminationService");
                    Log(
                        $"v1.1.63 soil-GC relief migration finalized: SoilContaminationService interval={interval}; " +
                        "only the v1.1.62 default value of 2 is upgraded to 4; other user-selected values are preserved");
                }
            }

            if (!BenchmarkActive)
            {
                RefreshDirectThrottlePatches();
            }
            Log($"config reloaded: {_settings.Intervals.Count} explicit interval(s)");
        }
        catch (Exception ex)
        {
            Log($"warning: config reload failed: {ex.GetType().Name}: {ex.Message}; keeping previous settings");
        }
    }

    private static bool ApplySoilContaminationGcReliefMigration(Settings settings)
    {
        if (settings.SoilContaminationGcReliefApplied)
        {
            return false;
        }

        const string fullName = "Timberborn.SoilContaminationSystem.SoilContaminationService";
        const string shortName = "SoilContaminationService";

        var existingKey = settings.Intervals.Keys.FirstOrDefault(key =>
            string.Equals(key, fullName, StringComparison.Ordinal) ||
            string.Equals(key, shortName, StringComparison.Ordinal) ||
            key.EndsWith("." + shortName, StringComparison.Ordinal));

        if (existingKey is null)
        {
            settings.Intervals[fullName] = 2;
        }
        else if (settings.Intervals[existingKey] <= 1)
        {
            settings.Intervals[existingKey] = 2;
        }

        settings.SoilContaminationGcReliefApplied = true;
        return true;
    }

    private static bool ApplySoilContaminationGcReliefV2Migration(Settings settings)
    {
        if (settings.SoilContaminationGcReliefV2Applied)
        {
            return false;
        }

        const string fullName = "Timberborn.SoilContaminationSystem.SoilContaminationService";
        const string shortName = "SoilContaminationService";

        var existingKey = settings.Intervals.Keys.FirstOrDefault(key =>
            string.Equals(key, fullName, StringComparison.Ordinal) ||
            string.Equals(key, shortName, StringComparison.Ordinal) ||
            key.EndsWith("." + shortName, StringComparison.Ordinal));

        // Upgrade only the v1.1.62 default. A user who already changed the value
        // explicitly keeps that value.
        if (existingKey is not null && settings.Intervals[existingKey] == 2)
        {
            settings.Intervals[existingKey] = 4;
        }
        else if (existingKey is null && !settings.SoilContaminationGcReliefApplied)
        {
            settings.Intervals[fullName] = 4;
        }

        settings.SoilContaminationGcReliefV2Applied = true;
        return true;
    }

    private static void SaveSettings(Settings settings)
    {
        NormalizeProtectedIntervals(settings);
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(ConfigPath, json + Environment.NewLine);
        _configWriteUtc = File.GetLastWriteTimeUtc(ConfigPath);
    }


    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void Log(string message)
    {
        // Intentionally empty. The Conditional attribute removes all call
        // sites and their interpolation/formatting work at compile time.
    }

    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void LogFreeze(string message)
    {
        // All freeze log writes and detectors are disabled.
    }

    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    private static void LogOnce(string key, string message)
    {
        // Diagnostics are retired; no warning tracking or disk I/O.
    }

    private static void ResetSessionLogs()
    {
        // Do not touch pre-existing log files and do not create new ones.
    }

    private sealed class CounterState
    {
        public int Remaining;
    }

    private sealed class OwnerDispatchPlans
    {
        public object Gate { get; } = new();
        public Dictionary<string, DispatchPlan> Plans { get; } = new(StringComparer.Ordinal);
    }

    private sealed record DispatchPlan(
        int SettingsGeneration,
        int ListCount,
        object? FirstItem,
        object? LastItem,
        bool HasThrottle,
        DispatchEntry[] Entries);

    private sealed record DispatchEntry(
        object Item,
        ItemAdapter Adapter,
        object Actual,
        Type ActualType);

    private sealed class BenchStat
    {
        public string Origin = "UNKNOWN";
        public long Count;
        public long TotalTicks;
        public long MinTicks = long.MaxValue;
        public long MaxTicks;
    }

    private sealed record ItemAdapter(
        string? ActualFieldName,
        string? EnabledMemberName,
        Func<object, object?> ActualGetter,
        Action<object> Invoke,
        Func<object, bool> Enabled)
    {
        public object? GetActual(object item) => ActualGetter(item);
        public bool IsEnabled(object item) => Enabled(item);
    }
}
internal sealed class Settings
{
    public bool Enabled { get; set; } = true;
    public int DefaultInterval { get; set; } = 1;
    public int ReloadSeconds { get; set; } = 2;
    public bool WriteDiscoveredTypes { get; set; } = true;

    // One-time v1.1.62 migration.
    public bool SoilContaminationGcReliefApplied { get; set; } = false;

    // One-time v1.1.63 migration. Only upgrades the previous default of 2 -> 4.
    public bool SoilContaminationGcReliefV2Applied { get; set; } = false;

    // 0 = disabled. Set e.g. 30 to benchmark the first 30 real seconds after loading a game.
    public int BenchmarkSeconds { get; set; } = 0;

    // Key may be a full type name (preferred) or a short class name.
    // 1 = vanilla frequency; 20 = run once, then skip the next 19 dispatches.
    public Dictionary<string, int> Intervals { get; set; } = new(StringComparer.Ordinal);

    public static Settings CreateDefault() => new()
    {
        Enabled = true,
        DefaultInterval = 1,
        ReloadSeconds = 2,
        WriteDiscoveredTypes = true,
        SoilContaminationGcReliefApplied = true,
        SoilContaminationGcReliefV2Applied = true,
        BenchmarkSeconds = 0,
        Intervals = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Timberborn.SoilContaminationSystem.SoilContaminationService"] = 4,
            ["ConstructionSite"] = 1,
            ["ResourceCountingService"] = 1,
            ["BehaviorManager"] = 1,
            ["NavMeshObserver"] = 1
        }
    };

    public void Normalize()
    {
        DefaultInterval = Math.Clamp(DefaultInterval, 1, 1000);
        ReloadSeconds = Math.Clamp(ReloadSeconds, 1, 60);
        // Legacy field kept only for backwards-compatible config parsing.
        // Benchmark execution is session-only.
        BenchmarkSeconds = 0;
        Intervals ??= new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var key in Intervals.Keys.ToArray())
        {
            Intervals[key] = Math.Clamp(Intervals[key], 1, 1000);
        }
    }
}
