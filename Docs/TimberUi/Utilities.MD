# Utilities

TimberUi ships a grab-bag of small, self-contained utilities that many mods find useful. This page covers localization shortcuts, persistence pair helpers, serializable numeric collections, collection utilities, WAV audio loading, the interactive entity-select tool, text-to-texture rendering, and UXML/USS tree-inspection tools.

---

## Localization helpers

**Source:** [`Extensions/Loc.cs`](../../TimberUi/TimberUi/Extensions/Loc.cs)  
**Namespace:** `Timberborn.Localization` (methods appear on `string` and `ILoc` — no extra `using` needed)

The `T` family turns a localization key string into its translated value using an injected `ILoc`, mirroring Timberborn's own `ILoc.T(key)` call but written as an extension on the key rather than the service.

```cs
// Translate a key with no substitution
string label = "MyMod.SomeKey".T(loc);

// Translate with up to three substitution arguments
string msg = "MyMod.Greeting".T(loc, playerName);

// Translate with a format-string style (any number of args)
string fmt = "MyMod.Report".TFormat(loc, count, total);
```

`ILoc` also gains extension methods for the five most common game strings. These are cached after their first call:

```cs
string yes    = loc.TYes();     // "Core.Yes"
string no     = loc.TNo();      // "Core.No"
string ok     = loc.TOK();      // "Core.OK"
string cancel = loc.TCancel();  // "Core.Cancel"
string none   = loc.TNone();    // "Inventory.NothingSelected"

// Convenience boolean → yes/no
string label = loc.TYesNo(building.IsEnabled);
```

`Priority` and `PopulationCounterMode` values also get localization extensions:

```cs
string priorityLabel = Priority.High.T(loc);
string modeLabel = PopulationCounterMode.Beavers.T(loc);
```

> The `TYes`/`TNo`/`TOK`/`TCancel`/`TNone` results are cached in static fields on first access. They are suitable for UIs that remain in one language for the lifetime of a play session.

---

## Persistence pair helpers

**Source:** [`Extensions/Persistences.cs`](../../TimberUi/TimberUi/Extensions/Persistences.cs)  
**Namespace:** `Timberborn.Persistence`

When you need to save a key-value pair into a Timberborn save file, `SetPair`/`GetPair` serialize the pair as a single `|`-delimited string under one `PropertyKey<string>`. Both key and value must implement `IConvertible` (all primitive numeric types, `string`, `bool`, and `enum` qualify).

```cs
// Saving
public void Save(IObjectSaver saver)
{
    saver.SetPair(_mappingKey, new KeyValuePair<string, int>("iron", 42));
}

// Loading
public void Load(IObjectLoader loader)
{
    KeyValuePair<string, int> pair = loader.GetPair<string, int>(_mappingKey);
}
```

To save or load a whole list of pairs at once, use `SetPairs`/`GetPairs` with a `ListKey<string>`:

```cs
// Saving a collection
saver.SetPairs(_allMappingsKey, myDictionary);

// Loading a collection back into a dictionary
var pairs = loader.GetPairs<string, int>(_allMappingsKey);
var dict = pairs.ToDictionary(p => p.Key, p => p.Value);
```

The default separator is `|` (`TimberUIPersistencesExtensions.DefaultSeparator`). You can supply a different `char` as an optional third argument. Keep keys and values free of the separator character — the implementation is a plain split with no escaping. A separator embedded in a key or value corrupts the round-trip, and a malformed entry throws `IndexOutOfRangeException` on load.

---

## Serializable collections

**Source:** [`Helpers/SerializableFloats.cs`](../../TimberUi/TimberUi/Helpers/SerializableFloats.cs), [`Helpers/SerializableInts.cs`](../../TimberUi/TimberUi/Helpers/SerializableInts.cs)  
**Namespace:** `TimberUi.Helpers`

`SerializableFloats` and `SerializableInts` are `readonly record struct` types that hold four numeric components (X, Y, Z, W) and serialize them as a semicolon-delimited string. They plug directly into Timberborn's `IValueSerializer<T>` system, making it straightforward to persist any Unity vector, color, quaternion, or rect into a save file.

Both types carry broad implicit conversions so you usually never need to construct them explicitly:

| Implicit conversion | `SerializableFloats` | `SerializableInts` |
|---|:---:|:---:|
| `Vector2` / `Vector2Int` | Yes | Yes |
| `Vector3` / `Vector3Int` | Yes | Yes |
| `Vector4` | Yes | — |
| `Color` | Yes | — |
| `Quaternion` | Yes | — |
| `Rect` / `RectInt` | Yes | Yes |
| `SerializableInts` → `SerializableFloats` | Yes | — |

### Saving a `Color`

Register the serializer with Timberborn's `IObjectSaver`/`IObjectLoader` pattern:

```cs
static readonly PropertyKey<SerializableFloats> ColorKey = new("HighlightColor");
static readonly IValueSerializer<SerializableFloats> Serializer = SerializableFloats.Serializer;

public void Save(IObjectSaver saver)
{
    saver.Set(ColorKey, (SerializableFloats)myColor, Serializer);
}

public void Load(IObjectLoader loader)
{
    Color loaded = loader.Get(ColorKey, Serializer); // implicit Color cast
}
```

### Saving a `Vector3Int`

```cs
static readonly PropertyKey<SerializableInts> PositionKey = new("Position");
static readonly IValueSerializer<SerializableInts> Serializer = SerializableInts.Serializer;

saver.Set(PositionKey, (SerializableInts)myVector3Int, Serializer);
Vector3Int pos = loader.Get(PositionKey, Serializer); // implicit Vector3Int cast
```

`Deconstruct` overloads (2-, 3-, and 4-component) let you unpack directly into tuples:

```cs
var (x, y) = (SerializableFloats)someVector2;
var (x, y, z) = (SerializableFloats)someVector3;
```

---

## Collection utilities

**Source:** [`Helpers/ListExtensions.cs`](../../TimberUi/TimberUi/Helpers/ListExtensions.cs), [`Helpers/TimberUiUtils.cs`](../../TimberUi/TimberUi/Helpers/TimberUiUtils.cs)  
**Namespace:** `TimberUi.Helpers` / `TimberUi`

### `ListExtensions`

Two LINQ-style extensions using C# 13 `extension` block syntax:

```cs
// Pick a random element from any IReadOnlyList<T>
// Returns default(T) when the list is empty.
string? pick = myList.Randomize();

// First-match index (returns -1 if not found)
int idx = myItems.FindIndex(item => item.Name == "copper");
```

### `TimberUiUtils` — standard UI colors and miscellany

`TimberUiUtils` (namespace `TimberUi`) provides named `Color` constants that match Timberborn's visual language:

| Constant | Usage |
|---|---|
| `SuccessColor` | Green — positive, OK |
| `NeutralColor` | Light grey — informational |
| `WarningColor` | Amber — caution |
| `DangerColor` | Dark red — destructive or error |
| `TransparentColor` | Fully transparent black |

```cs
highlight.SetBackgroundColor(TimberUiUtils.WarningColor);
```

Other utilities on `TimberUiUtils`:

```cs
// Check whether MoreModLogs is loaded (gates verbose logging)
if (TimberUiUtils.HasMoreModLogs) { /* ... */ }

// Load a WAV file as an AudioClip (delegates to WavUtility)
AudioClip clip = TimberUiUtils.LoadAudioClipFrom("path/to/sound.wav", name: "MySound");
AudioClip clip2 = TimberUiUtils.LoadAudioClipFrom(wavBytes, name: "MySound");

// Get all values of an enum, sorted
ImmutableArray<Priority> values = TimberUiUtils.GetSortedEnumValues<Priority>();
```

`LogVerbose` emits a `Debug.Log` only when MoreModLogs is loaded — convenient for high-frequency diagnostics that should stay quiet in production:

```cs
TimberUiUtils.LogVerbose(() => $"Processed {count} items.");
```

---

## WAV audio loading

**Source:** [`Helpers/WavUtility.cs`](../../TimberUi/TimberUi/Helpers/WavUtility.cs)  
**Namespace:** `TimberUi.Helpers`

`WavUtility` is a static utility (ported from [deadlyfingers/UnityWav](https://github.com/deadlyfingers/UnityWav)) that converts uncompressed PCM WAV data to and from Unity `AudioClip` objects. It supports 8-bit, 16-bit, 24-bit, and 32-bit PCM; only uncompressed formats (format code 1 or WaveFormatExtensible 65534) are intended to be used. Note that the format validation is implemented with `Debug.Assert*` calls, which are stripped in release builds — malformed or compressed input is not reliably rejected at runtime. Validate your input beforehand.

The preferred entry point for most mods is `TimberUiUtils.LoadAudioClipFrom`, which delegates here, but you can also call `WavUtility` directly:

```cs
// Load from a file path
AudioClip clip = WavUtility.ToAudioClip("path/to/alert.wav");

// Load from raw bytes (e.g. embedded resource)
AudioClip clip = WavUtility.ToAudioClip(wavBytes, name: "Alert");

// Encode an AudioClip back to 16-bit PCM WAV bytes
byte[] encoded = WavUtility.FromAudioClip(clip);

// Encode and save to Application.persistentDataPath/recordings/
byte[] encoded = WavUtility.FromAudioClip(clip, out string? savedPath, saveAsFile: true);
```

`FromAudioClip` always outputs 16-bit PCM regardless of the original clip's bit depth. Use `saveAsFile: false` when you only need the byte array.

---

## Entity-select tool

**Source:** [`Tools/SelectEntityTool.cs`](../../TimberUi/TimberUi/Tools/SelectEntityTool.cs)  
**Namespace:** `TimberUi.Tools`

`SelectEntityTool` is an injectable singleton tool that lets a player click on an entity in the game world and returns the result asynchronously. It handles cursor switching, raycasting, rollover highlighting, and re-selecting the original entity when the tool exits — you just `await` the result.

It is registered automatically for the Game context via `[BindSingleton(Contexts = BindAttributeContext.NonMenu)]`. Inject it wherever you need it.

### Basic usage

```cs
[BindSingleton]
public class MyService(SelectEntityTool selectTool)
{
    public async Task PickTarget()
    {
        EntityComponent? entity = await selectTool.SelectAsync();

        if (entity is null)
        {
            return; // player cancelled or switched tool
        }

        // entity is the EntityComponent the player clicked
    }
}
```

### Configuring the selection session

Pass a `SelectEntityToolOptions` to control behavior:

```cs
// Injected: SelectEntityTool selectTool
EntityComponent? picked = await selectTool.SelectAsync(
    new SelectEntityToolOptions(Cursor: "PickObjectCursor"));
if (picked is null) { return; } // cancelled, or the object had no EntityComponent
```

All fields of `SelectEntityToolOptions` (`Source`, `Cursor`, `HighlightColor`, `Filter`) are optional. `Filter` receives the `SelectableObject` under the cursor; objects that fail the filter are not highlighted and cannot be selected.

`Source` is a `BaseComponent?` that is re-selected (re-opening its panel) when the tool exits — pass a real `BaseComponent` instance or omit it. Do not pass `this` from a plain service class that is not itself a `BaseComponent`. A `null` result means either the player cancelled or the clicked object had no `EntityComponent` attached.

---

## Text-to-texture rendering

**Source:** [`Services/TextTexture/TextTexture.cs`](../../TimberUi/TimberUi/Services/TextTexture/TextTexture.cs), [`TextTextureRenderer.cs`](../../TimberUi/TimberUi/Services/TextTexture/TextTextureRenderer.cs)  
**Namespace:** `TimberUi.Services`

The text-texture subsystem rasterizes an arbitrary string into a Unity `Texture2D` using an OS-installed font. This is useful for in-world signage, custom material maps, or any situation where text must live in a texture rather than in UI Toolkit.

The subsystem is a two-class API: `TextTextureRenderer` (an exported bootstrapper singleton) allocates and manages `TextTexture` instances; `TextTexture` is a disposable wrapper around a `Texture2D` that you `Render` to.

### Creating a `TextTexture`

Inject `TextTextureRenderer` and call `Create`:

```cs
[BindSingleton]
public class MySignService(TextTextureRenderer textRenderer) : IUnloadableSingleton
{
    TextTexture? _sign;

    public void Load()
    {
        // 256×64 px, prefer a monospace font at 48pt
        _sign = textRenderer.Create(256, 64, TextTextureRenderer.MonospaceFonts, fontSize: 48);
    }

    public void Unload()
    {
        _sign?.Dispose();
        _sign = null;
    }
}
```

`MonospaceFonts` and `DefaultFonts` are static `string[]` arrays listing preferred OS font names in priority order. Unity picks the first available. You can supply your own list.

### Rendering text

Once you have a `TextTexture`, call `Render`. Subsequent calls with the same content and options are no-ops — the result is cached until content or options change.

```cs
// Render with default options (word-wrap, center alignment, tab size 4)
_sign.Render("Hello, Timberborn!");

// Render with custom options
var options = new TextTextureRenderOptions(
    TabSize: 4,
    WrapMode: TextTextureRenderWrapMode.Space,
    HorizontalAlignment: TextTextureRenderAlignment.Start,
    VerticalAlignment: TextTextureRenderAlignment.Center
);
_sign.Render("Line one\nLine two", options);

// Use the texture on a material
myRenderer.material.mainTexture = _sign.Texture;
```

`WrapMode` options:

| Value | Behaviour |
|---|---|
| `None` | No wrapping — only `\n` breaks lines |
| `Space` | Word-wrap at spaces (default) |
| `Anywhere` | Break at any character boundary |

### Measuring text

`MeasureUnbound` returns the pixel size a string would occupy with no wrapping constraints, at the texture's font and size:

```cs
Vector2Int size = _sign.MeasureUnbound("Hello!");
```

### Glyph appearance

Glyphs are rendered as a white mask whose alpha channel preserves the source font's anti-aliased coverage — only the source color/hue is discarded. Tint the texture downstream via material color or a UI `Image` tint color.

### Disposal and ref-counting

Each `TextTexture` holds a reference to a shared per-font glyph cache. The cache is freed automatically when every `TextTexture` that uses a given font and size is disposed. Always dispose `TextTexture` instances when they are no longer needed, either explicitly or from `IUnloadableSingleton.Unload`.

---

## Visual-tree debugging

**Source:** [`UnityUiElements/UxmlExporter.cs`](../../TimberUi/TimberUi/UnityUiElements/UxmlExporter.cs), [`UnityUiElements/StyleSheetToUss.cs`](../../TimberUi/TimberUi/UnityUiElements/StyleSheetToUss.cs), [`Extensions/Helpers.cs`](../../TimberUi/TimberUi/Extensions/Helpers.cs)  
**Namespaces:** `UnityEditor.UIElements.Debugger`, `UnityEditor.StyleSheets`, `UnityEngine.UIElements`

These tools let you dump a live UI tree to UXML text and convert individual `StyleSheet` objects to USS text at runtime — useful for understanding Timberborn's own panel structure when writing matching mod UI.

### Dumping a visual tree

The quickest path is the `PrintVisualTree` / `DescribeVisualTree` extensions that live on any `VisualElement` (via `Helpers.cs`):

```cs
// Log the UXML of an entire panel to the Unity console
myPanel.PrintVisualTree();

// Note: the bool overload always includes TemplateContainer contents regardless of the argument;
// prefer the (templateId, options) overload of UxmlExporter.Dump for explicit control.
myPanel.PrintVisualTree(printTemplates: true);

// Retrieve the UXML string without logging
string uxml = myPanel.DescribeVisualTree();

// Pass explicit export options
string uxml = myPanel.DescribeVisualTree(
    options: UxmlExporter.ExportOptions.NewLineOnAttributes | UxmlExporter.ExportOptions.AutoNameElements
);
```

You can also call `UxmlExporter.Dump` directly when you need more control:

```cs
string uxml = UxmlExporter.Dump(
    myElement,
    templateId: "MyMod.MyPanel",
    options: UxmlExporter.ExportOptions.PrintTemplate
);
```

`ExportOptions` is a `[Flags]` enum:

| Flag | Effect |
|---|---|
| `NewLineOnAttributes` | Each XML attribute on its own line |
| `StyleFields` | Declared but not yet fully implemented |
| `AutoNameElements` | Emits a synthetic `name` attribute (TypeName + text) for elements that are unnamed or whose name starts with `_` |
| `PrintTemplate` | Recurse into `TemplateContainer` children |

### Converting a `StyleSheet` to USS text

`StyleSheetToUss` provides granular overloads for converting individual selectors, rules, and value handles back to USS text. Work at the rule or selector level — the whole-`StyleSheet` `ToUssString(StyleSheet, …)` overload and `WriteStyleSheet` unconditionally throw `NotSupportedException`, as do the obsolete `Print`/`Describe`/`PrintStylesheet` extension helpers that route through them. Only the per-rule, per-selector, per-value, and `Color` overloads are functional.

```cs
var sb = new StringBuilder();
var exportOptions = new UssExportOptions();

// Emit a single rule
StyleSheetToUss.ToUssString(styleSheet, exportOptions, someRule, sb);
string ruleText = sb.ToString();

// Emit a single selector
string selector = StyleSheetToUss.ToUssSelector(complexSelector);

// Emit a Color value
string colorText = StyleSheetToUss.ToUssString(Color.red, useColorCode: true);
```

Use `UssExportOptions` to control formatting:

| Property | Default | Description |
|---|---|---|
| `propertyIndent` | `"    "` | Indentation string for each property |
| `useColorCode` | `false` | Emit colors as hex (`#rrggbbaa`) instead of `rgb()`/`rgba()` |
| `withComment` | `true` | Include comments from `UssComments` |
| `exportDefaultValues` | `true` | Include properties whose value is the USS default |

---

## Color conversions

**Source:** [`Extensions/Conversions.cs`](../../TimberUi/TimberUi/Extensions/Conversions.cs)  
**Namespace:** `UnityEngine` (methods appear on `Color`, `Vector3`, `Vector4`, `Vector3Int` — no extra `using` needed)

A small set of extensions for converting between `Color` and Unity vector types, in two directions.

### Normalized (0–1) conversions

```cs
Color c = new Color(0.2f, 0.4f, 0.8f, 1f);

Vector3 v3 = c.ToVector3();   // (r, g, b) — alpha dropped
Vector4 v4 = c.ToVector4();   // (r, g, b, a)

Color fromV3 = v3.ToColor();  // alpha defaults to 1
Color fromV4 = v4.ToColor();  // all four components
```

### 0–255 conversions

Note the direction-dependent return types carefully: converting a `Color` to 255-range yields a vector; converting a vector to a `Color` divides by 255.

```cs
Color c = new Color(0f, 0.502f, 1f);  // roughly (0, 128, 255) in byte terms

Vector3    asFloat = c.ToColor255();         // (0f, 128.0f, 255f)   — float components, returns Vector3
Vector3Int asInt   = c.ToColor255Int();      // (0, 128, 255)         — rounded to int, returns Vector3Int

Color fromFloat = new Vector3(0f, 128f, 255f).ToColor255();         // divides by 255 → Color
Color fromInt   = new Vector3Int(0, 128, 255).ToColor255();         // divides by 255 → Color
```

| Extension | Receiver | Returns | Direction |
|---|---|---|---|
| `ToVector3()` | `Color` | `Vector3` (r,g,b) | Color → vector |
| `ToVector4()` | `Color` | `Vector4` (r,g,b,a) | Color → vector |
| `ToColor()` | `Vector3` | `Color` | vector → Color |
| `ToColor()` | `Vector4` | `Color` | vector → Color |
| `ToColor255()` | `Color` | `Vector3` (×255) | Color → 0–255 float vector |
| `ToColor255Int()` | `Color` | `Vector3Int` (×255, rounded) | Color → 0–255 int vector |
| `ToColor255()` | `Vector3` | `Color` (÷255) | 0–255 float vector → Color |
| `ToColor255()` | `Vector3Int` | `Color` (÷255) | 0–255 int vector → Color |

---

## Key types

| Type | Namespace | Description |
|---|---|---|
| `UiBuilderExtensions` (Loc portion) | `Timberborn.Localization` | `string.T(ILoc, …)` and `ILoc.TYes/TNo/TOK/TCancel/TNone` |
| `TimberUIPersistencesExtensions` | `Timberborn.Persistence` | `IObjectSaver.SetPair/SetPairs`, `IObjectLoader.GetPair/GetPairs` |
| `SerializableFloats` | `TimberUi.Helpers` | 4-component float record struct with Unity type conversions and `IValueSerializer` |
| `SerializableInts` | `TimberUi.Helpers` | 4-component int record struct with Unity type conversions and `IValueSerializer` |
| `ListExtensions` | `TimberUi.Helpers` | `IReadOnlyList<T>.Randomize()`, `IEnumerable<T>.FindIndex(Predicate<T>)` |
| `TimberUiUtils` | `TimberUi` | UI color constants, `LoadAudioClipFrom`, `GetSortedEnumValues<T>`, `LogVerbose` |
| `WavUtility` | `TimberUi.Helpers` | PCM WAV ↔ `AudioClip` encode/decode |
| `SelectEntityTool` | `TimberUi.Tools` | Awaitable entity-pick tool |
| `SelectEntityToolOptions` | `TimberUi.Tools` | Options for a `SelectEntityTool` session |
| `TextTextureRenderer` | `TimberUi.Services` | Bootstrapper singleton — allocates `TextTexture` instances |
| `TextTexture` | `TimberUi.Services` | Per-texture render target; call `Render(content)` and read `Texture` |
| `TextTextureRenderOptions` | `TimberUi.Services` | Wrap mode, alignment, tab size |
| `TextTextureRenderWrapMode` | `TimberUi.Services` | `None`, `Space`, `Anywhere` |
| `TextTextureRenderAlignment` | `TimberUi.Services` | `Start`, `Center`, `End` |
| `UxmlExporter` | `UnityEditor.UIElements.Debugger` | Dumps a `VisualElement` tree to UXML string |
| `StyleSheetToUss` | `UnityEditor.StyleSheets` | Converts `StyleSheet` objects to USS text |
| `UssExportOptions` | `UnityEditor.StyleSheets` | Formatting options for USS export |
| `TimberUIHelperExtensions` (Conversions portion) | `UnityEngine` | `Color`↔`Vector3/4` and 0–255 conversions |
