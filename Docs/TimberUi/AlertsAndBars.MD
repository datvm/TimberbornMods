# Alerts and Bars

Timberborn surfaces three complementary UI slots that mods commonly extend: the **alert panel** (dismissible notification rows above the HUD), the **bottom bar** (tool/shortcut buttons at the screen edge), and the **top bar** (HUD icon buttons). TimberUi provides thin providers and helper types for all three, so you can focus on your logic instead of Bindito boilerplate.

---

## 1. Bottom-bar modules

The bottom bar is assembled from `BottomBarModule` instances, each contributed by an `IProvider<BottomBarModule>`. `BottomBarModuleProvider<T>` is TimberUi's ready-made provider: it takes your element class, adds it to the **right section** of the bar, and returns the built module.

### Implement `IBottomBarElementsProvider`

Timberborn's `IBottomBarElementsProvider` is the contract for a bottom-bar element. The easiest starting point is extending `GrouplessBottomBarButton`, which implements the interface and handles sprite loading, click wiring, and the label displayed beneath the button icon:

```cs
using TimberUiDemo.UI;

public class DevModeButton : GrouplessBottomBarButton
{
    readonly DevModeManager _devMode;

    public DevModeButton(VisualElementLoader veLoader, IAssetLoader assetLoader, DevModeManager devMode)
        : base(veLoader, assetLoader)
    {
        SpritePath   = "Sprites/my-mod-icon";
        Click        = Toggle;
        BottomText   = "Dev Mode";

        _devMode = devMode;
    }

    void Toggle()
    {
        if (_devMode.Enabled) _devMode.Disable();
        else                  _devMode.Enable();
    }
}
```

`GrouplessBottomBarButton` is a Timberborn base class; it requires `VisualElementLoader` and `IAssetLoader` injected through the constructor. Set `SpritePath`, `Click`, and `BottomText` before the panel is first shown.

You can also implement `IBottomBarElementsProvider` directly if you need full control over the visual tree.

### Register with `BottomBarModuleProvider<T>`

In your `[Context("Game")]` configurator, bind your element as a singleton and multibind its provider:

```cs
[Context("Game")]
public class GameConfigs : Configurator
{
    public override void Configure()
    {
        Bind<DevModeButton>().AsSingleton();
        MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider<DevModeButton>>().AsSingleton();
    }
}
```

`BottomBarModuleProvider<T>` is resolved by the container and receives the already-bound `T` instance via constructor injection. It calls `builder.AddRightSectionElement(btn)` internally, so your element always lands in the right section of the bar.

> **Placement.** `BottomBarModuleProvider` always adds elements to the right section. If your mod needs a left-section element, use Timberborn's `BottomBarModule.Builder` directly and multibind your own `IProvider<BottomBarModule>`.

---

## 2. Alert-panel fragments

The alert panel is a vertical stack of dismissible notification rows. Each row is contributed by an `IProvider<AlertPanelModule>`. To add a row you need two things: a fragment class that implements `IAlertFragmentWithOrder`, and a registration call that wires it up.

### Implement `IAlertFragmentWithOrder`

`IAlertFragmentWithOrder` extends Timberborn's `IAlertFragment` with a single `int Order` property used to sort rows within the panel:

```cs
using TimberUi.CommonUi;

public class LowFoodAlert : IAlertFragmentWithOrder
{
    readonly FoodService       _food;
    readonly AlertPanelRowFactory _rowFactory;

    ClosableAlertElement _row = null!;

    public int Order => 10;   // higher = lower in the panel; use 0 for neutral position

    public LowFoodAlert(FoodService food, AlertPanelRowFactory rowFactory)
    {
        _food       = food;
        _rowFactory = rowFactory;
    }

    // Timberborn calls InitializeAlertFragment once at startup.
    public void InitializeAlertFragment(VisualElement parent)
    {
        _row = parent.AddClosableAlert(
            factory:           _rowFactory,
            iconName:          "FoodWarning",
            text:              "Food supply is low",
            buttonAction:      () => { /* navigate to stockpile */ },
            closeButtonAction: () => _row.Visible = false);
    }

    // Timberborn calls UpdateAlertFragment periodically, to let you refresh state.
    public void UpdateAlertFragment()
    {
        _row.Visible = _food.IsLow;
    }
}
```

The `Order` value controls sort position relative to other registered fragments: lower numbers appear higher in the panel. Assign distinct values if you need a guaranteed sequence.

### Register with `BindAlertFragment<T>()`

`BindAlertFragment<T>()` is a one-call configurator extension that binds your fragment as a singleton and multibinds `AlertFragmentProvider<T>` into `AlertPanelModule`:

```cs
[Context("Game")]
public class GameConfigs : Configurator
{
    public override void Configure()
    {
        this.BindAlertFragment<LowFoodAlert>();
    }
}
```

Under the hood this is equivalent to:

```cs
BindSingleton<LowFoodAlert>();
MultiBind<AlertPanelModule>().ToProvider<AlertFragmentProvider<LowFoodAlert>>().AsSingleton();
```

`AlertFragmentProvider<T>` calls `builder.AddAlertFragment(fragment, fragment.Order)` on `Get()`, so the `Order` property on your class is the only configuration needed.

---

## 3. `ClosableAlertElement`

`ClosableAlertElement` wraps a Timberborn closable alert row: a main `Button` the player can click to act on the alert, plus a separate `Close` button to dismiss it. It is built from `AlertPanelRowFactory.CreateClosable(iconName)` and exposes a fluent configuration surface.

### Build with `AddClosableAlert`

The `AddClosableAlert` extension method on `VisualElement` is the recommended way to create and attach the element. It calls the constructor, applies optional text and callbacks, names the root, and calls `parent.Add(el.Root)` — all in one step:

```cs
_row = parent.AddClosableAlert(
    factory:           _rowFactory,      // inject AlertPanelRowFactory
    iconName:          "WaterWarning",
    text:              "Reservoir nearly empty",
    buttonAction:      NavigateToReservoir,
    closeButtonAction: () => _row.Visible = false);
```

`AddClosableAlert` returns the `ClosableAlertElement` so you can store it and update it later.

### API surface

| Member | Type | Description |
|---|---|---|
| `Root` | `VisualElement` | The actual element added to the hierarchy. Add this to a parent when constructing manually. |
| `Button` | `Button` | The main action button (left side of the row in the default game template; actual position depends on the game's alert-row layout). |
| `CloseButton` | `Button` | The dismiss button (right side of the row in the default game template; actual position depends on the game's alert-row layout). |
| `Visible` | `bool` | Shows/hides the row via `display` style. Default: `true` after construction. |
| `SetText(string)` | fluent | Sets `Button.text`. |
| `SetButtonCallback(Action)` | fluent | Registers a `ClickEvent` callback on `Button`. |
| `AddCloseCallback(Action)` | fluent | Registers a `ClickEvent` callback on `CloseButton`. |
| `SetButtonAsCloseCallback()` | fluent | Makes the main button also hide the row (`Visible = false`). |

> **`Root` is not a `VisualElement` itself.** `ClosableAlertElement` is a plain class that holds a `Root` property. If you construct it manually with `new ClosableAlertElement(factory, iconName)`, you must call `parent.Add(el.Root)` explicitly. `AddClosableAlert` does this for you.

### Manual construction

If you prefer to construct the element without the extension method:

```cs
_row = new ClosableAlertElement(_rowFactory, "FoodWarning");
_row.SetText("Food supply is low")
    .SetButtonCallback(NavigateToStockpile)
    .AddCloseCallback(() => _row.Visible = false);

parent.Add(_row.Root);   // <-- required: add Root, not the element itself
```

---

## 4. Top-bar buttons

`TopBarButton` is a `NineSliceButton` subclass styled for the game's top bar. It adds a single `Background` property that swaps the button's CSS class between green and brown nine-slice backgrounds.

### Create a `TopBarButton`

`TopBarButton` is a `VisualElement` and can be created with `new` or added via `AddChild<TopBarButton>()`:

```cs
var btn = parent.AddChild<TopBarButton>();
btn.text       = "My Tool";
btn.Background = TopBarButtonBackground.Green;
btn.RegisterCallback<ClickEvent>(_ => OnClick());
```

### Background values

| `TopBarButtonBackground` | CSS class applied | Appearance |
|---|---|---|
| `Green` | `square-large--green` | Green nine-slice button |
| `Brown` | `square-large--brown` | Brown nine-slice button |

Setting `Background` removes the previous class and adds the new one, so it is safe to change at runtime (for example, to reflect active/inactive state).

> **Assign `Background` at least once after construction.** The backing field defaults to `Green`, but the `square-large--*` CSS class is not applied until the setter runs. If you never set `Background`, no background class will be present on the element.

---

## 5. Complete example

The following connects all three features: a bottom-bar toggle button, an alert fragment that shows when a condition is active, and a `ClosableAlertElement` as the alert row.

```cs
// LowFoodAlert.cs
using TimberUi.CommonUi;

public class LowFoodAlert : IAlertFragmentWithOrder
{
    readonly FoodService          _food;
    readonly AlertPanelRowFactory _rowFactory;

    ClosableAlertElement _row = null!;

    public int Order => 10;

    public LowFoodAlert(FoodService food, AlertPanelRowFactory rowFactory)
    {
        _food       = food;
        _rowFactory = rowFactory;
    }

    public void InitializeAlertFragment(VisualElement parent)
    {
        _row = parent.AddClosableAlert(
            factory:           _rowFactory,
            iconName:          "FoodWarning",
            text:              "Food supply is low",
            buttonAction:      null,
            closeButtonAction: () => _row.Visible = false);
    }

    public void UpdateAlertFragment()
    {
        _row.Visible = _food.IsLow;
    }
}

// MyToolButton.cs
public class MyToolButton : GrouplessBottomBarButton
{
    public MyToolButton(VisualElementLoader veLoader, IAssetLoader assetLoader)
        : base(veLoader, assetLoader)
    {
        SpritePath = "Sprites/my-tool";
        Click      = OnClick;
        BottomText = "My Tool";
    }

    void OnClick() { /* open tool panel */ }
}

// ModConfigs.cs
[Context("Game")]
public class GameConfigs : Configurator
{
    public override void Configure()
    {
        this.BindAlertFragment<LowFoodAlert>();

        Bind<MyToolButton>().AsSingleton();
        MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider<MyToolButton>>().AsSingleton();
    }
}
```

---

## Key types

| Type | Role | Source |
|---|---|---|
| `IAlertFragmentWithOrder` | `IAlertFragment` + `int Order`; implement on your alert class. | [`../../TimberUi/TimberUi/CommonUi/IAlertFragmentWithOrder.cs`](../../TimberUi/TimberUi/CommonUi/IAlertFragmentWithOrder.cs) |
| `AlertFragmentProvider<T>` | `IProvider<AlertPanelModule>`; adds `T` to the panel at `T.Order`. | [`../../TimberUi/TimberUi/CommonProviders/AlertFragmentProvider.cs`](../../TimberUi/TimberUi/CommonProviders/AlertFragmentProvider.cs) |
| `BindAlertFragment<T>()` | Configurator extension: bind singleton + multibind provider, one call. | [`../../TimberUi/TimberUi/Extensions/Configurators.cs`](../../TimberUi/TimberUi/Extensions/Configurators.cs) |
| `ClosableAlertElement` | Alert row wrapper: main `Button`, `CloseButton`, `Visible`, `SetText`, `SetButtonCallback`, `AddCloseCallback`. | [`../../TimberUi/TimberUi/CommonUi/ClosableAlertElement.cs`](../../TimberUi/TimberUi/CommonUi/ClosableAlertElement.cs) |
| `AddClosableAlert(...)` | `VisualElement` extension: constructs, configures, and adds `ClosableAlertElement.Root` in one step. | [`../../TimberUi/TimberUi/Extensions/Buttons.cs`](../../TimberUi/TimberUi/Extensions/Buttons.cs) |
| `BottomBarModuleProvider<T>` | `IProvider<BottomBarModule>`; adds `T` to the right section of the bottom bar. | [`../../TimberUi/TimberUi/CommonProviders/BottomBarModuleProvider.cs`](../../TimberUi/TimberUi/CommonProviders/BottomBarModuleProvider.cs) |
| `TopBarButton` | `NineSliceButton` with a swappable `Background` (Green/Brown). | [`../../TimberUi/TimberUi/CommonUi/TopBarButton.cs`](../../TimberUi/TimberUi/CommonUi/TopBarButton.cs) |
| `TopBarButtonBackground` | Enum: `Green` / `Brown`. | [`../../TimberUi/TimberUi/CommonUi/TopBarButton.cs`](../../TimberUi/TimberUi/CommonUi/TopBarButton.cs) |

### Demo

| File | What it shows |
|---|---|
| [`../../TimberUi/TimberUiDemo/UI/DevModeButton.cs`](../../TimberUi/TimberUiDemo/UI/DevModeButton.cs) | `GrouplessBottomBarButton` subclass: `SpritePath`, `Click`, `BottomText`. |
| [`../../TimberUi/TimberUiDemo/ModConfigs.cs`](../../TimberUi/TimberUiDemo/ModConfigs.cs) | `MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider<DevModeButton>>()` in a `[Context("Game")]` configurator. |
