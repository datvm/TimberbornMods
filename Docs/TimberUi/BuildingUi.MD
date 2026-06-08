# Building UI with the UiBuilder DSL

TimberUi's UiBuilder DSL is a set of extension methods on Unity's `VisualElement` that let you assemble a complete UI tree in a straight, readable chain. Every `Add…` call creates a child element, parents it, applies the right game CSS classes, and returns the new element so the next call can chain off it. Every `Set…` call mutates the element and returns the same element. The result matches Timberborn's visual style — menus, entity-panel fragments, and settings panels all use these methods internally.

The builder extension methods live in `UnityEngine.UIElements`, but several types require additional `using` directives:

- **`using UiBuilder;`** — style enums: `GameLabelStyle`, `GameLabelSize`, `GameLabelColor`, `GameButtonStyle`, `GameButtonSize`, `ToggleStyle`
- **`using TimberUi;`** — `UiCssClasses`
- **`using TimberUi.CommonUi;`** — widget types returned by builders: `GameSlider`, `GameSliderInt`, `DropdownRow<>`, `IconSpan`, `ImageSource`, `CollapsiblePanel`, `ProgressBarWithLabel`

(`MinMaxSliderValues` lives in `UnityEngine.UIElements`, so it needs no extra `using`.)

A typical fragment therefore needs all three in addition to the standard `UnityEngine.UIElements` import.

> **Source:** [`../../TimberUi/TimberUi/Extensions/`](../../TimberUi/TimberUi/Extensions/)

---

## The core pattern: building a tree

### `AddChild`, `AddRow`, `AddHorizontalContainer`

Every builder ultimately goes through `AddChild`, which creates an element, adds it to the parent, and optionally sets its name and CSS classes:

```cs
// Generic: creates a typed child with new()
Button btn = parent.AddChild<Button>(name: "my-button", classes: ["my-class"]);

// Factory overload: for types that need constructor arguments
var row = parent.AddChild<DropdownRow<int>>(() => new DropdownRow<int>());

// Untyped: creates a plain VisualElement
VisualElement spacer = parent.AddChild(name: "spacer");
```

For horizontal layouts, `AddRow` and `AddHorizontalContainer` are the everyday shortcuts:

```cs
// AddRow — sets flexDirection: row; returns the row
var row = panel.AddRow();

// AddHorizontalContainer — row + 20px bottom margin (pass false to suppress)
var con = panel.AddHorizontalContainer();
var con2 = panel.AddHorizontalContainer(marginBottom: false);
```

A full fragment `InitializeFragment` typically looks like this:

```cs
public VisualElement InitializeFragment()
{
    panel = new EntityPanelFragmentElement
    {
        Background = EntityPanelFragmentBackground.Green
    };

    var row = panel.AddHorizontalContainer();
    row.AddGameLabel("Coordinates".Bold());
    lblCoord = row.AddGameLabel("N/A");

    panel.Initialize(initializer);   // required — see Initializing game widgets
    return panel;
}
```

### Inserting elements relative to an existing sibling

When you need to inject an element next to one that already exists in the tree:

```cs
newElement.InsertSelfBefore(existingElement);
newElement.InsertSelfAfter(existingElement);
// or at an arbitrary offset:
newElement.InsertSelfAsSibling(existingElement, delta: 2);
```

---

## Labels and text

### `AddLabel`, `AddLabelHeader`, `AddGameLabel`

```cs
// Plain game label — "text--default" USS class
Label lbl = parent.AddLabel("Hello");

// Header variant — "text--header"
Label hdr = parent.AddLabelHeader("Section title");

// Full game-label with size, color, bold, and centering control
Label big = parent.AddGameLabel(
    text:     "42",
    size:     GameLabelSize.Big,
    color:    GameLabelColor.Yellow,
    bold:     true,
    centered: false);
```

`AddLabel` accepts an optional `style` parameter from `GameLabelStyle`:

| `GameLabelStyle` | USS classes applied |
|---|---|
| `Default` (default) | `text--default` |
| `Header` | `text--header` |
| `Game` | `game-text-normal` (+ size/color/bold modifiers) |
| `EntityFragment` | `entity-panel__text` |

`AddGameLabel` accepts `GameLabelSize` and `GameLabelColor`:

| `GameLabelSize` | `GameLabelColor` |
|---|---|
| `Normal` (default) | `Default` (no color modifier) |
| `Big` | `Yellow` |

After creation, you can add the same label classes to any `TextElement` you constructed manually:

```cs
existingLabel.AddLabelClasses(GameLabelStyle.Game, size: GameLabelSize.Big, bold: true);
```

### Rich-text decorators

`Texts.cs` adds extension methods on `string` that return Unity TextMeshPro markup. Compose them inline:

```cs
parent.AddLabel(
    $"Status: {"online".Color(TimberbornTextColor.Green)} — " +
    $"{"WARNING".Bold().Color(TimberbornTextColor.Red)}");

parent.AddLabel("Note: " + "alt-click to type a value".Italic());
parent.AddLabel("Cost: " + "50".Bold().Color("#FFD700"));
```

| Method | Markup produced |
|---|---|
| `"x".Bold()` | `<b>x</b>` |
| `"x".Italic()` | `<i>x</i>` |
| `"x".Size(18)` | `<size=18>x</size>` |
| `"x".Strikethrough()` | `<s>x</s>` |
| `"x".Highlight()` | yellow color markup |
| `"x".Color(TimberbornTextColor.Green/Red/Yellow)` | `<color=#…>x</color>` |
| `"x".Color("#FF0000")` or `"x".Color("FF0000")` | custom hex color |

`TimberbornTextColor` has three members: `Green`, `Red`, and `Yellow` (`Solid` is a deprecated alias for `Yellow`).

---

## Buttons

### `AddButton` — the universal button

```cs
// Action overload (most common)
NineSliceButton btn = parent.AddButton(
    text:    "Confirm",
    onClick: OnConfirm,
    style:   GameButtonStyle.Menu,
    size:    GameButtonSize.Large,
    stretched: false);

// EventCallback overload (gives access to the click event)
NineSliceButton btn2 = parent.AddButton(
    text:    "Cancel",
    clickCb: evt => { evt.StopPropagation(); Cancel(); },
    style:   GameButtonStyle.Game);
```

### Style shortcuts

```cs
// Menu button (default style — most settings and dialog buttons)
parent.AddMenuButton("Apply", onClick: Apply);

// In-game button (used in entity panels and HUD)
parent.AddGameButton("Reset", onClick: Reset);

// In-game button with explicit padding (pixels X, Y)
parent.AddGameButtonPadded("Reset", onClick: Reset, paddingX: 8, paddingY: 4);
```

### `GameButtonStyle` reference

| Value | Usage |
|---|---|
| `Text` | Unstyled, bold text only |
| `Menu` (default) | Main-menu and settings panels |
| `WideMenu` | Wide variant of menu button |
| `BottomBar` | Bottom action bar |
| `DevPanel` | Developer panel |
| `Game` | In-game panels and fragments |
| `EntityFragment` | Inside entity-panel fragments |

`GameButtonSize` values are `Medium` (default), `Small`, and `Large`. Not all style/size combinations produce a visible size difference — `Game` style has no size modifier.

### Entity-fragment buttons

Fragment panels use a specialized button with a colored border:

```cs
// Green or Red border variant
NineSliceButton btn = parent.AddEntityFragmentButton(
    text:  "Fill",
    onClick: FillAll,
    color: EntityFragmentButtonColor.Green);

// Add an inventory icon inside the button's content area
btn.Q("Content").AddInventoryOutputImage();
```

### Close, plus, and minus buttons

```cs
Button closeBtn = parent.AddCloseButton();

// Plus / minus — size controls the icon scale
NineSliceButton plusBtn  = parent.AddPlusButton(size: GameButtonSize.Small);
NineSliceButton minusBtn = parent.AddMinusButton(size: GameButtonSize.Medium);
```

### Adding a click handler to an existing button

`AddAction` registers a callback on any `Button` and returns the same button, so it chains:

```cs
plusBtn.AddAction(Add);
minusBtn.AddAction(Subtract).SetMarginRight();
```

---

## Input fields

### Text, integer, and float fields

Each creates a styled `NineSlice*Field` with the `text-field` USS class:

```cs
NineSliceTextField   txt  = parent.AddTextField(changeCallback: OnNameChanged);
NineSliceIntegerField num = parent.AddIntField(changeCallback: OnCountChanged);
NineSliceFloatField  flt  = parent.AddFloatField(changeCallback: OnRatioChanged);
```

Callbacks receive the new value directly — no need to unwrap a `ChangeEvent`. All three support optional `name` and `additionalClasses` parameters.

---

## Sliders

### `AddSlider` and `AddSliderInt`

These return a `GameSlider` or `GameSliderInt` — a pre-styled row (`settings-slider` / `settings-text` classes) wrapping the underlying `Slider` or `SliderInt`.

```cs
GameSlider    floatSlider = parent.AddSlider(
    label:  "Speed",
    values: new SliderValues<float>(Low: 0f, High: 10f, Default: 1f));

GameSliderInt intSlider = parent.AddSliderInt(
    label:  "Workers",
    values: new SliderValues<int>(Low: 0, High: 20, Default: 5));
```

After adding, chain slider configuration:

```cs
intSlider
    .RegisterChange(v => workerCount = v)
    .AddEndLabel(v => $"{v} workers")   // live-updating label on the right
    .SetValue(currentWorkerCount);
```

`SetValueWithoutNotify` silently updates both the slider position and the end label without firing change callbacks.

### Alt-click to type an exact value

`GameSlider` and `GameSliderInt` support an optional mode where alt-clicking the slider opens an `InputDialogBox` so the user can type a precise number. Inject `GameSliderAlternativeManualValueDI` and call:

```cs
intSlider.RegisterAlternativeManualValue(altValueDI, prompt: "Enter worker count");
```

### `AddMinMaxSlider`

For a range selector (two thumbs):

```cs
MinMaxSlider range = parent.AddMinMaxSlider(
    values:         new MinMaxSliderValues(Value: new Vector2(2f, 8f), Min: 0f, Max: 10f),
    changeCallback: v => ApplyRange(v.x, v.y),
    label:          "Range");

// Fluent control after creation
range.SetValue(new MinMaxSliderValues(new Vector2(3f, 7f), 0f, 10f));
range.SetValueWithoutNotify(new Vector2(3f, 7f));
```

---

## Dropdowns

### `AddDropdown` — raw Timberborn `Dropdown`

A bare `Dropdown` requires a `DropdownItemsSetter` (injected) to populate it:

```cs
Dropdown cbo = parent.AddDropdown()
    .AddChangeHandler((value, index) => OnSelectionChanged(index));

// Populate (after the element is initialized — see below)
cbo.SetItems(dropdownSetter, ["Option A", "Option B", "Option C"]);
cbo.SetSelectedItem(0);
```

`AddMenuDropdown` is identical but adds the `settings-dropdown` USS class for a menu-styled appearance.

`GetSelectedValue()` returns the selected string; `GetSelectedIndex()` returns the zero-based index, or `-1` when nothing is selected or the dropdown is not yet populated. `SetSelectedItem(int)` throws `ArgumentOutOfRangeException` if the index is out of range.

### `AddDropdownRow<TValue>` — typed label + dropdown

`DropdownRow<TValue>` pairs a label with a typed dropdown and fires a strongly-typed `OnValueChanged` event. Three overloads are available:

```cs
// 1. Minimal — populate later
DropdownRow<Difficulty> row = parent.AddDropdownRow<Difficulty>(
    label:    "Difficulty",
    onChange: e => ApplyDifficulty(e.Item.Value));

// 2. Pre-populated from DropdownRowItem<TValue> collection
var items = new[]
{
    // record is (TValue Value, string Text)
    new DropdownRowItem<Difficulty>(Difficulty.Easy,   "Easy"),
    new DropdownRowItem<Difficulty>(Difficulty.Normal, "Normal"),
    new DropdownRowItem<Difficulty>(Difficulty.Hard,   "Hard"),
};
DropdownRow<Difficulty> row2 = parent.AddDropdownRow(
    values:     items,
    veInit:     initializer,
    itemSetter: dropdownSetter,
    label:      "Difficulty",
    onChange:   e => ApplyDifficulty(e.Item.Value));

// 3. Pre-populated from raw values + display-string function
DropdownRow<int> row3 = parent.AddDropdownRow(
    values:     Enumerable.Range(1, 10),
    textFn:     v => $"Level {v}",
    veInit:     initializer,
    itemSetter: dropdownSetter,
    label:      "Level");
```

To select an item programmatically:

```cs
row.SetSelectedValue(Difficulty.Hard);
row.SetSelectedIndex(2);

// Without firing OnValueChanged
row.SetSelectedValueWithoutNotifying(Difficulty.Normal);
row.SetSelectedIndexWithoutNotifying(1);
```

---

## Images and icons

### `AddImage` overloads

```cs
// By USS class name — the class provides the background sprite
// Use the named argument; a bare string binds to the `name` overload, not the class overload
Image img1 = parent.AddImage(className: "my-icon-class");

// By Sprite reference
Image img2 = parent.AddImage(mySprite);

// By Texture
Image img3 = parent.AddImage(myTexture2D);

// Load from mod asset path via IAssetLoader
Image img4 = parent.AddImage("Assets/MyMod/Textures/icon.png", assetLoader);

// By named-icon key via NamedIconProvider
Image img5 = parent.AddImage("ScienceIcon", namedIconProvider);
```

### `AddIconSpan`

`IconSpan` is a pre-styled row of optional prefix text + image + optional postfix text:

```cs
// From any ImageSource (Sprite / Texture / VectorImage — implicit conversions exist)
IconSpan span = parent.AddIconSpan(mySprite, prefixText: "50", postfixText: " kg");

// Arrow glyph from NamedIconProvider (used in recipe rows)
IconSpan arrow = parent.AddArrow(namedIconProvider);
```

Content helpers on an existing `IconSpan`:

```cs
span.SetGood(goodService, "Timberborn.Planks", amount: "10");
span.SetScience(namedIconProvider, amount: "25");
span.SetTime(namedIconProvider, time: "2.5s");
```

### Inventory slot images

Two shorthand adders give you the standard input/output tray icons used in building panels:

```cs
parent.AddInventoryInputImage();
parent.AddInventoryOutputImage();
```

---

## Progress bars

### `AddProgressBar`

A bare progress bar with the `progress-bar` class. The return type is Timberborn's `ProgressBar` (`Timberborn.CoreUI.ProgressBar`, which TimberUi aliases internally as `TProgressBar`); from your own code, declare it as `ProgressBar` or just use `var`:

```cs
var bar = parent.AddProgressBar()        // Timberborn.CoreUI.ProgressBar
    .SetFlexGrow()
    .SetColor(ProgressBarColor.Green);

// Update value (0–1) and swap color in one call
bar.SetProgress(ratio, label: null, text: null,
    oldColor: ProgressBarColor.Green, newColor: ProgressBarColor.Red);
```

### `AddProgressBarWithLabel`

Returns a `ProgressBarWithLabel` record struct `(ProgressBar ProgressBar, Label Label)` that keeps bar and label in sync:

```cs
ProgressBarWithLabel pgb = parent.AddProgressBarWithLabel(
    color: ProgressBarColor.Teal,
    text:  "0%");

// Simplest update — convenience method sets bar + label together
pgb.SetProgress(ratio, $"{ratio * 100:F0}%");

// Or update the bar directly when you need color-swap or other control
pgb.ProgressBar.SetProgress(ratio, pgb.Label, $"{ratio * 100:F0}%");
```

### `ProgressBarColor`

| Value | Color |
|---|---|
| `Green` (default) | Green |
| `Teal` | Teal |
| `Red` | Red |
| `Blue` | Blue |

`SetColor(color, removeColor: true)` removes a color class (use when swapping colors so the old class is cleaned up).

---

## Styling with `Properties.cs`

Every `Set…` and `Add…Class` method returns the same element, so they chain directly onto any builder call.

### CSS classes

```cs
element.AddClass("my-class");
element.AddClasses("class-a", "class-b", "class-c");
```

### Margins and padding

```cs
// Single value — applies to all sides
element.SetMargin(10f);

// X/Y shorthand — (marginX: left+right, marginY: top+bottom)
element.SetMargin(marginX: 8f, marginY: 4f);

// Per-side (top, right, bottom, left)
element.SetMargin(top: 0, right: 20, bottom: 20, left: 0);

// Convenience helpers — default value is 20px
element.SetMarginRight();          // 20px right margin
element.SetMarginBottom(12f);      // 12px bottom margin
element.SetMarginLeftAuto();       // auto left margin (push right)

// Padding follows the same pattern
element.SetPadding(5f);
element.SetPadding(paddingX: 8f, paddingY: 4f);
element.SetPadding(top: 2, right: 10, bottom: 2, left: 10);
```

### Size

```cs
element.SetWidth(200f);
element.SetHeight(40f);
element.SetSize(200f, 40f);      // width, height
element.SetSize(40f);            // square

element.SetWidthPercent(50f);    // 50% of parent
element.SetHeightPercent(100f);

element.SetMaxWidth(400f);
element.SetMaxHeight(200f);
element.SetMinSize(100f, 30f);   // min-width, min-height
element.SetMinMaxSize(120f, 30f); // (width, height) — pins each to min = max = value
```

### Flex layout

```cs
// Switch any element to row direction
element.SetAsRow();

// Flex grow / shrink
element.SetFlexGrow();           // grow = 1 (fills available space)
element.SetFlexGrow(2f);
element.SetFlexShrink();         // shrink = 1
element.SetWrap();               // enable flex-wrap

// Align children
element.AlignItems(Align.Center);    // default
element.AlignItems(Align.FlexStart);
element.JustifyContent(Justify.SpaceBetween);
```

### Visibility and borders

```cs
element.SetDisplay(visible);              // true = flex, false = none
element.SetBorder(color: Color.red, width: 2f);
```

### Naming an element at construction time

```cs
element.SetName("my-element");
```

---

## Collapsible panels and toggles

```cs
// Collapsible panel — header with +/- buttons and a collapsible body
CollapsiblePanel cp = parent.AddCollapsiblePanel(title: "Advanced options", expand: false);
cp.Container.AddLabel("Hidden content");
cp.ExpandChanged += expanded => Debug.Log($"Expanded: {expanded}");

// Toggle (settings style)
Toggle t = parent.AddToggle(
    text:           "Enable feature",
    style:          ToggleStyle.Settings,
    onValueChanged: v => featureEnabled = v);
```

---

## Scroll views and list views

```cs
// Scroll view with green decoration (default)
ScrollView sv = parent.AddScrollView();

// Game-styled scroll view
ScrollView gsv = parent.AddGameScrollView();

// List view (same USS class as scroll view; green-decorated by default)
ListView lv = parent.AddListView(greenDecorated: false);
```

---

## Initializing game widgets

Several Timberborn controls — sliders, dropdowns, text fields — need their `VisualElementInitializer` to run before they respond to input or display correctly. Call `Initialize` **after** building the element tree and **before** returning from `InitializeFragment`:

```cs
public VisualElement InitializeFragment()
{
    panel = new EntityPanelFragmentElement
    {
        Background = EntityPanelFragmentBackground.Green
    };

    slider   = panel.AddSliderInt(label: "Count", values: new(0, 10, 5));
    dropdown = panel.AddDropdown();

    // Initialize the whole tree in one call — resolves on all children recursively
    panel.Initialize(initializer);

    // Populate the dropdown after initializing
    dropdown.SetItems(dropdownSetter, ["A", "B", "C"]);

    return panel;
}
```

`Initialize` has two overloads:

```cs
// From a VisualElementInitializer (injected directly)
element.Initialize(visualElementInitializer);

// From a VisualElementLoader (injected in some contexts — extracts its initializer)
element.Initialize(visualElementLoader);
```

`AddDropdownRow` with `veInit` and `itemSetter` arguments calls `Initialize` for you automatically. For all other widget types, and for entity-panel fragments built on `EntityPanelFragmentElement`, call it explicitly on the root before returning.

---

## Builder reference

### Layout primitives

| Method | Returns | Notes |
|---|---|---|
| `AddChild<T>(name?, classes?)` | `T` | Generic, requires `new()` |
| `AddChild<T>(Func<T> factory)` | `T` | For types with constructor args |
| `AddChild(Type?, name?, classes?)` | `VisualElement` | Untyped |
| `AddRow(name?)` | `VisualElement` | `flexDirection: row` |
| `AddHorizontalContainer(marginBottom?)` | `VisualElement` | Row + 20px bottom margin |
| `AddScrollView(name?, additionalClasses?, greenDecorated?)` | `ScrollView` | |
| `AddGameScrollView(name?, additionalClasses?)` | `ScrollView` | |
| `AddListView(name?, additionalClasses?, greenDecorated?)` | `ListView` | |
| `AddCollapsiblePanel(title?, expand?, name?)` | `CollapsiblePanel` | |
| `AddFragment(background?, name?, additionalClasses?)` | `EntityPanelFragmentElement` | |

### Labels

| Method | Returns | Notes |
|---|---|---|
| `AddLabel(text?, name?, additionalClasses?, style?)` | `Label` | `GameLabelStyle.Default` |
| `AddLabelHeader(text?, name?, additionalClasses?)` | `Label` | `GameLabelStyle.Header` |
| `AddGameLabel(text?, name?, additionalClasses?, size?, color?, bold?, centered?)` | `Label` | Full control |

### Buttons

| Method | Returns | Notes |
|---|---|---|
| `AddButton(text?, name?, onClick?, additionalClasses?, style?, size?, stretched?)` | `NineSliceButton` | Action overload |
| `AddButton(text?, clickCb?, name?, additionalClasses?, style?, size?, stretched?)` | `NineSliceButton` | EventCallback overload |
| `AddMenuButton(text?, onClick?, ...)` | `NineSliceButton` | `GameButtonStyle.Menu` |
| `AddGameButton(text?, onClick?, ...)` | `NineSliceButton` | `GameButtonStyle.Game` |
| `AddGameButtonPadded(text?, onClick?, ..., paddingX?, paddingY?)` | `NineSliceButton` | Game + explicit padding |
| `AddEntityFragmentButton(text?, onClick?, name?, additionalClasses?, color?)` | `NineSliceButton` | Green or Red border |
| `AddCloseButton(name?)` | `Button` | Standard close `×` button |
| `AddPlusButton(name?, size?)` | `NineSliceButton` | |
| `AddMinusButton(name?, size?)` | `NineSliceButton` | |
| `AddAction(onClick)` | `T` (same button) | Chains onto any `Button` |

### Input fields

| Method | Returns | Notes |
|---|---|---|
| `AddTextField(name?, changeCallback?, additionalClasses?)` | `NineSliceTextField` | |
| `AddIntField(name?, changeCallback?, additionalClasses?)` | `NineSliceIntegerField` | |
| `AddFloatField(name?, changeCallback?, additionalClasses?)` | `NineSliceFloatField` | |

### Sliders

| Method | Returns | Notes |
|---|---|---|
| `AddSlider(label?, name?, additionalClasses?, values?)` | `GameSlider` | Float |
| `AddSliderInt(label?, name?, additionalClasses?, values?)` | `GameSliderInt` | Int |
| `AddMinMaxSlider(values?, changeCallback?, label?, name?, additionalClasses?)` | `MinMaxSlider` | |

### Dropdowns

| Method | Returns | Notes |
|---|---|---|
| `AddDropdown(name?, additionalClasses?)` | `Dropdown` | |
| `AddMenuDropdown(name?, additionalClasses?)` | `Dropdown` | `settings-dropdown` class |
| `AddDropdownRow<TValue>(label?, onChange?, veInit?, itemSetter?)` | `DropdownRow<TValue>` | |
| `AddDropdownRow<TValue>(values, veInit, itemSetter, label?, onChange?, generateUniqueNames?)` | `DropdownRow<TValue>` | Pre-populated |
| `AddDropdownRow<TValue>(values, textFn, veInit, itemSetter, label?, onChange?, generateUniqueNames?)` | `DropdownRow<TValue>` | Pre-populated |

### Images

| Method | Returns | Notes |
|---|---|---|
| `AddImage(name?, additionalClasses?)` | `Image` | Bare image |
| `AddImage(className, name?, additionalClasses?)` | `Image` | USS class as source |
| `AddImage(Sprite, name?, additionalClasses?)` | `Image` | |
| `AddImage(Texture, name?, additionalClasses?)` | `Image` | |
| `AddImage(path, IAssetLoader, name?, additionalClasses?)` | `Image` | Loads from mod assets |
| `AddImage(iconName, NamedIconProvider, name?, additionalClasses?)` | `Image` | Named icon |
| `AddIconSpan()` | `IconSpan` | Empty span |
| `AddIconSpan(ImageSource, prefix?, postfix?, size?)` | `IconSpan` | With content |
| `AddArrow(NamedIconProvider)` | `IconSpan` | Arrow glyph |
| `AddInventoryInputImage()` | `Image` | Inventory input tray |
| `AddInventoryOutputImage()` | `Image` | Inventory output tray |

### Progress bars

| Method | Returns | Notes |
|---|---|---|
| `AddProgressBar(name?, additionalClasses?)` | `ProgressBar` | `Timberborn.CoreUI.ProgressBar` (aliased `TProgressBar` internally) |
| `AddProgressBarWithLabel(color?, text?, name?, additionalClasses?)` | `ProgressBarWithLabel` | |
| `SetColor(color, removeColor?)` | same bar | Add or remove a color class |
| `SetProgress(progress, label?, text?, oldColor?, newColor?)` | same bar | |

### Styling (`Properties.cs`)

| Method | Notes |
|---|---|
| `AddClass(className)` | |
| `AddClasses(params …)` | |
| `SetMargin(…)` / `SetMarginBottom()` / `SetMarginRight()` / `SetMarginLeftAuto()` | |
| `SetPadding(…)` | |
| `SetWidth(f)` / `SetHeight(f)` / `SetSize(w, h)` | Pixel values |
| `SetWidthPercent(f)` / `SetHeightPercent(f)` / `SetSizePercent(w, h)` | |
| `SetMaxWidth/Height/Size(…)` / `SetMinSize(…)` / `SetMinMaxSize(…)` | |
| `SetAsRow()` | `flexDirection: row` |
| `SetFlexGrow(n?)` / `SetFlexShrink(n?)` / `SetWrap(bool?)` | |
| `SetDisplay(bool)` | `flex` or `none` |
| `AlignItems(Align)` / `JustifyContent(Justify)` | |
| `SetBorder(color?, width?)` | |
| `SetName(string)` | |
| `Initialize(VisualElementInitializer)` / `Initialize(VisualElementLoader)` | Required for sliders, dropdowns, and fragment roots |

---

## Key types

| Type | Role | Source |
|---|---|---|
| `GameLabelStyle` | Label visual variant enum | [`UiEnums.cs`](../../TimberUi/TimberUi/UiEnums.cs) |
| `GameLabelSize` | Normal / Big | [`UiEnums.cs`](../../TimberUi/TimberUi/UiEnums.cs) |
| `GameLabelColor` | Default / Yellow | [`UiEnums.cs`](../../TimberUi/TimberUi/UiEnums.cs) |
| `GameButtonStyle` | Button variant enum | [`UiEnums.cs`](../../TimberUi/TimberUi/UiEnums.cs) |
| `GameButtonSize` | Medium / Small / Large | [`UiEnums.cs`](../../TimberUi/TimberUi/UiEnums.cs) |
| `TimberbornTextColor` | Green / Red / Yellow | [`Texts.cs`](../../TimberUi/TimberUi/Extensions/Texts.cs) |
| `ProgressBarColor` | Green / Teal / Red / Blue | [`ProgressBars.cs`](../../TimberUi/TimberUi/Extensions/ProgressBars.cs) |
| `EntityFragmentButtonColor` | Green / Red | [`Buttons.cs`](../../TimberUi/TimberUi/Extensions/Buttons.cs) |
| `ToggleStyle` | Settings | [`UiEnums.cs`](../../TimberUi/TimberUi/UiEnums.cs) |
| `GameSlider` / `GameSliderInt` | Styled slider widget | [`CommonUi/GameSlider.cs`](../../TimberUi/TimberUi/CommonUi/GameSlider.cs) |
| `SliderValues<TValue>` | `(Low, High, Default)` range record | [`CommonUi/GameSlider.cs`](../../TimberUi/TimberUi/CommonUi/GameSlider.cs) |
| `MinMaxSliderValues` | `(Value, Min, Max)` for range sliders | [`Extensions/Sliders.cs`](../../TimberUi/TimberUi/Extensions/Sliders.cs) |
| `DropdownRow<TValue>` | Typed label + dropdown | [`CommonUi/DropdownRow.cs`](../../TimberUi/TimberUi/CommonUi/) |
| `DropdownRowItem<TValue>` | `(TValue Value, string Text)` item record | [`CommonUi/DropdownRow.cs`](../../TimberUi/TimberUi/CommonUi/) |
| `IndexedDropdownRowItem<TValue>` | `(int Index, DropdownRowItem<TValue> Item)` in change events | [`CommonUi/DropdownRow.cs`](../../TimberUi/TimberUi/CommonUi/) |
| `IconSpan` | Image + optional label row | [`CommonUi/IconSpan.cs`](../../TimberUi/TimberUi/CommonUi/) |
| `ImageSource` | `Sprite` / `Texture` / `VectorImage` union | [`CommonUi/IconSpan.cs`](../../TimberUi/TimberUi/CommonUi/) |
| `ProgressBarWithLabel` | `(TProgressBar, Label)` record | [`CommonUi/`](../../TimberUi/TimberUi/CommonUi/) |
| `CollapsiblePanel` | Collapsible header + body | [`CommonUi/`](../../TimberUi/TimberUi/CommonUi/) |
| `EntityPanelFragmentElement` | Styled fragment root container | [`CommonUi/`](../../TimberUi/TimberUi/CommonUi/) |
| `EntityPanelFragmentBackground` | Background color enum | [`CommonUi/`](../../TimberUi/TimberUi/CommonUi/) |
| `UiCssClasses` | USS class name constants | [`UiCssClasses.cs`](../../TimberUi/TimberUi/UiCssClasses.cs) |
