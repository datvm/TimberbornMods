# Entity Panel Fragments

When a player clicks a building or character, Timberborn opens an **entity inspector panel** made up of stacked fragment sections. TimberUi makes it straightforward to add your own sections: extend `BaseEntityPanelFragment<T>`, build the content with the fluent UiBuilder DSL, and register the fragment in your configurator. This guide walks through each step.

## 1. Write a fragment

### Extend `BaseEntityPanelFragment<T>`

`BaseEntityPanelFragment<T>` is an abstract class that implements Timberborn's `IEntityPanelFragment` lifecycle. The type parameter `T` is the `BaseComponent` your fragment inspects — typically a component that lives on the buildings or characters you want to show it for.

```cs
using TimberUi.CommonUi;

public class StorageFragment(ILoc t) : BaseEntityPanelFragment<Inventory>
{
    Label _lblContents = null!;

    protected override void InitializePanel()
    {
        var row = panel.AddRow();
        row.AddGameLabel("Contents:".Bold()).SetMarginRight();
        _lblContents = row.AddGameLabel("—");
    }

    public override void UpdateFragment()
    {
        if (component is null) { return; }
        _lblContents.text = $"{component.AmountOf(GoodSpecId.Log)} logs";
    }
}
```

The base class creates a `panel` field (an `EntityPanelFragmentElement`) before calling `InitializePanel`, so it is always available when your override runs. `component` is populated by `ShowFragment` and cleared by `ClearFragment`.

### Lifecycle methods

| Method | When called | Responsibility |
|---|---|---|
| `InitializeFragment()` | Once, when the entity-panel UI is built | Sealed base implementation: creates `panel`, calls `InitializePanel()`, returns `panel`. Do **not** override this. |
| `InitializePanel()` | Once, when the entity-panel UI is built (called by `InitializeFragment`) | **Override this** to build the visual tree inside `panel`. Add labels, buttons, sliders, etc. |
| `ShowFragment(BaseComponent entity)` | Each time the player selects an entity | Base resolves `T` via `entity.GetComponent<T>()`. If the entity has no `T`, returns early — panel visibility is unchanged. Sets `panel.Visible = true` when the component is found. Call `base.ShowFragment(entity)` if you override. |
| `UpdateFragment()` | Called periodically by the game while a compatible entity is selected | Virtual no-op by default. Override to read the current state from `component` and update labels/controls. Guard with `if (component is null) return;`. |
| `ClearFragment()` | When the player deselects an entity | Base sets `panel.Visible = false` and nulls `component`. Override only if you need extra teardown, and always call `base.ClearFragment()`. |

### Component resolution

The base calls `entity.GetComponent<T>()` on the raw `BaseComponent` passed by the game. If the selected entity does not have a `T` component, `ShowFragment` returns early leaving the panel as-is (visibility is unchanged). Hiding is handled by `ClearFragment()` setting `panel.Visible = false` on deselect, and by the initial `Visible = false` default — so a fragment for `WaterTower` will silently do nothing when a beaver is selected.

### The `panel` field

`panel` is an `EntityPanelFragmentElement` — a `NineSliceVisualElement` styled as an `entity-sub-panel`. It is the root element returned to the game. Build all your content inside it.

---

## 2. Customize the container appearance

### `EntityPanelFragmentElement`

The panel is an `entity-sub-panel`-styled nine-slice box. Two properties control its visual state:

```cs
panel.Background = EntityPanelFragmentBackground.Blue;
panel.Visible = false;   // hide/show via display-style (not opacity)
```

`Background` swaps a CSS class on the element. The default is `Green`. All available values:

| `EntityPanelFragmentBackground` | Visual style |
|---|---|
| `Green` | Standard green panel (default) |
| `Blue` | Blue panel |
| `PurpleStriped` | Striped purple |
| `PalePurple` | Solid pale purple |
| `RedStriped` | Striped red |
| `Frame` | Frame-only, no fill |

Set the background inside `InitializePanel` (or in an object initializer):

```cs
protected override void InitializePanel()
{
    panel.Background = EntityPanelFragmentBackground.Blue;
    panel.AddGameLabel("My content");
}
```

`Visible = false` is the initial default. The base `ShowFragment` sets it to `true` when the component is found and returns early (leaving visibility unchanged) when the component is absent; the base `ClearFragment` sets it back to `false`.

### Building content with the UiBuilder DSL

Inside `InitializePanel`, use the chainable extension methods on `VisualElement` to assemble controls. Common patterns:

```cs
// A labelled row
var row = panel.AddRow();
row.AddGameLabel("Throughput:").SetMarginRight();
_lblValue = row.AddGameLabel("0/s");

// A slider
_slider = panel.AddSliderInt(label: "Priority", values: new(0, 10, 5));
_slider.RegisterChange(v => _priority = v);

// Buttons
var btns = panel.AddHorizontalContainer();
btns.AddGameButton("Reset", onClick: Reset).SetMarginRight();
btns.AddEntityFragmentButton("Boost", onClick: Boost, color: EntityFragmentButtonColor.Green);

// Progress bar with text label
var bar = panel.AddProgressBar().SetFlexGrow(1).SetColor(ProgressBarColor.Teal);
_barLabel = bar.AddProgressLabel();
```

See [Building UI](./BuildingUi.MD) for the full builder catalog.

> **Initialize complex controls.** Timberborn widgets such as sliders and dropdowns require `panel.Initialize(initializer)` (where `initializer` is an injected `VisualElementInitializer`) before they function. Call it once at the end of `InitializePanel`, after all controls have been added. Simple labels and buttons do not need it.

```cs
public class MyFragment(VisualElementInitializer initializer, DropdownItemsSetter dropdownSetter)
    : BaseEntityPanelFragment<MyComponent>
{
    Dropdown _cbo = null!;

    protected override void InitializePanel()
    {
        _cbo = panel.AddDropdown().AddChangeHandler((_, i) => _selected = i);
        panel.Initialize(initializer);
        _cbo.SetItems(dropdownSetter, ["Option A", "Option B", "Option C"]);
    }
}
```

---

## 3. Register a fragment

Fragments must be registered in your `Configurator` for the **Game** or **MapEditor** context (entity panels do not exist in the main menu).

### Option A — attribute (simplest)

If your configurator uses `GameAttributeConfigurator` (see [Dependency Injection](./DependencyInjection.MD)), add `[BindFragment]` to the fragment class:

```cs
[BindFragment]
public class StorageFragment(ILoc t) : BaseEntityPanelFragment<Inventory>
{
    // ...
}
```

The attribute configurator discovers the class at startup and calls `BindFragment<StorageFragment>()` automatically. No configurator code needed. If the class also implements `IEntityFragmentOrder`, `BindOrderedFragment` is called instead.

### Option B — single fragment, one call

When you are wiring fragments imperatively, `BindFragment<T>()` is a one-liner that registers the fragment as a singleton, wraps it in `EntityPanelFragmentProvider<T>`, and multibinds the provider into `EntityPanelModule`:

```cs
[Context("Game")]
public class GameConfigs : Configurator
{
    public override void Configure()
    {
        this.BindFragment<StorageFragment>();
    }
}
```

The fragment is placed in the `Top` slot of the entity panel by default.

### Option C — multiple fragments via a provider

When you have several fragments to register, or want to control their position, derive a provider class from `EntityPanelFragmentProvider` and call `BindFragments<TProvider>()`. The provider's constructor parameters are the fragment instances; `BindFragments` automatically binds every parameter that implements `IEntityPanelFragment` as a singleton before registering the provider.

```cs
[Context("Game")]
public class GameConfigs : Configurator
{
    // One public constructor, listing every fragment as a parameter.
    public class FragmentsProvider(StorageFragment storage, StatusFragment status)
        : EntityPanelFragmentProvider([storage, status]);

    public override void Configure()
    {
        this.BindFragments<FragmentsProvider>();
    }
}
```

`BindFragments<T>()` requires the provider to have **exactly one public constructor that takes parameters**. All `IEntityPanelFragment` parameters are bound as singletons for you.

The demo project uses this exact pattern to register both `DemoFragment` and all `ColorfulFragments`:

```cs
// TimberUiDemo/ModConfigs.cs (abridged)
[Context("Game")]
public class GameConfigs : Configurator
{
    public class FragmentsProvider(DemoFragment coord) : EntityPanelFragmentProvider([coord]);

    public override void Configure()
    {
        this.BindFragments<FragmentsProvider>();
        this.BindFragments<ColorfulFragmentsProvider>();
    }
}
```

### Fragment positions

`EntityPanelFragmentProvider` places fragments at `Top` by default. To choose a different slot, wrap registrations in `EntityPanelRegistration` and pass them to the non-generic `EntityPanelFragmentProvider` constructor:

```cs
public class FragmentsProvider(StorageFragment storage, StatusFragment status)
    : EntityPanelFragmentProvider(
    [
        new(storage, EntityPanelFragmentPosition.Top),
        new(status,  EntityPanelFragmentPosition.Side),
    ]);
```

Available positions:

| `EntityPanelFragmentPosition` | Location in panel |
|---|---|
| `Top` | Main top section (default) |
| `Middle` | Middle section |
| `Bottom` | Below middle |
| `Side` | Side panel |
| `LeftHeader` | Left header strip (supports `Order`) |
| `MiddleHeader` | Middle header strip |
| `Diagnostic` | Diagnostic section |
| `Footer` | Footer strip |

> **Warning — `Order` is honored only for `LeftHeader`.** The `Order` field on `EntityPanelRegistration` is forwarded to the builder only for `LeftHeader`; for every other position it is silently ignored. To control ordering for fragments in other positions, implement `IEntityFragmentOrder` and register with `BindOrderedFragment` / `BindFragmentOrder` (handled by `EntityFragmentOrderService`).

---

## 4. Control fragment ordering

By default, the game orders fragments in the sequence they were registered. To guarantee that your fragment appears above or below the vanilla fragments — regardless of registration order — implement `IEntityFragmentOrder` on the fragment and register it with `BindOrderedFragment`.

### Implement `IEntityFragmentOrder`

```cs
[BindFragment]                       // attribute path auto-promotes to BindOrderedFragment
public class PriorityFragment : BaseEntityPanelFragment<MyComponent>, IEntityFragmentOrder
{
    // A negative Order floats the fragment to the top (above vanilla fragments).
    // A positive Order sinks it to the bottom.
    // Zero does nothing.
    public int Order => -10;

    // Return the root VisualElement that EntityFragmentOrderService will reposition.
    public VisualElement Fragment => panel;

    protected override void InitializePanel() { /* ... */ }
}
```

### Register with the imperative helpers

When not using attributes, use `BindOrderedFragment<T>()` in place of `BindFragment<T>()`:

```cs
public override void Configure()
{
    this.BindOrderedFragment<PriorityFragment>();
}
```

`BindOrderedFragment<T>()` calls `BindFragment<T>()` and then `BindFragmentOrder<T>()` in one step.

If you already registered the fragment via `BindFragments<TProvider>()` and only want to add ordering separately:

```cs
this.BindFragments<FragmentsProvider>();
this.BindFragmentOrder<PriorityFragment>();  // adds it to the IEntityFragmentOrder multibinding
```

### How ordering works

`EntityFragmentOrderService` (`[BindSingleton(Contexts = NonMenu)]`) collects all `IEntityFragmentOrder` registrations at load time, sorts them by `Order`, then repositions their `Fragment` elements:

- **Negative order** — inserted before all non-ordered fragments (more negative = higher up).
- **Positive order** — appended after all non-ordered fragments (more positive = lower down).
- **Zero** — no repositioning; equivalent to not implementing the interface.

Among fragments with the same `Order` value, relative ordering is determined by the sort's tie-breaking, which is unspecified — assign distinct values if you need a guaranteed sequence.

---

## 5. Complete example

The following mirrors the `DemoFragment` + `ColorfulFragmentsProvider` pattern from the demo project.

```cs
// MyFragment.cs
public class MyFragment(VisualElementInitializer initializer)
    : BaseEntityPanelFragment<BlockObject>
{
    Label _lblCoords = null!;

    protected override void InitializePanel()
    {
        panel.Background = EntityPanelFragmentBackground.Green;

        var row = panel.AddRow();
        row.AddGameLabel("Position:".Bold()).SetMarginRight();
        _lblCoords = row.AddGameLabel("—");

        panel.Initialize(initializer);
    }

    public override void UpdateFragment()
    {
        if (component is null) { return; }
        var c = component.Coordinates;
        _lblCoords.text = $"({c.x}, {c.y}, {c.z})";
    }
}

// ModConfigs.cs
[Context("Game")]
public class GameConfigs : Configurator
{
    public class FragmentsProvider(MyFragment coords)
        : EntityPanelFragmentProvider([coords]);

    public override void Configure()
    {
        this.BindFragments<FragmentsProvider>();
    }
}
```

---

## Key types

| Type | Role | Source |
|---|---|---|
| `BaseEntityPanelFragment<T>` | Abstract base for fragments; lifecycle, component resolution, panel creation. | [`../../TimberUi/TimberUi/CommonUi/DefaultEntityPanelFragment.cs`](../../TimberUi/TimberUi/CommonUi/DefaultEntityPanelFragment.cs) |
| `EntityPanelFragmentElement` | `NineSliceVisualElement` container; `Background` + `Visible`. | [`../../TimberUi/TimberUi/CommonUi/EntityPanelFragmentElement.cs`](../../TimberUi/TimberUi/CommonUi/EntityPanelFragmentElement.cs) |
| `EntityPanelFragmentBackground` | Enum of visual styles for the panel container. | [`../../TimberUi/TimberUi/CommonUi/EntityPanelFragmentElement.cs`](../../TimberUi/TimberUi/CommonUi/EntityPanelFragmentElement.cs) |
| `EntityPanelFragmentProvider` | `IProvider<EntityPanelModule>` that registers one or more fragments with the game. | [`../../TimberUi/TimberUi/CommonProviders/EntityPanelFragmentProvider.cs`](../../TimberUi/TimberUi/CommonProviders/EntityPanelFragmentProvider.cs) |
| `EntityPanelRegistration` | `readonly record struct(Fragment, Position, Order)` used to configure per-fragment position. | [`../../TimberUi/TimberUi/CommonProviders/EntityPanelFragmentProvider.cs`](../../TimberUi/TimberUi/CommonProviders/EntityPanelFragmentProvider.cs) |
| `EntityPanelFragmentPosition` | Enum of slots in the entity panel (Top, Middle, Bottom, Side, …). | [`../../TimberUi/TimberUi/CommonProviders/EntityPanelFragmentProvider.cs`](../../TimberUi/TimberUi/CommonProviders/EntityPanelFragmentProvider.cs) |
| `[BindFragment]` | Attribute that auto-registers a fragment when using `AttributeConfigurator` bases. | [`../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs`](../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs) |
| `BindFragment<T>()` | Configurator extension: register one fragment as a singleton + multibind. | [`../../TimberUi/TimberUi/Extensions/Configurators.cs`](../../TimberUi/TimberUi/Extensions/Configurators.cs) |
| `BindFragments<TProvider>()` | Configurator extension: register a multi-fragment provider; auto-binds all `IEntityPanelFragment` ctor params. | [`../../TimberUi/TimberUi/Extensions/Configurators.cs`](../../TimberUi/TimberUi/Extensions/Configurators.cs) |
| `BindOrderedFragment<T>()` | Configurator extension: `BindFragment` + `BindFragmentOrder` in one call. | [`../../TimberUi/TimberUi/Extensions/Configurators.cs`](../../TimberUi/TimberUi/Extensions/Configurators.cs) |
| `BindFragmentOrder<T>()` | Configurator extension: adds an `IEntityFragmentOrder` to the multibinding only. | [`../../TimberUi/TimberUi/Extensions/Configurators.cs`](../../TimberUi/TimberUi/Extensions/Configurators.cs) |
| `IEntityFragmentOrder` | Interface for ordering: `int Order` + `VisualElement Fragment`. | [`../../TimberUi/TimberUi/Services/EntityPanelOrder/IEntityFragmentOrder.cs`](../../TimberUi/TimberUi/Services/EntityPanelOrder/IEntityFragmentOrder.cs) |
| `EntityFragmentOrderService` | Singleton that repositions ordered fragments after the panel is constructed. | [`../../TimberUi/TimberUi/Services/EntityPanelOrder/EntityFragmentOrderService.cs`](../../TimberUi/TimberUi/Services/EntityPanelOrder/EntityFragmentOrderService.cs) |

### Demo

| File | What it shows |
|---|---|
| [`../../TimberUi/TimberUiDemo/UI/DemoFragment.cs`](../../TimberUi/TimberUiDemo/UI/DemoFragment.cs) | Full `IEntityPanelFragment` implementation: labels, buttons, slider, dropdown, progress bars, formatted text. |
| [`../../TimberUi/TimberUiDemo/UI/ColorfulFragments.cs`](../../TimberUi/TimberUiDemo/UI/ColorfulFragments.cs) | Six concrete fragments (one per `EntityPanelFragmentBackground` value) and a multi-fragment `ColorfulFragmentsProvider`. |
| [`../../TimberUi/TimberUiDemo/ModConfigs.cs`](../../TimberUi/TimberUiDemo/ModConfigs.cs) | `FragmentsProvider` pattern and `BindFragments<T>()` calls in a `[Context("Game")]` configurator. |
