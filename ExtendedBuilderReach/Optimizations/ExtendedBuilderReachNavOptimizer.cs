using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace ExtendedBuilderReach.Optimizations;

internal static class ExtendedBuilderReachNavOptimizer
{
    private const string EbrTypeName =
        "ExtendedBuilderReach.Components.ExtendedDemolishableAccessible";
    private const string RegistryTypeName =
        "Timberborn.Navigation.NavMeshListenerEntityRegistry";
    private const int BucketSize = 8;
    private const int LargeUpdateCandidateThreshold = 1024;
    private const int DeferredDrainMaxListenersPerFrame = 256;
    private const double DeferredDrainBudgetMs = 8.0;

    private static Type? _ebrType;
    private static FieldInfo? _registryListenersField;
    private static FieldInfo? _listenerBoundsField;
    private static FieldInfo? _boundsMinX;
    private static FieldInfo? _boundsMinY;
    private static FieldInfo? _boundsMaxX;
    private static FieldInfo? _boundsMaxY;
    private static PropertyInfo? _terrainCoordinatesProperty;
    private static PropertyInfo? _updatedRoadsProperty;
    private static PropertyInfo? _updateBoundsProperty;
    private static MemberInfo? _coordX;
    private static MemberInfo? _coordY;
    // Avoid repeated reflection/boxing for public int x/y fields or properties.
    // Other coordinate shapes retain the original implementation.
    private static Func<object, int>? _fastX;
    private static Func<object, int>? _fastY;
    private static Func<object, object, bool>? _intersects;
    private static Action<object, object>? _notify;

    private static readonly List<object> EbrListeners = new();
    private static readonly HashSet<object> ActiveListeners = new(ReferenceComparer.Instance);
    private static readonly Dictionary<long, List<object>> SpatialBuckets = new();
    private static readonly ConditionalWeakTable<object, BucketMembership> Memberships = new();
    private static readonly ConditionalWeakTable<object, MigrationMarker> MigratedRegistries = new();
    private static readonly HashSet<object> CandidateSet = new(ReferenceComparer.Instance);
    private static readonly Queue<PendingBatch> DeferredBatches = new();
    private static int _deferredCandidates;
    private static bool _deferredModeLogged;
    private static bool _installed;

    private sealed class MigrationMarker
    {
    }

    private sealed class BucketMembership
    {
        public List<long> Keys { get; } = new();
    }

    private sealed class PendingBatch
    {
        public PendingBatch(object update, object[] candidates, int candidateCount, int total, int scanned, bool usedSpatial)
        {
            Update = update;
            Candidates = candidates;
            CandidateCount = candidateCount;
            Total = total;
            Scanned = scanned;
            UsedSpatial = usedSpatial;
        }

        public object Update { get; }
        public object[] Candidates { get; }
        public int CandidateCount { get; }
        public int Total { get; }
        public int Scanned { get; }
        public bool UsedSpatial { get; }
        public int Index { get; set; }
        public int Notified { get; set; }
    }

    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceComparer Instance = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    public static void Patch(Harmony harmony)
    {
        if (_installed)
        {
            return;
        }

        var registryType = AccessTools.TypeByName(RegistryTypeName);
        _ebrType = AccessTools.TypeByName(EbrTypeName);
        if (registryType is null || _ebrType is null)
        {
            NavOptimizerDiagnostics.Log(
                $"EBR nav optimizer not installed: registry={registryType is not null}, " +
                $"ExtendedDemolishableAccessible={_ebrType is not null}");
            return;
        }

        // Do not run two EBR dispatchers against the same global registry.
        // An older external optimizer may already own these callbacks.
        var oldOwner = "tharbad08.TBOptimizer";
        var oldStandaloneOwner = "tharbad08.TBEBROptimizations";
        var existingNotify = AccessTools.Method(registryType, "NotifyAll");
        var existingPatches = existingNotify is null ? null : Harmony.GetPatchInfo(existingNotify);
        if (existingPatches is not null &&
            existingPatches.Owners.Any(owner =>
                owner == oldOwner || owner == oldStandaloneOwner))
        {
            NavOptimizerDiagnostics.Log(
                "EBR navigation optimization skipped: another EBR optimizer is installed");
            return;
        }

        var register = AccessTools.Method(registryType, "RegisterNavMeshListener");
        var unregister = AccessTools.Method(registryType, "UnregisterNavMeshListener");
        var notifyAll = AccessTools.Method(registryType, "NotifyAll");
        _registryListenersField = AccessTools.Field(registryType, "_navMeshListeners");

        if (register is null ||
            unregister is null ||
            notifyAll is null ||
            _registryListenersField is null)
        {
            NavOptimizerDiagnostics.Log(
                "warning: EBR nav optimizer unavailable: required registry members not found");
            return;
        }

        var updateType = notifyAll.GetParameters().FirstOrDefault()?.ParameterType;
        _listenerBoundsField = AccessTools.Field(_ebrType, "bounds");
        var updateBounds = updateType is null ? null : AccessTools.Property(updateType, "Bounds");
        _updateBoundsProperty = updateBounds;
        _terrainCoordinatesProperty =
            updateType is null ? null : AccessTools.Property(updateType, "TerrainCoordinates");
        _updatedRoadsProperty =
            updateType is null ? null : AccessTools.Property(updateType, "UpdatedRoads");
        var notifyMethod = updateType is null
            ? null
            : AccessTools.Method(_ebrType, "OnNavMeshUpdated", new[] { updateType });

        if (updateType is null ||
            _listenerBoundsField is null ||
            updateBounds?.GetGetMethod(true) is null ||
            notifyMethod is null)
        {
            NavOptimizerDiagnostics.Log(
                "warning: EBR nav optimizer unavailable: EBR/update bounds members not resolved");
            return;
        }

        var boundsType = _listenerBoundsField.FieldType;
        var minX = AccessTools.Field(boundsType, "_minX");
        var minY = AccessTools.Field(boundsType, "_minY");
        var minZ = AccessTools.Field(boundsType, "_minZ");
        var maxX = AccessTools.Field(boundsType, "_maxX");
        var maxY = AccessTools.Field(boundsType, "_maxY");
        var maxZ = AccessTools.Field(boundsType, "_maxZ");

        if (minX is null || minY is null || minZ is null ||
            maxX is null || maxY is null || maxZ is null)
        {
            NavOptimizerDiagnostics.Log(
                "warning: EBR nav optimizer unavailable: BoundingBox fields not resolved");
            return;
        }

        _boundsMinX = minX;
        _boundsMinY = minY;
        _boundsMaxX = maxX;
        _boundsMaxY = maxY;

        var terrainCoordinatesType =
            _terrainCoordinatesProperty?.PropertyType.IsGenericType == true
                ? _terrainCoordinatesProperty.PropertyType.GetGenericArguments().FirstOrDefault()
                : null;
        if (terrainCoordinatesType is not null)
        {
            _coordX = (MemberInfo?)AccessTools.Field(terrainCoordinatesType, "x")
                ?? AccessTools.Property(terrainCoordinatesType, "x");
            _coordY = (MemberInfo?)AccessTools.Field(terrainCoordinatesType, "y")
                ?? AccessTools.Property(terrainCoordinatesType, "y");
            // Compilation is opportunistic; reflection fallback is always available.
            try
            {
                _fastX = CreateCoordinateReader(terrainCoordinatesType, _coordX);
                _fastY = CreateCoordinateReader(terrainCoordinatesType, _coordY);
            }
            catch (Exception ex)
            {
                _fastX = null;
                _fastY = null;
                NavOptimizerDiagnostics.Log($"EBR coordinate accessor fallback: {ex.GetType().Name}: {ex.Message}");
            }
        }

        try
        {
            _intersects = CreateIntersectsDelegate(
                _ebrType,
                updateType,
                _listenerBoundsField,
                updateBounds.GetGetMethod(true)!,
                boundsType,
                minX,
                minY,
                minZ,
                maxX,
                maxY,
                maxZ);
            _notify = CreateNotifyDelegate(_ebrType, updateType, notifyMethod);

            // NotifyAll must be intercepted successfully before suppressing
            // registration, otherwise listeners could disappear from updates.
            harmony.Patch(
                notifyAll,
                prefix: new HarmonyMethod(
                    AccessTools.Method(
                        typeof(ExtendedBuilderReachNavOptimizer),
                        nameof(NotifyPrefix)))
                {
                    priority = Priority.First
                },
                postfix: new HarmonyMethod(
                    AccessTools.Method(
                        typeof(ExtendedBuilderReachNavOptimizer),
                        nameof(NotifyPostfix)))
                {
                    priority = Priority.Last
                });

            harmony.Patch(
                register,
                prefix: new HarmonyMethod(
                    AccessTools.Method(
                        typeof(ExtendedBuilderReachNavOptimizer),
                        nameof(RegisterPrefix)))
                {
                    priority = Priority.First
                });
            harmony.Patch(
                unregister,
                prefix: new HarmonyMethod(
                    AccessTools.Method(
                        typeof(ExtendedBuilderReachNavOptimizer),
                        nameof(UnregisterPrefix)))
                {
                    priority = Priority.First
                });
            _installed = true;
            NavOptimizerDiagnostics.Log(
                $"Extended Builder Reach nav optimizer installed: {BucketSize}x{BucketSize} " +
                "spatial buckets + exact terrain/road-bounds candidate selection; pooled large-update arrays and fast coordinate readers when supported; " +
                $"updates with >= {LargeUpdateCandidateThreshold} EBR candidates are deferred " +
                $"and drained at <= {DeferredDrainMaxListenersPerFrame} listeners / {DeferredDrainBudgetMs:F0}ms per frame; " +
                "exact intersection check retained before notifying listeners");
        }
        catch (Exception ex)
        {
            // Fail closed: remove any interceptors installed by this instance.
            // Original Register/Unregister/NotifyAll will then remain effective.
            try
            {
                harmony.Unpatch(register, HarmonyPatchType.All, harmony.Id);
                harmony.Unpatch(unregister, HarmonyPatchType.All, harmony.Id);
                harmony.Unpatch(notifyAll, HarmonyPatchType.All, harmony.Id);
            }
            catch { /* Keep startup alive; installation errors are reported below. */ }
            _intersects = null;
            _notify = null;
            NavOptimizerDiagnostics.Log(
                $"warning: EBR nav optimizer installation failed; vanilla registry retained: " +
                $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool RegisterPrefix(object __0)
    {
        if (_installed && IsEbr(__0))
        {
            Add(__0);
            return false;
        }

        return true;
    }

    private static bool UnregisterPrefix(object __0)
    {
        if (_installed && IsEbr(__0))
        {
            Remove(__0);
            return false;
        }

        return true;
    }

    private static void NotifyPrefix(object __instance)
    {
        if (!_installed ||
            _registryListenersField is null ||
            MigratedRegistries.TryGetValue(__instance, out _))
        {
            return;
        }

        if (_registryListenersField.GetValue(__instance) is IList listeners)
        {
            var migrated = 0;
            for (var i = listeners.Count - 1; i >= 0; i--)
            {
                var listener = listeners[i];
                if (listener is not null && IsEbr(listener))
                {
                    Add(listener);
                    listeners.RemoveAt(i);
                    migrated++;
                }
            }

            if (migrated > 0)
            {
                NavOptimizerDiagnostics.Log(
                    $"EBR nav optimizer migrated {migrated} pre-registered " +
                    "ExtendedDemolishableAccessible listener(s) into spatial buckets");
            }
        }

        MigratedRegistries.Add(__instance, new MigrationMarker());
    }

    private static void NotifyPostfix(object __0)
    {
        if (!_installed || _intersects is null || _notify is null)
        {
            return;
        }

        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        var lookupStarted = started;
        var total = EbrListeners.Count;
        var notified = 0;
        var scanned = 0;
        var usedSpatial = false;

        CandidateSet.Clear();

        var roadsUpdated = false;
        if (_updatedRoadsProperty is not null)
        {
            try
            {
                roadsUpdated = Convert.ToBoolean(_updatedRoadsProperty.GetValue(__0));
            }
            catch
            {
                roadsUpdated = true;
            }
        }

        if (roadsUpdated && _updateBoundsProperty is not null)
        {
            try
            {
                var updateBounds = _updateBoundsProperty.GetValue(__0);
                if (updateBounds is not null)
                {
                    AddCandidatesForBounds(updateBounds);
                    usedSpatial = true;
                }
            }
            catch
            {
                // If a modded update object exposes an unexpected Bounds shape,
                // preserve correctness by falling back to the full EBR list below.
            }
        }

        if (!usedSpatial &&
            _terrainCoordinatesProperty is not null &&
            _coordX is not null &&
            _coordY is not null &&
            _terrainCoordinatesProperty.GetValue(__0) is IEnumerable coordinates)
        {
            var coordinateCount = 0;
            foreach (var coordinate in coordinates)
            {
                if (coordinate is null)
                {
                    continue;
                }

                coordinateCount++;
                var x = _fastX is null
                    ? Convert.ToInt32(ReadMember(coordinate, _coordX))
                    : _fastX(coordinate);
                var y = _fastY is null
                    ? Convert.ToInt32(ReadMember(coordinate, _coordY))
                    : _fastY(coordinate);
                var key = BucketKey(FloorDiv(x, BucketSize), FloorDiv(y, BucketSize));
                if (!SpatialBuckets.TryGetValue(key, out var bucket))
                {
                    continue;
                }

                foreach (var listener in bucket)
                {
                    CandidateSet.Add(listener);
                }
            }

            usedSpatial = coordinateCount > 0;
        }

        // Measure lookup separately from intersections, callbacks and snapshotting.
        

        object[]? deferredCandidates = null;
        var deferredCount = 0;

        if (usedSpatial)
        {
            scanned = CandidateSet.Count;
            if (scanned >= LargeUpdateCandidateThreshold)
            {
                deferredCount = scanned;
                deferredCandidates = System.Buffers.ArrayPool<object>.Shared.Rent(scanned);
                CandidateSet.CopyTo(deferredCandidates);
            }
            else
            {
                foreach (var listener in CandidateSet)
                {
                    if (_intersects!(listener, __0))
                    {
                        _notify!(listener, __0);
                        notified++;
                    }
                }
            }
        }
        else
        {
            scanned = total;
            if (scanned >= LargeUpdateCandidateThreshold)
            {
                deferredCount = scanned;
                deferredCandidates = System.Buffers.ArrayPool<object>.Shared.Rent(scanned);
                EbrListeners.CopyTo(deferredCandidates);
            }
            else
            {
                for (var i = 0; i < EbrListeners.Count; i++)
                {
                    var listener = EbrListeners[i];
                    if (_intersects!(listener, __0))
                    {
                        _notify!(listener, __0);
                        notified++;
                    }
                }
            }
        }

        if (deferredCandidates is not null)
        {
            DeferredBatches.Enqueue(
                new PendingBatch(__0, deferredCandidates, deferredCount, total, scanned, usedSpatial));
            _deferredCandidates += deferredCount;

            if (!_deferredModeLogged)
            {
                _deferredModeLogged = true;
                NavOptimizerDiagnostics.Log(
                    $"EBR large-update deferral activated: threshold={LargeUpdateCandidateThreshold}, " +
                    $"per-frame budget={DeferredDrainMaxListenersPerFrame} listeners/{DeferredDrainBudgetMs:F0}ms");
            }

NavOptimizerDiagnostics.RecordNavigationDetail(
                usedSpatial
                    ? $"EBR.DeferredQueue[{total}~{scanned}]"
                    : $"EBR.DeferredFullQueue[{total}]",
                System.Diagnostics.Stopwatch.GetTimestamp() - started);
            return;
        }

        NavOptimizerDiagnostics.RecordNavigationDetail(
            usedSpatial
                ? $"EBR.SpatialDispatch[{total}~{scanned}->{notified}]"
                : $"EBR.FullDispatch[{total}->{notified}]",
            System.Diagnostics.Stopwatch.GetTimestamp() - started);
    }

    public static void DrainDeferred()
    {
        if (!_installed ||
            _intersects is null ||
            _notify is null ||
            DeferredBatches.Count == 0)
        {
            return;
        }

        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        var budgetTicks = (long)Math.Ceiling(
            DeferredDrainBudgetMs * System.Diagnostics.Stopwatch.Frequency / 1000.0);
        var processed = 0;
        var notified = 0;
        var failed = 0;

        while (DeferredBatches.Count > 0 &&
               processed < DeferredDrainMaxListenersPerFrame &&
               System.Diagnostics.Stopwatch.GetTimestamp() - started < budgetTicks)
        {
            var batch = DeferredBatches.Peek();

            if (batch.Index >= batch.CandidateCount)
            {
                DeferredBatches.Dequeue();
                // Clear references before returning the pool rental.
                System.Buffers.ArrayPool<object>.Shared.Return(batch.Candidates, clearArray: true);
continue;
            }

            var listener = batch.Candidates[batch.Index++];
            processed++;
            _deferredCandidates = Math.Max(0, _deferredCandidates - 1);

            if (!ActiveListeners.Contains(listener))
            {
                continue;
            }

            try
            {
                if (_intersects!(listener, batch.Update))
                {
                    _notify!(listener, batch.Update);
                    batch.Notified++;
                    notified++;
                }
            }
            catch (Exception ex)
            {
                failed++;
                NavOptimizerDiagnostics.Log(
                    $"warning: deferred EBR notification failed for {listener.GetType().FullName}: " +
                    $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        var elapsedTicks = System.Diagnostics.Stopwatch.GetTimestamp() - started;
        NavOptimizerDiagnostics.RecordNavigationDetail(
            $"EBR.DeferredDrain[{processed}->{notified};fail={failed};pending={_deferredCandidates}]",
            elapsedTicks);
    }

    private static bool IsEbr(object listener) =>
        _ebrType is not null && listener.GetType() == _ebrType;

    private static void Add(object listener)
    {
        if (!ActiveListeners.Add(listener))
        {
            return;
        }

        EbrListeners.Add(listener);
        AddToSpatialBuckets(listener);
    }

    private static void Remove(object listener)
    {
        ActiveListeners.Remove(listener);

        for (var i = EbrListeners.Count - 1; i >= 0; i--)
        {
            if (ReferenceEquals(EbrListeners[i], listener))
            {
                EbrListeners.RemoveAt(i);
                break;
            }
        }

        RemoveFromSpatialBuckets(listener);

    }

    // EBR normally keeps bounds fixed, but reach settings or game-state changes
    // may regenerate them. Stale bucket membership would silently miss updates.
    // Reindex only when the component reports an actual bounds change.
    internal static void BoundsChanged(object listener)
    {
        if (!_installed || !ActiveListeners.Contains(listener))
            return;

        RemoveFromSpatialBuckets(listener);
        AddToSpatialBuckets(listener);
    }

    private static void RemoveFromSpatialBuckets(object listener)
    {
        if (Memberships.TryGetValue(listener, out var membership))
        {
            foreach (var key in membership.Keys)
            {
                if (!SpatialBuckets.TryGetValue(key, out var bucket))
                {
                    continue;
                }

                for (var i = bucket.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(bucket[i], listener))
                    {
                        bucket.RemoveAt(i);
                    }
                }

                if (bucket.Count == 0)
                {
                    SpatialBuckets.Remove(key);
                }
            }

            Memberships.Remove(listener);
        }
    }

    private static void AddToSpatialBuckets(object listener)
    {
        if (_listenerBoundsField is null ||
            _boundsMinX is null ||
            _boundsMinY is null ||
            _boundsMaxX is null ||
            _boundsMaxY is null)
        {
            return;
        }

        var bounds = _listenerBoundsField.GetValue(listener);
        if (bounds is null)
        {
            return;
        }

        var minX = Convert.ToInt32(_boundsMinX.GetValue(bounds));
        var minY = Convert.ToInt32(_boundsMinY.GetValue(bounds));
        var maxX = Convert.ToInt32(_boundsMaxX.GetValue(bounds));
        var maxY = Convert.ToInt32(_boundsMaxY.GetValue(bounds));

        var membership = new BucketMembership();
        for (var bucketY = FloorDiv(minY, BucketSize);
             bucketY <= FloorDiv(maxY, BucketSize);
             bucketY++)
        {
            for (var bucketX = FloorDiv(minX, BucketSize);
                 bucketX <= FloorDiv(maxX, BucketSize);
                 bucketX++)
            {
                var key = BucketKey(bucketX, bucketY);
                if (!SpatialBuckets.TryGetValue(key, out var bucket))
                {
                    bucket = new List<object>();
                    SpatialBuckets[key] = bucket;
                }

                bucket.Add(listener);
                membership.Keys.Add(key);
            }
        }

        Memberships.Add(listener, membership);
    }

    private static void AddCandidatesForBounds(object bounds)
    {
        if (_boundsMinX is null ||
            _boundsMinY is null ||
            _boundsMaxX is null ||
            _boundsMaxY is null)
        {
            return;
        }

        var minX = Convert.ToInt32(_boundsMinX.GetValue(bounds));
        var minY = Convert.ToInt32(_boundsMinY.GetValue(bounds));
        var maxX = Convert.ToInt32(_boundsMaxX.GetValue(bounds));
        var maxY = Convert.ToInt32(_boundsMaxY.GetValue(bounds));

        for (var bucketY = FloorDiv(minY, BucketSize);
             bucketY <= FloorDiv(maxY, BucketSize);
             bucketY++)
        {
            for (var bucketX = FloorDiv(minX, BucketSize);
                 bucketX <= FloorDiv(maxX, BucketSize);
                 bucketX++)
            {
                var key = BucketKey(bucketX, bucketY);
                if (!SpatialBuckets.TryGetValue(key, out var bucket))
                {
                    continue;
                }

                foreach (var listener in bucket)
                {
                    CandidateSet.Add(listener);
                }
            }
        }
    }

    private static long BucketKey(int x, int y) =>
        ((long)x << 32) | (uint)y;

    private static int FloorDiv(int value, int divisor)
    {
        var result = value / divisor;
        var remainder = value % divisor;
        return remainder < 0 ? result - 1 : result;
    }

    private static Func<object, int>? CreateCoordinateReader(Type coordinateType, MemberInfo? member)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(object), "coordinate");
        var typed = System.Linq.Expressions.Expression.Convert(parameter, coordinateType);
        if (member is FieldInfo field && field.FieldType == typeof(int) && field.IsPublic)
        {
            return System.Linq.Expressions.Expression.Lambda<Func<object, int>>(
                System.Linq.Expressions.Expression.Field(typed, field), parameter).Compile();
        }

        if (member is PropertyInfo property && property.PropertyType == typeof(int) &&
            property.GetGetMethod() is { IsPublic: true })
        {
            return System.Linq.Expressions.Expression.Lambda<Func<object, int>>(
                System.Linq.Expressions.Expression.Property(typed, property), parameter).Compile();
        }

        return null;
    }

    private static object? ReadMember(object instance, MemberInfo member) =>
        member switch
        {
            FieldInfo field => field.GetValue(instance),
            PropertyInfo property => property.GetValue(instance),
            _ => null,
        };

    private static Func<object, object, bool> CreateIntersectsDelegate(
        Type listenerType,
        Type updateType,
        FieldInfo listenerBounds,
        MethodInfo getUpdateBounds,
        Type boundsType,
        FieldInfo minX,
        FieldInfo minY,
        FieldInfo minZ,
        FieldInfo maxX,
        FieldInfo maxY,
        FieldInfo maxZ)
    {
        var dynamicMethod = new System.Reflection.Emit.DynamicMethod(
            "BenchmarkOptimizer_EbrBoundsIntersectFast",
            typeof(bool),
            new[] { typeof(object), typeof(object) },
            typeof(ExtendedBuilderReachNavOptimizer).Module,
            true);

        var il = dynamicMethod.GetILGenerator();
        var left = il.DeclareLocal(boundsType);
        var right = il.DeclareLocal(boundsType);
        var update = updateType.IsValueType ? il.DeclareLocal(updateType) : null;
        var noIntersection = il.DefineLabel();

        il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
        il.Emit(System.Reflection.Emit.OpCodes.Castclass, listenerType);
        il.Emit(System.Reflection.Emit.OpCodes.Ldfld, listenerBounds);
        il.Emit(System.Reflection.Emit.OpCodes.Stloc, left);

        il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
        if (updateType.IsValueType)
        {
            il.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, updateType);
            il.Emit(System.Reflection.Emit.OpCodes.Stloc, update!);
            il.Emit(System.Reflection.Emit.OpCodes.Ldloca, update!);
            il.Emit(System.Reflection.Emit.OpCodes.Call, getUpdateBounds);
        }
        else
        {
            il.Emit(System.Reflection.Emit.OpCodes.Castclass, updateType);
            il.Emit(
                getUpdateBounds.IsVirtual
                    ? System.Reflection.Emit.OpCodes.Callvirt
                    : System.Reflection.Emit.OpCodes.Call,
                getUpdateBounds);
        }
        il.Emit(System.Reflection.Emit.OpCodes.Stloc, right);

        EmitAxisIntersection(il, left, right, minX, maxX, noIntersection);
        EmitAxisIntersection(il, left, right, minY, maxY, noIntersection);
        EmitAxisIntersection(il, left, right, minZ, maxZ, noIntersection);

        il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_1);
        il.Emit(System.Reflection.Emit.OpCodes.Ret);

        il.MarkLabel(noIntersection);
        il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
        il.Emit(System.Reflection.Emit.OpCodes.Ret);

        return (Func<object, object, bool>)dynamicMethod.CreateDelegate(
            typeof(Func<object, object, bool>));
    }

    private static void EmitAxisIntersection(
        System.Reflection.Emit.ILGenerator il,
        System.Reflection.Emit.LocalBuilder left,
        System.Reflection.Emit.LocalBuilder right,
        FieldInfo min,
        FieldInfo max,
        System.Reflection.Emit.Label noIntersection)
    {
        il.Emit(System.Reflection.Emit.OpCodes.Ldloca, left);
        il.Emit(System.Reflection.Emit.OpCodes.Ldfld, min);
        il.Emit(System.Reflection.Emit.OpCodes.Ldloca, right);
        il.Emit(System.Reflection.Emit.OpCodes.Ldfld, max);
        il.Emit(System.Reflection.Emit.OpCodes.Bgt, noIntersection);

        il.Emit(System.Reflection.Emit.OpCodes.Ldloca, left);
        il.Emit(System.Reflection.Emit.OpCodes.Ldfld, max);
        il.Emit(System.Reflection.Emit.OpCodes.Ldloca, right);
        il.Emit(System.Reflection.Emit.OpCodes.Ldfld, min);
        il.Emit(System.Reflection.Emit.OpCodes.Blt, noIntersection);
    }

    private static Action<object, object> CreateNotifyDelegate(
        Type listenerType,
        Type updateType,
        MethodInfo notifyMethod)
    {
        var dynamicMethod = new System.Reflection.Emit.DynamicMethod(
            "BenchmarkOptimizer_EbrNotify",
            typeof(void),
            new[] { typeof(object), typeof(object) },
            typeof(ExtendedBuilderReachNavOptimizer).Module,
            true);

        var il = dynamicMethod.GetILGenerator();
        il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
        il.Emit(System.Reflection.Emit.OpCodes.Castclass, listenerType);
        il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
        if (updateType.IsValueType)
        {
            il.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, updateType);
        }
        else
        {
            il.Emit(System.Reflection.Emit.OpCodes.Castclass, updateType);
        }
        il.Emit(System.Reflection.Emit.OpCodes.Callvirt, notifyMethod);
        il.Emit(System.Reflection.Emit.OpCodes.Ret);

        return (Action<object, object>)dynamicMethod.CreateDelegate(
            typeof(Action<object, object>));
    }
}

internal static class NavOptimizerDiagnostics
{
    // Disabled in normal releases. No per-listener log allocations or profiler hooks.
    [System.Diagnostics.Conditional("EBR_NAV_DIAGNOSTICS")]
    public static void Log(string message) { }

    [System.Diagnostics.Conditional("EBR_NAV_DIAGNOSTICS")]
    public static void RecordNavigationDetail(string name, long elapsedTicks) { }
}
