# HUD & Main-Menu Injection

Timberborn exposes two high-visibility injection points that mods commonly use to surface persistent controls: the **top-right HUD** (the row of icon buttons in the upper-right corner of the game screen) and the **main-menu button column** (the vertical list of buttons on the title screen). Both are claimed from a DI-bound service class that implements `ILoadableSingleton` and does the injection work inside `Load()`.

- **Top-right HUD** — inject `UILayout` (a Timberborn game type), build a `NineSliceButton`, and call `layout.AddTopRight(element, order)`.
- **Main-menu column** — inject `MainMenuPanel` (a Timberborn game type), use the TimberUi query helpers to locate the exit button, then use `AddMenuButton(...).InsertSelfAfter(...)` to splice your button into the list.

Neither approach requires a UXML asset or a custom `VisualTreeAsset`. Both injection points persist for the lifetime of their scene — `ILoadableSingleton.Load` is called once per scene load.

---

## 1. Adding a top-right HUD item

The top-right HUD is a row of clickable icon-buttons. `UILayout.AddTopRight(element, order)` appends any `VisualElement` to that row; the `order` parameter controls its position relative to other items (lower numbers appear earlier in the row).

The standard pattern from the demo is:

1. Inject `UILayout`.
2. Create a `NineSliceButton` and apply the `top-right-item` class plus a `square-large--*` color class.
3. Add a child `Label` for live text.
4. Wire `clicked +=` to a handler.
5. Call `layout.AddTopRight(item, order)`.

### Example

```cs
// GameService.cs
using UnityEngine.UIElements;
using TimberUi;
using UiBuilder;

public class GameService(UILayout layout) : ILoadableSingleton
{
    int _counter;
    Label _topBarLabel = null!;

    public void Load()
    {
        AddTopBarItem();
    }

    void AddTopBarItem()
    {
        // Build the button: top-right-item sizes the container;
        // square-large--green applies the nine-slice green background.
        var item = new NineSliceButton()
            .AddClasses([UiCssClasses.TopRightItemClass,
                         UiCssClasses.ButtonTopBarPrefix + UiCssClasses.Green]);

        item.style.justifyContent = Justify.Center;

        item.clicked += OnTopBarItemClicked;

        // AddLabel creates a child Label; AddLabelClasses applies game text styling.
        _topBarLabel = item.AddLabel("Hello TimberUi")
            .AddLabelClasses(GameLabelStyle.Game, color: GameLabelColor.Yellow);

        // order 7 — adjust to taste; lower numbers sit closer to the left of the row.
        layout.AddTopRight(item, 7);
    }

    void OnTopBarItemClicked()
    {
        _topBarLabel.text = $"Clicked: {++_counter} times.";
    }
}
```

Key CSS constants (all on `UiCssClasses`):

| Constant | Value | Purpose |
|---|---|---|
| `TopRightItemClass` | `"top-right-item"` | Sizes and positions the item in the HUD row. |
| `ButtonTopBarPrefix` | `"square-large--"` | Prefix for the color class. Concatenate with a color constant. |
| `Green` | `"green"` | Produces `"square-large--green"`, the green nine-slice background. |
| `Brown` | `"brown"` | Produces `"square-large--brown"`, the brown nine-slice background. |

`AddLabel` and `AddLabelClasses` come from the TimberUi `UiBuilderExtensions` (see [Building UI with the UiBuilder DSL](./BuildingUi.MD)).

### Binding

```cs
// ModConfigs.cs
[Context("Game")]
public class GameConfigs : Configurator
{
    public override void Configure()
    {
        Bind<GameService>().AsSingleton();
    }
}
```

`[Context("Game")]` scopes the binding to the in-game scene. Because `GameService` implements `ILoadableSingleton`, the container calls `Load()` automatically after the scene is ready. Registering as a singleton ensures a single instance manages the HUD item for the lifetime of the session.

---

## 2. Injecting a main-menu button

The main-menu button column is a `NineSliceVisualElement` inside `MainMenuPanel`. TimberUi's query helpers on `MainMenuPanel` give you a typed reference to any existing button, and `InsertSelfAfter` / `InsertSelfBefore` let you splice a new button into the list relative to an existing one.

### Example

```cs
// MenuService.cs
using UnityEngine.UIElements;

public class MenuService(MainMenuPanel menu) : ILoadableSingleton
{
    public void Load()
    {
        // Locate the existing "Exit Game" button in the menu hierarchy.
        var exitButton = menu.GetExitGameButton();

        // AddMenuButton builds a NineSliceButton with menu-button styling
        // (initially parented under exitButton); InsertSelfAfter then splices
        // it into the column immediately after exitButton.
        exitButton
            .AddMenuButton("Show demo dialog", ShowDemoDialog,
                           name: "ShowDemoDialog",
                           stretched: true)
            .InsertSelfAfter(exitButton);
    }

    void ShowDemoDialog()
    {
        // ... open your dialog here
    }
}
```

`GetExitGameButton()` returns the `LocalizableButton` named `"ExitButton"` inside the menu's root. `AddMenuButton` builds the new button (initially as a child of the exit button), and `InsertSelfAfter(exitButton)` moves it into the column immediately after the exit button. TimberUi also provides `GetMainMenuPanel()`, which returns the `NineSliceVisualElement` named `"MainMenuPanel"` — the column container itself — if you prefer to add buttons to the column directly.

`AddMenuButton` is a `VisualElement` extension that creates a `NineSliceButton` with `menu-button` styling. The `stretched: true` parameter adds the `menu-button--stretched` modifier, which makes the button fill the full column width, matching the style of the built-in main-menu buttons.

### Binding

```cs
// ModConfigs.cs
[Context("MainMenu")]
public class MainMenuConfigs : Configurator
{
    public override void Configure()
    {
        Bind<MenuService>().AsSingleton();
    }
}
```

`[Context("MainMenu")]` scopes the binding to the title-screen scene. As with the game context, `ILoadableSingleton.Load` is called once after the scene is ready.

### Positioning variants

| Method | Effect |
|---|---|
| `.InsertSelfAfter(target)` | Places the new element immediately **below** `target` in the column. |
| `.InsertSelfBefore(target)` | Places the new element immediately **above** `target` in the column. |

Both are `VisualElement` extensions from TimberUi's `Childrens.cs`. They locate the parent of `target`, find its index, and insert the new element immediately before or after it.

---

## 3. `TopBarButton` vs. a raw `NineSliceButton` in `UILayout`

TimberUi offers two ways to put a styled button in the top-right area:

| Approach | When to use |
|---|---|
| **`TopBarButton`** (from `TimberUi.CommonUi`) | When you want a self-contained `NineSliceButton` subclass where the background (`TopBarButtonBackground.Green` or `.Brown`) is managed through a typed property and can be toggled at runtime to reflect state. The class handles adding and removing the `square-large--*` class for you. See [Alerts and Bars](./AlertsAndBars.MD#4-top-bar-buttons). |
| **Raw `NineSliceButton` + `UILayout.AddTopRight`** | When you need to embed child elements (such as a live `Label`) inside the button, or when you need direct control over the full visual tree. You apply `UiCssClasses.TopRightItemClass` and `UiCssClasses.ButtonTopBarPrefix + UiCssClasses.Green` manually, just as `GameService` does. |

Both approaches call `layout.AddTopRight(element, order)` to register the element with the HUD row. The difference is only in how the button itself is constructed and styled.

Use `TopBarButton` for a simple clickable icon whose background might change. Use a raw `NineSliceButton` with child elements when the item is compositional — for example, a button that displays a counter or a status label inside its bounds.

---

## Key types

| Type / Member | Role | Source |
|---|---|---|
| `UILayout` | Timberborn game type. `AddTopRight(element, order)` adds an element to the top-right HUD row. | Timberborn game assembly |
| `MainMenuPanel` | Timberborn game type. Root panel for the title-screen UI. | Timberborn game assembly |
| `GetMainMenuPanel()` | Extension on `MainMenuPanel`. Returns the `NineSliceVisualElement` named `"MainMenuPanel"` — the button column container. | [`../../TimberUi/TimberUi/Extensions/Queries.cs`](../../TimberUi/TimberUi/Extensions/Queries.cs) |
| `GetExitGameButton()` | Extension on `MainMenuPanel`. Returns the `LocalizableButton` named `"ExitButton"`. | [`../../TimberUi/TimberUi/Extensions/Queries.cs`](../../TimberUi/TimberUi/Extensions/Queries.cs) |
| `AddMenuButton(...)` | `VisualElement` extension. Creates a `NineSliceButton` with `menu-button` styling; optional `stretched`, `size`, `name`. | [`../../TimberUi/TimberUi/Extensions/Buttons.cs`](../../TimberUi/TimberUi/Extensions/Buttons.cs) |
| `InsertSelfAfter(target)` | `VisualElement` extension. Inserts the element immediately after `target` in the parent's child list. | [`../../TimberUi/TimberUi/Extensions/Childrens.cs`](../../TimberUi/TimberUi/Extensions/Childrens.cs) |
| `InsertSelfBefore(target)` | `VisualElement` extension. Inserts the element immediately before `target` in the parent's child list. | [`../../TimberUi/TimberUi/Extensions/Childrens.cs`](../../TimberUi/TimberUi/Extensions/Childrens.cs) |
| `UiCssClasses.TopRightItemClass` | `"top-right-item"` — required class for HUD row items. | [`../../TimberUi/TimberUi/UiCssClasses.cs`](../../TimberUi/TimberUi/UiCssClasses.cs) |
| `UiCssClasses.ButtonTopBarPrefix` | `"square-large--"` — prefix for the color class on top-bar buttons. | [`../../TimberUi/TimberUi/UiCssClasses.cs`](../../TimberUi/TimberUi/UiCssClasses.cs) |
| `AddLabelClasses(style, color)` | Extension on `TextElement`. Applies game text CSS classes to a label in place. | [`../../TimberUi/TimberUi/Extensions/Properties.cs`](../../TimberUi/TimberUi/Extensions/Properties.cs) |
| `TopBarButton` | `NineSliceButton` subclass with a typed `Background` property (Green/Brown). Alternative to a raw button. | [`../../TimberUi/TimberUi/CommonUi/TopBarButton.cs`](../../TimberUi/TimberUi/CommonUi/TopBarButton.cs) |
| `ILoadableSingleton` | Timberborn contract. `Load()` is called once after the scene is ready. Implement on your service to hook injection timing. | Timberborn game assembly |

### Demo

| File | What it shows |
|---|---|
| [`../../TimberUi/TimberUiDemo/Services/GameService.cs`](../../TimberUi/TimberUiDemo/Services/GameService.cs) | Full HUD item: `NineSliceButton`, CSS classes, `AddLabel`, `AddLabelClasses`, `layout.AddTopRight`, live counter in `clicked +=`. |
| [`../../TimberUi/TimberUiDemo/Services/MenuService.cs`](../../TimberUi/TimberUiDemo/Services/MenuService.cs) | Full menu injection: `GetExitGameButton()`, `AddMenuButton(..., stretched: true)`, `InsertSelfAfter`. |
| [`../../TimberUi/TimberUiDemo/ModConfigs.cs`](../../TimberUi/TimberUiDemo/ModConfigs.cs) | `[Context("Game")]` binding for `GameService`; `[Context("MainMenu")]` binding for `MenuService`. |
