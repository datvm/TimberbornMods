using System.Reflection;
using Bindito.Core;
using ModSettings.CommonUI;
using ModSettings.Core;
using ModSettings.CoreUI;
using Timberborn.CoreUI;
using Timberborn.Modding;
using Timberborn.Options;
using Timberborn.OptionsGame;
using Timberborn.SettingsSystem;
using Timberborn.SingletonSystem;
using TimberUi.CommonUi;
using UnityEngine.UIElements;

namespace BenchmarkAndOptimizerV11;

[Context("MainMenu")]
public sealed class MainMenuConfig : Configurator
{
    public override void Configure()
    {
        this.BindSingleton<OptimizerSettingsOwner>()
            .MultiBindSingleton<IModSettingElementFactory, OptimizerSettingElementFactory>()
            .BindSingleton<OptimizerSettingsBoxWidthService>();
    }
}

[Context("Game")]
public sealed class GameConfig : Configurator
{
    public override void Configure()
    {
        this.BindSingleton<OptimizerSettingsOwner>()
            .MultiBindSingleton<IModSettingElementFactory, OptimizerSettingElementFactory>()
            .BindSingleton<OptimizerSettingsBoxWidthService>()
            .BindSingleton<OptimizerOriginRegistryService>()
            .BindSingleton<OptimizerMenuService>();
    }
}

public sealed class OptimizerSettingsOwner(
    ISettings settings,
    ModSettingsOwnerRegistry registry,
    ModRepository repository
) : ModSettingsOwner(settings, registry, repository)
{
    public override string ModId => "BenchmarkAndOptimizerV11Port";
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public OptimizerUiSetting CsvBenchmark { get; } = new();
    public IntervalSettingsUiSetting UpdateIntervals { get; } = new();
}

public sealed class OptimizerUiSetting()
    : NonPersistentSetting(ModSettingDescriptor.Create("Benchmark & CSV profiler"))
{
    public override void Reset() { }
}

public sealed class IntervalSettingsUiSetting()
    : NonPersistentSetting(ModSettingDescriptor.Create("Advanced update intervals"))
{
    public override void Reset() => Runtime.ResetIntervals();
}

public sealed class CsvBenchmarkPanel : VisualElement
{
    public CsvBenchmarkPanel()
    {
        this.SetPadding(8);
        this.AddGameLabel("Timberborn benchmark / CSV", bold: true).SetMarginBottom(6);
        this.AddGameLabel("Collect per-system timings for 120 seconds.\nSimulation-critical ticks run at vanilla cadence.")
            .SetMarginBottom(10);
        this.AddMenuButton("Start 120s CSV benchmark", onClick: Runtime.StartBenchmark)
            .SetMarginBottom(6);
        this.AddGameLabel("CSV output: the mod folder.\nBenchmark stops automatically after 120 seconds.")
            .SetMarginBottom(6);
    }
}

public sealed class OptimizerSettingElementFactory(
    VisualElementInitializer veInit
) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is OptimizerUiSetting)
        {
            element = new ModSettingElement(new CsvBenchmarkPanel(), modSetting);
            return true;
        }
        if (modSetting is IntervalSettingsUiSetting)
        {
            element = new ModSettingElement(new OptimizerPanel(veInit), modSetting);
            return true;
        }

        element = null;
        return false;
    }
}

public sealed class OptimizerSettingsBoxWidthService(
    IContainer container
) : IPostLoadableSingleton
{
    public void PostLoad()
    {
        try
        {
            // Same pattern as datvm/TImprove4Mods/Services/ModSettingBoxService.cs:
            // resolve ModSettingsBox from the finished container, then modify its panel.
            var box = container.GetInstance<ModSettingsBox>();
            var panelBox = box.GetPanel().Q("Box");
            if (panelBox is null)
            {
                Runtime.Log("warning: ModSettings Box element not found; keeping default width");
                return;
            }

            panelBox.SetWidth(885f);
            Runtime.Log("ModSettings box width set to 885px (1.5x default)");
        }
        catch (Exception ex)
        {
            Runtime.Log($"warning: failed to widen ModSettings box: {ex.GetType().Name}: {ex.Message}");
        }
    }
}

public sealed class OptimizerPanel : VisualElement
{
    readonly VisualElement wellKnownPanel;
    readonly VisualElement otherSystemsPanel;
    readonly Label statusLabel;
    string filterText = "";
    // Cache rendered controls: filtering must not tear down a focused UI tree.
    readonly List<(VisualElement Element, string Name, string Origin)> systemRows = [];

    static readonly string[] WellKnown =
    {
        "PhysicsSimulator",
        "BehaviorManager",
        "NavMeshObserver",
        "NavigationSynchronizer",
        "AutomationRunner",
        "WaterSimulator",
        "SpeedManager",
        "ConstructionSite",
        "ResourceCountingService"
    };

    public OptimizerPanel(VisualElementInitializer veInit)
    {
        // Follow datvm's own TimberUi/ModSettings pattern:
        // use AddToggle/AddSliderInt/RegisterChange instead of raw UIElements callbacks.
        this.SetPadding(8);

        this.AddGameLabel("Benchmark & Optimizer — safe update intervals", bold: true);
        this.AddGameLabel("1 = vanilla frequency. Higher values run that system once every N dispatcher calls. Changes apply immediately.")
            .SetMarginBottom(8);

        var enabled = this.AddToggle("Optimizer enabled", onValueChanged: Runtime.SetEnabled);
        enabled.value = Runtime.Enabled;

        this.AddSliderInt(
                label: "Default interval",
                values: new SliderValues<int>(1, 50, Runtime.DefaultInterval))
            .AddEndLabel(v => v == 1 ? "vanilla" : $"1/{v}")
            .RegisterChange(Runtime.SetDefaultInterval)
            .SetWidthPercent(100)
            .SetMarginBottom(8);

        var actions = this.AddRow();
        actions.AddMenuButton("Reset all to 1", onClick: () =>
        {
            Runtime.ResetIntervals();
            Rebuild();
        });
        actions.AddMenuButton("Refresh detected systems", onClick: Rebuild);
        actions.SetMarginBottom(8);

        statusLabel = this.AddGameLabel("");
        statusLabel.SetMarginBottom(8);

        // Never Clear()/recreate UI controls during a text-field change callback.
        // The input event may run inside Unity's LateUpdate/UI dispatch.
        this.AddTextField("Filter", keyword =>
        {
            filterText = keyword?.Trim() ?? "";
            try
            {
                ApplyFilter();
            }
            catch (Exception ex)
            {
                Runtime.Log($"warning: optimizer UI filter failed: {ex.GetType().Name}: {ex.Message}");
            }
        })
        .SetWidthPercent(100)
        .SetMarginBottom(8);

        // Match datvm's TechTree horizontal-scroll pattern: initialize the
        // ScrollView through Timberborn's VisualElementInitializer before use.
        // Unlike TechTree, this list would otherwise shrink to the viewport, so
        // give its content a wider minimum width to create real horizontal overflow.
        var scroll = this.AddScrollView().Initialize(veInit);
        scroll.mode = ScrollViewMode.VerticalAndHorizontal;
        scroll.horizontalScrollerVisibility = scroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
        scroll.SetMaxHeight(650);

        var scrollContent = scroll.AddChild()
            .SetMinSize(1200f, null);

        wellKnownPanel = scrollContent.AddChild();
        otherSystemsPanel = scrollContent.AddChild();

        Rebuild();
    }

    void UpdateStatus()
    {
        statusLabel.text = Runtime.IsBenchmarking
            ? $"Mode: {Runtime.Mode} — {Runtime.BenchmarkRemainingSeconds:F1}s remaining"
            : $"Mode: {Runtime.Mode} — Detected systems: {Runtime.KnownTypes.Count}";
    }

    void Rebuild()
    {
        UpdateStatus();
        wellKnownPanel.Clear();
        otherSystemsPanel.Clear();
        systemRows.Clear();

        wellKnownPanel.AddGameLabel("Well-known systems", bold: true).SetMarginBottom(5);
        otherSystemsPanel.AddGameLabel("Other detected systems", bold: true).SetMargin(10, 0, 5, 0);

        var names = WellKnown
            .Concat(Runtime.KnownTypes)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => Array.IndexOf(WellKnown, n) < 0 ? 1 : 0)
            .ThenBy(n => n, StringComparer.OrdinalIgnoreCase);

        foreach (var name in names)
        {
            var origin = Runtime.GetOrigin(name);
            var row = AddSystemRow(
                Array.IndexOf(WellKnown, name) >= 0 ? wellKnownPanel : otherSystemsPanel,
                name, origin);
            systemRows.Add((row, name, origin));
        }

        ApplyFilter();
    }

    void ApplyFilter()
    {
        // Hiding existing rows is safe even during a UI text-change callback.
        // Refresh/reset buttons may still explicitly rebuild the list.
        foreach (var row in systemRows)
        {
            var matches = string.IsNullOrEmpty(filterText)
                || row.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase)
                || row.Origin.Contains(filterText, StringComparison.OrdinalIgnoreCase);
            row.Element.style.display = matches ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    static VisualElement AddSystemRow(VisualElement parent, string typeName, string origin)
    {
        var current = Runtime.GetInterval(typeName);
        var isProtected = Runtime.IsProtected(typeName);

        // Two-line layout so long type/origin text never pushes the interval control
        // out of the visible settings panel.
        var item = parent.AddChild()
            .SetMarginBottom(10);

        item.AddGameLabel($"{typeName}    [{origin}]")
            .SetMarginBottom(3);

        var ownershipNote = Runtime.GetThrottleOwnershipNote(typeName);
        if (!string.IsNullOrWhiteSpace(ownershipNote))
        {
            item.AddGameLabel(ownershipNote)
                .SetMarginBottom(3);
        }

        // Keep the interval text outside BaseSlider's fixed-width label area.
        // Otherwise Timberborn clips the label and the slider thumb overlaps it.
        var intervalRow = item.AddRow()
            .AlignItems(Align.Center);

        var intervalLabel = intervalRow.AddGameLabel(
                isProtected ? "Tick interval - 1/1 (protected)" : TickIntervalLabel(current))
            .SetMinSize(isProtected ? 220f : 125f, null)
            .SetMarginRight(10f);

        if (isProtected)
        {
            return item;
        }

        var slider = intervalRow.AddSliderInt(
                values: new SliderValues<int>(1, 50, current))
            .SetFlexGrow()
            .SetMinSize(700f, null);

        slider.RegisterChange(v =>
        {
            intervalLabel.text = TickIntervalLabel(v);
            Runtime.SetInterval(typeName, v);
        });

        return item;
    }

    static string TickIntervalLabel(int value) => $"Tick interval - 1/{value}";
}


public sealed class OptimizerOriginRegistryService(
    ModRepository repository
) : ILoadableSingleton
{
    public void Load()
    {
        try
        {
            var enabledMods = repository.GetType()
                .GetProperty("EnabledMods", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(repository) as System.Collections.IEnumerable;

            if (enabledMods is null)
            {
                Runtime.Log("warning: origin registry could not read ModRepository.EnabledMods");
                return;
            }

            var registeredMods = 0;
            var registeredAssemblies = 0;

            foreach (var mod in enabledMods)
            {
                if (mod is null)
                {
                    continue;
                }

                var modType = mod.GetType();
                var manifest = modType
                    .GetProperty("Manifest", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(mod);
                var modDirectory = modType
                    .GetProperty("ModDirectory", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(mod);

                if (manifest is null || modDirectory is null)
                {
                    continue;
                }

                var manifestType = manifest.GetType();
                var name = manifestType
                    .GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(manifest) as string;
                var id = manifestType
                    .GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(manifest) as string;

                var directoryType = modDirectory.GetType();
                var originPath = directoryType
                    .GetProperty("OriginPath", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(modDirectory) as string;

                if (string.IsNullOrWhiteSpace(originPath) || !Directory.Exists(originPath))
                {
                    continue;
                }

                var workshopId = ExtractWorkshopId(originPath);
                var isLocal = originPath.Replace('\\', '/')
                    .Contains("/Timberborn/Mods/", StringComparison.OrdinalIgnoreCase);

                var display = !string.IsNullOrWhiteSpace(name)
                    ? name!
                    : !string.IsNullOrWhiteSpace(id) ? id! : Path.GetFileName(originPath.TrimEnd(Path.DirectorySeparatorChar));

                if (!string.IsNullOrWhiteSpace(id) && !string.Equals(display, id, StringComparison.Ordinal))
                {
                    display += $" ({id})";
                }

                var suffix = !string.IsNullOrWhiteSpace(workshopId)
                    ? $" [Workshop {workshopId}]"
                    : isLocal ? " [local]" : "";

                var origin = $"MOD: {display}{suffix}";
                registeredMods++;

                foreach (var dll in Directory.EnumerateFiles(originPath, "*.dll", SearchOption.AllDirectories))
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(dll);
                    Runtime.RegisterModAssemblyOrigin(assemblyName, origin);
                    Runtime.RegisterModPathOrigin(dll, origin);
                    registeredAssemblies++;
                }
            }

            Runtime.Log($"origin registry loaded: mods={registeredMods}, assemblies={registeredAssemblies}");
        }
        catch (Exception ex)
        {
            Runtime.Log($"warning: origin registry failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    static string? ExtractWorkshopId(string path)
    {
        var normalized = path.Replace('\\', '/');
        const string marker = "/steamapps/workshop/content/1062090/";
        var index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var rest = normalized[(index + marker.Length)..];
        return rest.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    }
}


public sealed class OptimizerMenuService(
    IOptionsBox optionsBox,
    ModSettingsBox modSettingsBox,
    ModRepository repository
) : ILoadableSingleton
{
    readonly GameOptionsBox settingsOptionsBox = (GameOptionsBox)optionsBox;
    Mod? currentMod;

    public void Load()
    {
        try
        {
            currentMod = ResolveMod(repository, "BenchmarkAndOptimizerV11Port");
            if (currentMod is null)
            {
                Runtime.Log("warning: pause-menu button could not resolve BenchmarkAndOptimizerV11Port in ModRepository");
                return;
            }

            // The game stores the menu root in a non-public member. Our build
            // stubs are not publicized game assemblies, so access it via reflection
            // rather than emitting a direct field access that fails at runtime.
            var root = ResolveOptionsRoot(settingsOptionsBox);
            if (root is null)
            {
                Runtime.Log("warning: pause-menu root not available; optimizer menu entry not added");
                return;
            }

            var resumeButton = root.Q("ResumeButton") ?? root.Q("Resume");
            if (resumeButton is null)
            {
                Runtime.Log("warning: pause-menu ResumeButton/Resume not found; optimizer menu entry not added");
                return;
            }

            if (root.Q("BenchmarkAndOptimizerButton") is not null)
            {
                return;
            }

            var button = root.AddMenuButton(
                "Benchmark & Optimizer",
                onClick: () => modSettingsBox.Open(currentMod),
                name: "BenchmarkAndOptimizerButton",
                stretched: true);

            button.InsertSelfAfter(resumeButton);
            Runtime.Log("pause-menu entry installed after ResumeButton/Resume");
        }
        catch (Exception ex)
        {
            // The shortcut must not abort scene loading if a dependency changes.
            Runtime.Log($"warning: pause-menu integration disabled after {ex.GetType().Name}: {ex.Message}");
        }
    }

    static VisualElement? ResolveOptionsRoot(GameOptionsBox optionsBox)
    {
        // "_root" is an existing Timberborn field name, not a new member of our
        // mod. Keep its exact spelling, and never rely on publicized assembly
        // access in a mod compiled against stand-in reference assemblies.
        for (Type? type = optionsBox.GetType(); type is not null; type = type.BaseType)
        {
            var rootField = type.GetField(
                "_root",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            if (rootField is not null)
            {
                return rootField.GetValue(optionsBox) as VisualElement;
            }
        }

        return null;
    }

    static Mod? ResolveMod(ModRepository repository, string id)
    {
        try
        {
            var enabledMods = repository.GetType()
                .GetProperty("EnabledMods", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(repository) as System.Collections.IEnumerable;

            if (enabledMods is null)
            {
                return null;
            }

            foreach (var item in enabledMods)
            {
                if (item is not Mod mod)
                {
                    continue;
                }

                var manifest = item.GetType()
                    .GetProperty("Manifest", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(item);

                var manifestId = manifest?.GetType()
                    .GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(manifest) as string;

                if (manifestId == id)
                {
                    return mod;
                }
            }
        }
        catch (Exception ex)
        {
            Runtime.Log($"warning: failed to resolve optimizer mod for pause-menu button: {ex.GetType().Name}: {ex.Message}");
        }

        return null;
    }
}
