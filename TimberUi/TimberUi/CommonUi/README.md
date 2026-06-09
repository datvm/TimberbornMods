# CommonUi

## Purpose
A catalog of reusable, custom `VisualElement` widgets (and a few helper records/factories) that mods can instantiate directly or — more commonly — obtain through the fluent `UiBuilderExtensions` API in `Extensions/`. These wrap Timberborn's CoreUI controls (`Slider`, `Dropdown`, `DialogBox`, `NineSliceVisualElement`, `TProgressBar`, etc.) with mod-friendly, chainable, strongly-typed surfaces so authors don't hand-assemble panels, dialogs, sliders, and entity-fragment boxes from raw UIElements. If you're building a settings panel, a popup dialog, an entity inspector fragment, or a recipe/icon row, these are the building blocks.

## Key types

### Dialogs
- **`DialogBoxElement`** — Base modal dialog. Builds the CoreUI box chrome (`options-panel` > `sliced-border`/`box__content-container` `NineSliceVisualElement` > `ScrollView Content`). Fluent `SetTitle`, `AddCloseButton(customAction?)`, `SetDialogSize`/`SetDialogPercentSize`. `Show(...)` wraps content in a real Timberborn `DialogBox` and pushes it onto a `PanelStack`; `ShowAsync(...)` returns a `Task<bool>` (true = confirmed). Exposes `OnUIConfirmed`/`OnUICancelled`/`Close` delegating to the inner `DialogBox`.
- **`InputDialogBox<TInput,TValueType>`** — `DialogBoxElement` subclass adding a prompt label, a `BaseField<TValueType>` input, and OK/Cancel buttons. `ShowAsync` returns `(bool Confirmed, TValueType? Value)`.
- **`InputDialogBox<TSelf,TInput,TValueType>`** — CRTP layer re-typing `SetPrompt`/`SetValue`/`AddCloseButton` to return `TSelf` for fluent chaining on the concrete subtypes.
- **`InputDialogBox`** / **`IntegerInputDialogBox`** / **`FloatInputDialogBox`** — concrete string/int/float dialogs. The non-generic `InputDialogBox` also exposes static factories `CreateString/CreateInteger/CreateFloat(ILoc)`.

### Sliders
- **`GameSlider<TSlider,TValue>`** — Wraps a Timberborn `BaseSlider<TValue>` in a settings-styled row. Fluent `SetLabel`, `SetHorizontalSlider(SliderValues)`, `AddEndLabel(text | Func<TValue,string>)`, `RegisterChange`, `RegisterAlternativeClickCallback`. `RegisterAlternativeManualValue` opens an `InputDialogBox` when the slider is alt-clicked so the user can type an exact value.
- **`GameSlider<TSelf,TSlider,TValue>`** — CRTP layer re-typing the fluent methods to `TSelf`.
- **`GameSlider`** (float) / **`GameSliderInt`** (int) — concrete sliders, each with `RegisterAlternativeManualValue` overloads wiring the appropriate `InputDialogBox.CreateFloat/CreateInteger`.
- **`SliderValues<TValue>`** — record struct `(Low, High, Default)`.
- **`GameSliderAlternativeManualValueDI`** — `[BindSingleton(Contexts = All)]` bundle of `InputService`, `ILoc`, `VisualElementInitializer`, `PanelStack` so the alt-value dialog can be wired from one injected dependency.

### Dropdown / toggle / collapsible
- **`DropdownRow<TValue>`** — Label + Timberborn `Dropdown` row, strongly typed over `TValue`. `SetItems` (with optional unique-name generation), `SetSelectedValue/Index` (+ `WithoutNotifying` variants), `OnValueChanged` event carrying an `IndexedDropdownRowItem<TValue>`.
- **`DropdownRowProvider<TValue>`** — `IDropdownProvider` backing store mapping `TValue` items to display strings; tracks the selected index and raises `OnSelectedIndexChanged`.
- **`DropdownRowItem<TValue>`** / **`IndexedDropdownRowItem<TValue>`** — record structs for an item and an item-with-index.
- **`ToggleGroup<TValue>`** — Radio-button-style mutually-exclusive group over a set of `Toggle`s; enforces exactly one selection, exposes `Value`/`SelectedOption`, `SetValue(WithoutNotify)`, `ClearSelection`, `OnValueChanged`.
- **`ToggleGroupOption<TValue>`** — record struct pairing a `Toggle` with its `TValue`.
- **`CollapsiblePanel`** — Header (bold clickable label + plus/minus buttons) over a collapsible `Container`. `SetTitle`, `SetExpand`/`ToggleExpand`/`SetExpandWithoutNotify`, `ExpandChanged` event.

### Icons / content rows
- **`IconSpan`** — Row (or column via `SetVertical`) of optional prefix label + `Image` + optional postfix label. `SetImage`/`SetImageSize`/`SetContent`/`ClearContent`, image source abstracted via `ImageSource`.
- **`ImageSource`** — record struct holding exactly one of `Sprite`/`Texture`/`VectorImage`, with implicit conversions both ways; `SetImage(Image)` applies it (throws if empty).
- **`RecipeRow`** — Assembles a full recipe display (ingredients, fuel, arrow, time, products, science) from a `RecipeSpec`, using `IconSpan`s and `NamedIconProvider` glyphs. Built via the injected **`RecipeRowFactory`** (`[BindSingleton]`).
- **`ProgressBarWithLabel`** — record struct pairing a `TProgressBar` with its `Label`; `SetProgress(progress, text?)` updates both. Constructed by `AddProgressBarWithLabel` in `Extensions/ProgressBars.cs`.

### Entity panel fragments
- **`EntityPanelFragmentElement`** — `NineSliceVisualElement` styled as an `entity-sub-panel` with a switchable colored background (`Background` enum property), and a `Visible` toggle (display style). The standard container for entity-inspector fragment content.
- **`EntityPanelFragmentBackground`** — enum: Green/Blue/PurpleStriped/PalePurple/RedStriped/Frame.
- **`BaseEntityPanelFragment<T>`** (in `DefaultEntityPanelFragment.cs`) — abstract `IEntityPanelFragment` base. Creates an `EntityPanelFragmentElement`, resolves the target component `T` on `ShowFragment`, hides on `ClearFragment`. Subclasses implement `InitializePanel` and override `UpdateFragment`.

### Alerts / top bar / misc
- **`ClosableAlertElement`** — Wraps an alert row from `AlertPanelRowFactory.CreateClosable(iconName)`: a main `Button` + a `Close` button, with `Visible`, `AddCloseCallback`, `SetButtonCallback`, `SetButtonAsCloseCallback`, `SetText`. Not a `VisualElement` itself — holds `Root`.
- **`IAlertFragmentWithOrder`** — `IAlertFragment` plus an `int Order` for sorting alert fragments.
- **`TopBarButton`** — `NineSliceButton` with a switchable `square-large--green/brown` background via its `Background` property.
- **`TopBarButtonBackground`** — enum: Green/Brown.
- **`NineSliceFloatField`** — `[UxmlElement]` `FloatField` that paints a `NineSliceBackground` (CoreUI styled float input usable in UXML and as the input field type for float dialogs).

## How it fits together
Most of these widgets are not constructed directly by mod authors. The `Extensions/` fluent builders (`AddIconSpan`, `AddArrow`, `AddDropdownRow`, `AddProgressBarWithLabel`, `AddGameLabel`, `AddMinusButton`/`AddPlusButton`, `AddCloseButton`, etc.) are the intended entry points and they `new` these classes (or call `AddChild<T>()`) and immediately apply CSS classes from `UiCssClasses`. For example `CollapsiblePanel` builds itself entirely out of `AddRow`/`AddGameLabel`/`AddMinusButton`/`AddPlusButton`/`AddChild`; `RecipeRow` is built from `AddRow`/`AddIconSpan`/`AddArrow`; `GameSlider` from `AddChild<TSlider>`. So this folder defines the *components* and `Extensions/` defines the *fluent grammar* that produces and styles them.

CoreUI integration points:
- Dialogs bridge into Timberborn's real `DialogBox` + `PanelStack.PushDialog`, so they participate in the game's modal stack, ESC handling, and confirm/cancel plumbing. `VisualElementInitializer`/`VisualElementLoader._visualElementInitializer` is used to bind/initialize the tree before showing.
- Sliders/dropdowns/toggles wrap CoreUI's `BaseSlider<T>`, `Dropdown`/`IDropdownProvider`, and `Toggle`, adding strong typing and change events on top.
- Entity fragments implement Timberborn's `IEntityPanelFragment` lifecycle (`Initialize/Show/Clear/Update`) so they slot into the entity inspector.
- Styling is driven by the shared `UiCssClasses` string table rather than inline styles, so widgets inherit the game's look (`settings-slider`, `entity-sub-panel`, `options-panel`, `progress-bar`, `square-large--*`, etc.).

The CRTP (`TSelf`) layering on sliders and input dialogs exists purely so fluent calls keep the concrete type and stay chainable without casts.

## Dependencies & patterns
- **Bindito DI:** `RecipeRowFactory` (`[BindSingleton]`) and `GameSliderAlternativeManualValueDI` (`[BindSingleton(Contexts = All)]`) are container-registered; the rest are plain constructibles.
- **Timberborn CoreUI:** `NineSliceVisualElement`/`NineSliceButton`/`NineSliceBackground`, `DialogBox`/`PanelStack`, `Dropdown`/`IDropdownProvider`/`DropdownItemsSetter`, `BaseSlider<T>`/`Slider`/`SliderInt`, `TProgressBar`, `AlertPanelRowFactory`, `IEntityPanelFragment`/`IAlertFragment`, `NamedIconProvider`, `IGoodService`, `RecipeSpec`, `InputService`, `ILoc`, `VisualElementInitializer`/`VisualElementLoader`.
- **Unity UIElements:** subclasses of `VisualElement`, `[UxmlElement]` + generated `UxmlSerializedData` (NineSliceFloatField), `generateVisualContent`/`CustomStyleResolvedEvent` for nine-slice painting, `RegisterValueChangedCallback`, `ChangeEvent<T>`.
- **Patterns:** fluent/chainable builders returning `this`/`TSelf`; CRTP for typed fluent chains; `record struct` value bundles (`SliderValues`, `ImageSource`, `ToggleGroupOption`, `DropdownRowItem`, `ProgressBarWithLabel`); `WithoutNotify` paired setters; backing-field property setters that swap CSS classes (`Background`).

## Usage notes
- **`ClosableAlertElement`** is not a `VisualElement` — it exposes a `Root` property that holds the actual element. Add `.Root` to a parent container rather than the `ClosableAlertElement` instance itself.
