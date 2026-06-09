# Developer Panel Tools

Timberborn ships a collapsible **dev panel** that, when visible, lists sections of clickable debug actions. Each section is contributed by a registered `IDevModule` implementation. TimberUi lets your mod add its own section to this panel with a straightforward multibind plus two game types — and because the dev panel exposes the live UI tree through a `PanelStack`, it is also a convenient hook for dumping UXML snapshots using TimberUi's `PrintVisualTree` extension.

This page shows how to:

1. Implement `IDevModule` and register it.
2. Dump the live visual tree from a dev action.
3. Add a bottom-bar toggle button for dev mode.

For the `PrintVisualTree` / `DescribeVisualTree` extension details and all `UxmlExporter.ExportOptions` flags, see [Utilities.MD — Visual-tree debugging](./Utilities.MD#visual-tree-debugging).  
For bottom-bar registration mechanics, see [AlertsAndBars.MD — Bottom-bar modules](./AlertsAndBars.MD#1-bottom-bar-modules).

---

## 1. What the dev panel is

The dev panel is a Timberborn game-side overlay (shown or hidden by `DevModeManager`) that contains one collapsible card per registered `IDevModule`. Each card has a header (derived from the module's definition) and one button per registered method. Clicking a button calls the `Action` you supplied.

`IDevModule`, `DevModuleDefinition`, `DevMethod`, and `DevModeManager` are **Timberborn game types** from the `Timberborn.Debugging` namespace (`PanelStack` is also a Timberborn game type, from `Timberborn.CoreUI`). TimberUi does not wrap or re-export them — it only makes registration easier via its `MultiBind` helper.

---

## 2. Writing an `IDevModule`

Implement `IDevModule.GetDefinition()` and return a `DevModuleDefinition` built with its fluent builder. Each entry in the panel is a `DevMethod` that pairs a label string with an `Action`.

The demo's `PrintVisualTreeDevModule` is a minimal, complete example:

```cs
// TimberUiDemo/UI/PrintVisualTreeDevModule.cs
namespace TimberUiDemo.UI;

public class PrintVisualTreeDevModule(PanelStack panelStack) : IDevModule
{
    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Print PanelStack root Visual Tree", PrintVisualTree))
            .Build();
    }

    void PrintVisualTree()
    {
        panelStack._root.PrintVisualTree(true);
    }
}
```

Key points:

- The class receives its dependencies through constructor injection. `PanelStack` is a Timberborn game singleton; inject whatever services your debug action needs.
- `DevModuleDefinition.Builder.AddMethod` accepts a `DevMethod` returned by `DevMethod.Create(label, action)`. Call `AddMethod` once per button you want in the card.
- `.Build()` finalises the definition and returns it.
- Your action method can be `void` or `async void`. If it performs async work (e.g. opening a dialog, see `BvmDevModule` in the same repo) it should be `async void` and awaited internally.

### Adding multiple buttons

Call `AddMethod` repeatedly — each call adds one more button to the card:

```cs
public DevModuleDefinition GetDefinition()
{
    return new DevModuleDefinition.Builder()
        .AddMethod(DevMethod.Create("Dump UI tree",   DumpUiTree))
        .AddMethod(DevMethod.Create("Reset counters", ResetCounters))
        .Build();
}
```

---

## 3. Registering the module

In your `[Context("Game")]` configurator, multibind your implementation into `IDevModule`:

```cs
// ModConfigs.cs
[Context("Game")]
public class GameConfigs : Configurator
{
    public override void Configure()
    {
        MultiBind<IDevModule>().To<PrintVisualTreeDevModule>().AsSingleton();
    }
}
```

`MultiBind<IDevModule>()` is the standard Bindito multibind call — no TimberUi extension needed. Using `.AsSingleton()` ensures the same instance handles every button click.

The game collects all `IDevModule` registrations and builds a panel card for each one at startup. You may register as many modules as you like; each gets its own card.

---

## 4. Dumping the visual tree

Getting a UXML snapshot of the live UI hierarchy is the main use-case in the demo. The entry point is the `PrintVisualTree` extension that TimberUi adds to every `VisualElement`:

```cs
void PrintVisualTree()
{
    // Logs the full UXML of the PanelStack root (templates expanded) to the Unity console.
    panelStack._root.PrintVisualTree(true);
}
```

`panelStack._root` is the root `VisualElement` of Timberborn's main UI hierarchy. Injecting `PanelStack` (a Timberborn game type) and accessing its `_root` field is exactly how the demo reaches it.

If you want to capture the output as a string rather than log it immediately, use `DescribeVisualTree` instead:

```cs
void CaptureTree()
{
    string uxml = panelStack._root.DescribeVisualTree();
    // write to file, show in a dialog, etc.
}
```

Both methods live in TimberUi's `Extensions/Helpers.cs` (namespace `UnityEngine.UIElements`). For the full signature including the `templateId` and `options` parameters, and for the `UxmlExporter.ExportOptions` flag table, see [Utilities.MD — Visual-tree debugging](./Utilities.MD#visual-tree-debugging).

> **Note on the bool overload.** `PrintVisualTree(bool)` ignores its argument — it always expands templates (`UxmlExporter.ExportOptions.PrintTemplate`), which is why the demo simply calls `PrintVisualTree(true)`. The no-argument `PrintVisualTree()` binds a *different* overload that uses default options (templates not expanded); use the `(templateId?, options?)` overload and pass explicit `options` when you need control over the export flags.

---

## 5. Dev-mode toggle button

If your mod has UI that should only appear while dev mode is active, you can surface a bottom-bar toggle button that calls `DevModeManager.Enable()` / `DevModeManager.Disable()`. The demo does this via a `GrouplessBottomBarButton` subclass:

```cs
// TimberUiDemo/UI/DevModeButton.cs
namespace TimberUiDemo.UI;

public class DevModeButton : GrouplessBottomBarButton
{
    readonly DevModeManager devMode;

    public DevModeButton(VisualElementLoader veLoader, IAssetLoader assetLoader, DevModeManager devMode)
        : base(veLoader, assetLoader)
    {
        SpritePath = "Sprites/timberui-dev";
        Click      = ToggleDevMode;
        BottomText = "Dev Mode";

        this.devMode = devMode;
    }

    void ToggleDevMode()
    {
        if (devMode.Enabled)
            devMode.Disable();
        else
            devMode.Enable();
    }
}
```

`GrouplessBottomBarButton` and `DevModeManager` are **Timberborn game types**. `GrouplessBottomBarButton` requires `VisualElementLoader` and `IAssetLoader` through its base constructor. Set `SpritePath`, `Click`, and `BottomText` in your constructor before the bar is first rendered.

Register it in the same `[Context("Game")]` configurator, alongside the `IDevModule` binding:

```cs
[Context("Game")]
public class GameConfigs : Configurator
{
    public override void Configure()
    {
        Bind<DevModeButton>().AsSingleton();
        MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider<DevModeButton>>().AsSingleton();

        MultiBind<IDevModule>().To<PrintVisualTreeDevModule>().AsSingleton();
    }
}
```

`BottomBarModuleProvider<T>` is a **TimberUi** type that adds your element to the right section of the bottom bar. See [AlertsAndBars.MD — Bottom-bar modules](./AlertsAndBars.MD#1-bottom-bar-modules) for the full registration walkthrough.

---

## Key types

| Type | Kind | Namespace | Role |
|---|---|---|---|
| `IDevModule` | Timberborn game type | `Timberborn.Debugging` | Interface to implement; `GetDefinition()` returns the panel card definition. |
| `DevModuleDefinition` | Timberborn game type | `Timberborn.Debugging` | Describes a dev panel card; built via `DevModuleDefinition.Builder`. |
| `DevMethod` | Timberborn game type | `Timberborn.Debugging` | A labeled button entry; created with `DevMethod.Create(label, action)`. |
| `DevModeManager` | Timberborn game type | `Timberborn.Debugging` | Singleton that exposes `Enabled`, `Enable()`, and `Disable()` for dev mode state. |
| `PanelStack` | Timberborn game type | `Timberborn.CoreUI` (resolved via DI) | Holds `_root`, the root `VisualElement` of the game's main UI hierarchy. |
| `GrouplessBottomBarButton` | Timberborn game type | `Timberborn.BottomBarSystem` | Base class for a simple bottom-bar button; set `SpritePath`, `Click`, `BottomText`. |
| `BottomBarModuleProvider<T>` | TimberUi type | `TimberUi.CommonProviders` | `IProvider<BottomBarModule>`; adds `T` to the right section of the bottom bar. |
| `PrintVisualTree()` | TimberUi extension | `UnityEngine.UIElements` | Logs the UXML of a `VisualElement` to the Unity console. |
| `DescribeVisualTree()` | TimberUi extension | `UnityEngine.UIElements` | Returns the UXML of a `VisualElement` as a string without logging. |

### Demo source

| File | What it shows |
|---|---|
| [`../../TimberUi/TimberUiDemo/UI/PrintVisualTreeDevModule.cs`](../../TimberUi/TimberUiDemo/UI/PrintVisualTreeDevModule.cs) | Full `IDevModule` implementation: `GetDefinition()`, `DevModuleDefinition.Builder`, `DevMethod.Create`, and calling `PrintVisualTree` on a panel root. |
| [`../../TimberUi/TimberUiDemo/UI/DevModeButton.cs`](../../TimberUi/TimberUiDemo/UI/DevModeButton.cs) | `GrouplessBottomBarButton` subclass wired to `DevModeManager.Enable()` / `Disable()`. |
| [`../../TimberUi/TimberUiDemo/ModConfigs.cs`](../../TimberUi/TimberUiDemo/ModConfigs.cs) | `[Context("Game")]` configurator with `MultiBind<IDevModule>()`, `Bind<DevModeButton>()`, and `MultiBind<BottomBarModule>()`. |

### Library source

| File | What it provides |
|---|---|
| [`../../TimberUi/TimberUi/Extensions/Helpers.cs`](../../TimberUi/TimberUi/Extensions/Helpers.cs) | `PrintVisualTree` and `DescribeVisualTree` extensions on `VisualElement`. |
| [`../../TimberUi/TimberUi/CommonProviders/BottomBarModuleProvider.cs`](../../TimberUi/TimberUi/CommonProviders/BottomBarModuleProvider.cs) | `BottomBarModuleProvider<T>` implementation. |
