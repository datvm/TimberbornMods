# UnityUiElements

## Purpose
Debugging and export tooling that reconstructs USS (stylesheet) and UXML (layout) text from live runtime UIElements objects. `StyleSheetToUss` serializes a `StyleSheet` back into USS source text; `UxmlExporter` walks a `VisualElement` tree and dumps it as a UXML XML document. These let a developer inspect or extract the actual styles and layout the game UI is built from at runtime — useful for reverse-engineering Timberborn's panels when authoring matching mod UI.

Both files are lightly trimmed copies of Unity's own internal editor source (UnityCsReference), reproduced here so the code can run against the game's UIElements assemblies without the Unity Editor.

## Key types
- **`StyleSheetToUss`** (`StyleSheetToUss.cs`) — static converter. Overloads of `ToUssString(...)` turn `Color`, individual `StyleValueHandle`s, value arrays, `StyleRule`s, selectors (`StyleComplexSelector`/`StyleSelectorPart[]`), and properties into USS text via `StringBuilder`. `ToUssSelector(StyleComplexSelector)` returns a selector string. Handles USS specifics: rgb/rgba/hex color emission, `cursor` multi-value property special-casing, functions with arg counts, comma separators.
- **`UssComments`** / **`UssExportOptions`** (`StyleSheetToUss.cs`) — comment storage (per-rule and per-property dictionaries) and export configuration (indent, color-code mode, whether to emit comments/default values).
- **`UxmlExporter`** (`UxmlExporter.cs`) — static `Dump(VisualElement, templateId, ExportOptions)` builds an `XDocument` by recursively walking the element tree, emitting `name`/`text`/`class` attributes, namespace prefixes (`ui` → `UnityEngine.UIElements`), and `<Using>` aliases for templates. `ExportOptions` (`[Flags]`) controls newline-on-attributes, style fields, auto-naming, and whether to expand `TemplateContainer` contents.

## How it fits together
These are standalone, leaf debugging utilities — nothing else in the folder consumes them, and they are invoked on demand. `UxmlExporter.Recurse` descends a `VisualElement` hierarchy; `StyleSheetToUss` walks the parallel `StyleSheet` model (selectors → rules → properties → value handles). Together they recover the two halves (layout + style) of a UIElements UI as text. They depend entirely on Unity's UIElements object model being accessible at runtime.

## Dependencies & patterns
- **Namespaces are Unity's own:** `StyleSheetToUss.cs` declares `namespace UnityEditor.StyleSheets`; `UxmlExporter.cs` declares `namespace UnityEditor.UIElements.Debugger`. These namespaces are inherited from the upstream Unity source files — the code does not reference the Unity Editor assembly and compiles against the game's `UnityEngine.UIElements` runtime types. The editor namespace is cosmetic/historical.
- UIElements model types: `StyleSheet`, `StyleRule`, `StyleProperty`, `StyleValueHandle`, `StyleComplexSelector`, `StyleSelectorPart`, `StyleSelectorRelationship`, `StyleValueType`, `VisualElement`, `TemplateContainer`, `TextElement`.
- `System.Xml`/`System.Xml.Linq` (`XDocument`, `XElement`, `XmlWriter`) for UXML; `StringBuilder` + `CultureInfo.InvariantCulture` for USS.

## Notes
- USS color emission rounds float components to bytes with `MidpointRounding.AwayFromZero`. The exporter intends to emit `rgba(...)` when `a != 1` (alpha to 2 decimals), but `StyleSheetToUss.ToUssString(Color)` currently throws for non-1 alpha due to a formatting bug (string alpha passed to a `{3:F2}` format specifier). Hex output requires `useColorCode = true`.
- `UxmlExporter.Recurse` skips element names beginning with `'_'` (treated as internal/auto-generated) unless `AutoNameElements` is set.
- `ToUssString(StyleValueHandle)` throws `ArgumentException` on an unhandled `StyleValueType`, so USS export will surface loudly if an unsupported value type is encountered.
- These utilities assume a well-formed UIElements hierarchy; no error handling is provided for malformed or cyclic trees.
