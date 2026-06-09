# Extensions

## Purpose

This folder is the consumer-facing heart of TimberUi: a large set of `static` extension-method classes that form a fluent **UiBuilder DSL** for assembling Timberborn `VisualElement` trees, plus a set of **configurator/binding helpers** that wrap Bindito DI ceremony into readable one-liners. A mod author building any custom panel, fragment, dialog, or settings UI lives almost entirely in these methods: `parent.AddRow().AddLabel(...).AddButton(...)` chains for layout, and `configurator.BindFragment<T>()` / `BindSingleton<T>()` / `BindTemplateModule(...)` for wiring.

Two distinct concerns share the folder (and mostly share the same `UiBuilderExtensions` type via `partial`):
1. **UI construction** — `Add*`, `Set*`, `Q*` element builders (the bulk of files).
2. **DI / configurator wiring** — `Configurators.cs`, `ConfiguratorHelpers.cs`, `OptionalServices.cs`.

Because everything is extension methods on Timberborn/Unity types and is declared in *their* namespaces (`UnityEngine.UIElements`, `Bindito.Core`, `Timberborn.*`), the API surface appears "native" — a mod author gets these methods just by referencing the assembly, with no `using TimberUi` needed.

## Key types

The vast majority of methods hang off one `partial` class **`UiBuilderExtensions`**, deliberately declared in several files across namespaces to match the receiver types. Grouped by file:

### UI element builders (namespace `UnityEngine.UIElements`)
- **`Childrens.cs`** — the primitives everything else builds on: `AddChild<T>` / `AddChild(Type)` (Activator-creates a `VisualElement`, parents it, applies name + classes), `AddChild<T>(Func<T> factory)`, `AddRow`, `AddHorizontalContainer`, `InsertSelfBefore/After/AsSibling`, `AddLabel` / `AddLabelHeader` / `AddGameLabel`, `AddScrollView` / `AddGameScrollView` / `AddListView`, `AddFragment` (EntityPanelFragmentElement), `AddToggle`, `AddCollapsiblePanel`, plus CSS-class resolvers `GetClasses(GameLabelStyle...)` / `GetGameLabelClasses` / `GetClasses(ToggleStyle)`.
- **`Buttons.cs`** — `AddButton` (two overloads: `Action` and `EventCallback<ClickEvent>`), style-specific shortcuts `AddMenuButton` / `AddGameButton` / `AddGameButtonPadded` / `AddEntityFragmentButton` / `AddStretchedEntityFragmentButton`, `AddCloseButton`, `AddSquareButton` / `AddPlusButton` / `AddMinusButton`, `AddAction<T>` (register click on any `Button`), `AddClosableAlert`, and the CSS resolver `GetClasses(GameButtonStyle, size, stretched)`. Defines `enum EntityFragmentButtonColor { Green, Red }`.
- **`TextFields.cs`** — generic `AddValueField<T,TValueType>` plus `AddTextField` / `AddIntField` / `AddFloatField` (the `NineSlice*Field` variants), each wiring an optional `Action<TValue>` change callback.
- **`Dropdowns.cs`** — `AddDropdown` / `AddMenuDropdown`, `AddChangeHandler`, `SetItems` (via `SimpleDropdownItemProvider`), `SetSelectedItem` (by string or index — index overload range-checks and throws), `GetSelectedValue` / `GetSelectedIndex`.
- **`DropdownRowExtensions.cs`** — separate class `DropdownRowExtensions`: three `AddDropdownRow<TValue>` overloads building `DropdownRow<TValue>` with label, change handler, and item-setter wiring.
- **`Sliders.cs`** — `AddSlider` (float) / `AddSliderInt` / `AddMinMaxSlider`, plus a `MinMaxSlider` extension block (`SetLabel`, `SetValue`, `SetValueWithoutNotify`, `RegisterChangeCallback`). Defines `readonly record struct MinMaxSliderValues(Value, Min, Max)`.
- **`ProgressBars.cs`** — `AddProgressBar` / `AddProgressBarWithLabel` / `AddProgressLabel`, `SetColor` (add/remove a color modifier class on both the host and its inner `"ProgressBar"` child), `SetProgress` (progress + optional label text + old/new color swap). Defines `enum ProgressBarColor { Green, Teal, Red, Blue }`.
- **`Images.cs`** — `IconSpan` content helpers (`SetGood`, `SetScience`, `SetTime`), `AddIconSpan` (several overloads), `AddArrow`, `AddImage` (by class, sprite, texture, asset path via `IAssetLoader`, or `NamedIconProvider` icon name), `AddInventoryInputImage` / `AddInventoryOutputImage`. Exposes `InventoryInputClasses` / `InventoryOutputClasses` immutable arrays.
- **`Properties.cs`** — pure inline-style fluent setters: `AddClass` / `AddClasses`, `SetMargin*` / `SetPadding*`, `SetWidth/Height/Size` (+ percent, max, min, min-max variants), `SetAsRow`, `SetWrap`, `SetFlexGrow` / `SetFlexShrink`, `SetDisplay`, `AlignItems`, `JustifyContent`, `SetBorder`, `AddLabelClasses`. This is the styling vocabulary of the DSL.
- **`Helpers.cs`** — `VisualElement` lifecycle/debug helpers: `Initialize(VisualElementLoader/Initializer)` (required to make game widgets functional — see Usage notes), `SetName`, `PrintVisualTree` / `DescribeVisualTree` (UXML dump via `UxmlExporter`), and obsolete stylesheet print/describe helpers.
- **`Deconstructions.cs`** — `Deconstruct` for `Vector2/2Int/3/3Int` (enables tuple unpacking).
- **`Texts.cs`** — rich-text string decorators returning Unity TMP markup strings: `Bold` / `Italic` / `Size` / `Strikethrough` / `Highlight` / `Color`. Defines `enum TimberbornTextColor { Green, Red, Yellow }` (with obsolete `Solid` alias for `Yellow`).

### Localization & data (Timberborn namespaces)
- **`Loc.cs`** (`Timberborn.Localization`) — `string.T(ILoc)` shorthands (0–3 args + `TFormat`), cached common strings `TYes/TNo/TOK/TCancel/TNone` (plus `TYesNo`, which composes `TYes`/`TNo`), and `Priority` / `PopulationCounterMode` localization.
- **`BaseComponents.cs`** (`Timberborn.BaseComponentSystem`) — `CommonTimberUiExtensions` on `BaseComponent`: `GetTemplateName`, runtime-typed `GetComponent(Type)` (caches the closed generic `MethodInfo`), `GetName` / `GetLabeledName`.
- **`Specs.cs`** (`Timberborn.BlueprintSystem`) — `CommonTimberUiExtensions` on `ComponentSpec`: `GetTemplateName`, `GetName(ILoc)`.
- **`PopulationDataExtensions.cs`** (`Timberborn.Population`) — `PopulationCounterMode.UseCountParameters()` and `PopulationData.GetData` — two overloads: `GetData(mode)` (both count flags hard-coded `true`) and `GetData(mode, countBeavers, countBots)` (both flags required) — a big mode→value switch across population/workplace/contamination data.
- **`Persistences.cs`** (`Timberborn.Persistence`) — `IObjectLoader.GetPair/GetPairs` and `IObjectSaver.SetPair/SetPairs`: serialize `KeyValuePair<TKey,TValue>` (both `IConvertible`) into a single separator-delimited string (default `'|'`).
- **`Conversions.cs`** (`UnityEngine`) — `Color`↔`Vector3/4` and 0–255 color conversions (`ToColor255`, `ToColor255Int`, etc.).
- **`Properties.cs` / `Queries.cs`** — `Queries.cs` exposes `MainMenuPanel.GetMainMenuPanel()` / `GetExitGameButton()` (mod-menu injection points).
- **`Dialog.cs`** (`Timberborn.CoreUI`) — `DialogBoxShower.ShowAsync(...)`: wraps the callback-based `DialogBoxShower` into an `async Task<bool?>` (true=confirm, false=cancel, null=info) via a `TaskCompletionSource`. The message is a required parameter (localization optional, defaulting to localized); up to 3 button labels are optional.

### DI / configurator wiring (namespace `Bindito.Core`)
- **`Configurators.cs`** — the core binding helpers (see next section).
- **`ConfiguratorHelpers.cs`** — fluent entry points to the stateful helper objects: `BindTemplateModule()` (returns a `TemplateModuleHelper`, you must call `.Bind()`) and the `Func`-taking overload that binds for you; `MassRebind()` / `MassRebind(Action<MassRebindingHelper>)`.
- **`OptionalServices.cs`** — opt-in service bindings safe to call from multiple mods (idempotent via `TryBind`/`IsBound`): `TryBindingCameraShake`, `TryBindingModdableAudioClip` (mod WAV loading; bootstrapper context), `TryBindingAudioClipManagement`, `TryBindingSystemFileDialogService`, `TryBindingSpriteOperations` (MainMenu context only), `BindAchievement<T>`. Contains one obsolete alias (`TryAddingCameraShake`).

## How it fits together

**The UiBuilder pattern.** Almost every builder follows the same contract: it is an extension on `VisualElement` (the parent), it creates a child via `AddChild`, applies a name + a computed list of CSS classes, optionally wires a callback or sets text/value, and **returns the created element** (or, for `Set*`/`AddClass` mutators, returns the same element). That uniform "return the element" convention is what makes the fluent chaining work:

```
var row = panel.AddRow();
row.AddLabel("Name").SetMarginRight();
row.AddTextField(changeCallback: OnNameChanged).SetFlexGrow();
row.AddButton("OK", onClick: Save, style: GameButtonStyle.Menu);
```

Styling is layered on via the `Properties.cs` `Set*` mutators (inline `style.*`) and `AddClass/AddClasses` (USS classes from `UiCssClasses`). Enums (`GameButtonStyle`, `GameLabelStyle`, `ProgressBarColor`, etc.) are translated to class strings by the various `GetClasses(...)` resolvers, centralizing the "which USS class" knowledge.

`AddChild` is the universal root: typed `AddChild<T>()` uses `new()`, the `Type` overload uses `Activator.CreateInstance`, and a `Func<T>` overload lets callers construct widgets that need constructor args (e.g. `DropdownRow`). Game-specific widgets returned by these builders frequently must be `Initialize(...)`d (Helpers.cs) before they behave correctly — the builders generally do not call Initialize for you (dropdowns/sliders do it conditionally when both a `VisualElementInitializer` and item-setter are supplied).

**The configurator/binding pattern.** A mod's `Configurator.Configure(IContainerDefinition)` normally writes verbose Bindito chains. `Configurators.cs` collapses these into fluent, chainable `Configurator`-returning helpers:

- **Fragments (entity panels).** `BindFragment<T>()` registers an `IEntityPanelFragment` by wrapping it in `EntityPanelFragmentProvider<T>` and binding it (plus the fragment itself as singleton). `BindFragments<T>()` (on a fragment *provider*) reflects the provider's single parameterized public constructor and auto-singleton-binds every `IEntityPanelFragment` constructor parameter, then multibinds the provider into `EntityPanelModule`. `BindFragmentOrder<T>` / `BindOrderedFragment<T>` add `IEntityFragmentOrder`. `BindAlertFragment<T>` does the alert-panel equivalent. Type-object overloads (`BindFragment(Type)`, `BindOrderedFragment(Type)`) validate the interface and dispatch through cached generic `MethodInfo`s resolved in the static constructor.
- **General bindings.** `BindSingleton<T>` / `BindSingleton<T,TImpl>` / `BindTransient<T>`, plus runtime-`Type` variants (`Bind(Type)`, `Bind(src,dst,scope,...)`, `BindExported(...)`, `MultiBind(Type,Type)`) that use reflection to invoke the generic Bindito `Bind`/`MultiBind`. `MultiBindSingleton` / `MultiBindAndBindSingleton` cover the multibind+singleton combos. `AsScope(Scope)` maps the `Scope` enum to `AsTransient`/`AsSingleton`.
- **Removal & queries.** `RemoveBinding` / `MassRemoveBindings` / `RemoveMultibinding(s)` remove entries from the binding registry — used for rebinding/overriding game services. `IsBound` / `TryGetBound` / `IsMultiBound` / `TryBind` / `TryMultiBind` are idempotent guards that make "call from multiple mods" safe (used throughout `OptionalServices.cs`).
- **Helpers as objects.** For bulk operations, `ConfiguratorHelpers.cs` hands out stateful helper objects (`TemplateModuleHelper`, `MassRebindingHelper`) defined elsewhere in the library; the `Func`/`Action` overloads run-and-`Bind` for you, the bare overloads require an explicit `.Bind()`.

A typical configurator reads top-to-bottom as a fluent script: `configurator.BindSingleton<MyService>().BindFragment<MyFragment>().BindTemplateModule(t => t.Add(...))`.

## Dependencies & patterns

- **Timberborn UIElements / CoreUI:** `NineSliceButton`, `NineSlice*Field`, `Dropdown` / `DropdownRow`, `GameSlider(Int)`, `MinMaxSlider`, `TProgressBar` / `ProgressBarWithLabel`, `IconSpan`, `EntityPanelFragmentElement`, `CollapsiblePanel`, `ClosableAlertElement`, `DialogBoxShower`, `MainMenuPanel`, `VisualElementInitializer` / `VisualElementLoader`, `UxmlExporter`, `UiCssClasses` (the central USS-class constant table).
- **Bindito.Core:** binding and multibinding infrastructure accessed via reflection to support the runtime-`Type` overloads and the removal/rebinding helpers.
- **C# 13 `extension` blocks.** Many files use the new `extension(receiver)` / `extension<T>(receiver)` member syntax rather than classic `this`-parameter methods. Both styles coexist (Buttons/Childrens/Properties use classic `this`; Loc/Images/Sliders/Persistences/BaseComponents use `extension` blocks).
- **Reflection caching:** `getComponentCache` (BaseComponents), the static-constructor-resolved `BindFragmentMethod` / `BindOrderedFragmentMethod` (Configurators) and `GetComponentMethod`. The static constructor throws `InvalidOperationException` if those method lookups fail — a loud, fail-fast initialization.
- **Fail-loud philosophy:** unknown enum values throw `NotImplementedException`, missing names throw, range checks throw.

## Usage notes

- **Extension methods appear without `using TimberUi`.** Because the extension classes are declared in game and Unity namespaces (`UnityEngine.UIElements`, `Bindito.Core`, `Timberborn.*`), they are in scope whenever those namespaces are already imported — no additional `using` directive is required.
- **Many game widgets must be initialized before they work.** Timberborn widgets such as sliders, dropdowns, and text fields require `VisualElementInitializer.InitializeVisualElement` to function correctly. Call `Initialize(...)` (from `Helpers.cs`) after adding a widget if you are not passing an initializer directly to the builder method. Slider and dropdown builders call `Initialize` automatically when you supply both an initializer and an item-setter; most other builders leave this to the caller.
- **`BindFragments<T>()` requires exactly one parameterized public constructor on the provider.** A provider with only a parameterless constructor, or with two or more parameterized constructors, will throw `InvalidOperationException` at bind time.
