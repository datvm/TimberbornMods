# TextTexture

## Purpose

This subsystem rasterizes arbitrary strings into a Unity `Texture2D` using an OS-installed font, with full control over wrapping, alignment, tab size, and font size. It exists because Timberborn UI (UI Toolkit / IMGUI) cannot trivially render text into a texture for use as a sprite or material map, and modders sometimes need text baked into a texture (e.g. in-world signage, custom widgets). The pipeline pulls glyphs from Unity's dynamic font atlas, re-renders each glyph into its own tightly-cropped `Texture2D`, lays the glyphs out into lines, and stamps them onto a destination texture via `Graphics.CopyTexture`.

It is a multi-stage pipeline: font cache management → glyph extraction/rendering → layout (line breaking + alignment) → stamping. Two singletons (`TextTextureRenderer`, `TextTextureFontService`) own shared state; per-text instances are `TextTexture` objects you allocate and dispose.

## Key types

- **`TextTextureRenderer`** (singleton, `IUnloadableSingleton`, bound in `Bootstrapper` context, `Exported`): the orchestrator. Owns the `caches` dictionary keyed by `FontDefinitionPair`. Creates `TextTexture` instances, extracts/renders missing glyphs from the Unity font atlas, measures text, and ref-counts caches for disposal. Holds the char constants (`NewLine`, `Tab`, `CarriageReturn`, `Space`) and `GlyphTextureFormat = RGBA32`.
- **`TextTexture`** (`IDisposable`): one rendered text target. Wraps a destination `Texture2D` of fixed `width`×`height`. `Render(content, options)` lays out and stamps; `Clear()` blits a cached blank texture; `MeasureUnbound` measures without wrapping bounds. Caches `Content`/`currOptions` to skip redundant re-renders.
- **`TextGlyphCache`** (`IDisposable`): per-font (name+size) glyph store. Holds the Unity `Font`, a `Dictionary<char, TextGlyphCacheEntry> glyphs`, `LineHeight`, `SpaceWidth`, and a `cacheUse` ref-count. `GetMissingGlyphs` yields chars not yet rendered. Disposing destroys every glyph texture and the font.
- **`TextGlyphCacheEntry`** (record struct): one glyph = `char C`, its cropped `Texture2D`, and the Unity `CharacterInfo` (advance, bearings, uv rect).
- **`GlyphLayout`** (static): pure layout engine. `Layout(...)` produces a `TextGlyphLayout`. Internally builds "raw lines" via one of three wrap strategies, computes vertical metrics from top/bottom bearings, applies horizontal and vertical alignment, and emits absolute top-left glyph positions.
- **`TextGlyphLayout` / `TextGlyphLayoutLine` / `TextGlyphLayoutGlyph`** (LayoutModels.cs): immutable layout result — array of placed glyphs (char, entry, top-left `Vector2Int` position, advance), array of lines (start index, count, width, Y), and total `Size`.
- **`TextTextureFontService`** (singleton, `Bootstrapper`, `Exported`): OS font discovery. Enumerates `Font.GetOSInstalledFontNames()`, resolves a preferred monospace and default font from per-OS preference lists, and can populate a `DropdownRow<string>`.
- **`TextTextureRenderOptions`** (record struct): `TabSize` (default 4), `WrapMode`, horizontal and vertical `Alignment`. `Default` = center/center, space-wrap.
- **`TextTextureRenderWrapMode`**: `None`, `Space`, `Anywhere`.
- **`FontDefinitionPair`** (record struct): `(string Name, int Size)` — the cache key.

## How it fits together

End-to-end render pipeline:

1. **Allocation** — `TextTextureRenderer.Create(width, height, fontNames, fontSize)` calls `Font.CreateDynamicFontFromOSFont(fontNames, size)` (Unity picks the first available from the name list). It builds a `FontDefinitionPair` from the **actual resolved** `font.name` plus the requested size, looks up or creates a `TextGlyphCache`, immediately caches the space glyph (so `SpaceWidth` is valid; `LineHeight` comes straight from `font.lineHeight` and is valid as soon as the font loads), increments `cacheUse`, and returns a `TextTexture` bound to that font def. The `TextTexture` constructor allocates its destination `Texture2D` (RGBA32, point filter, clamp).

2. **Glyph extraction** (`GetGlyphMap` → `RenderMissingGlyphs`) — on `Render`, the renderer computes the unique non-whitespace chars in the content (`GetUniqueGlyphs`), finds which are missing from the cache, and renders them. `RenderMissingGlyphs` calls `font.RequestCharactersInTexture(...)` to force Unity to pack the glyphs into the font's dynamic atlas, then grabs `font.material.mainTexture` as the atlas. For each char it reads `CharacterInfo` (advance, min/max X/Y bearings, uv corners).

3. **Per-glyph re-render** (`RenderGlyphTexture`) — rather than referencing the shared atlas (which moves as it grows), each glyph is baked into its own standalone `Texture2D`. A temporary ARGB32 `RenderTexture` sized to the glyph is activated, cleared, and a single textured `GL.QUADS` is drawn mapping the glyph's four uv corners (`uvBottomLeft`/`Right`, `uvTopRight`/`Left`) to the quad corners — this preserves any rotation or mirroring the atlas packer applied. Pixels are `ReadPixels`'d back, then **alpha-extracted**: each output pixel becomes solid white with alpha = max(r,g,b,a) of the source. This produces a white glyph mask that can be tinted later via material color. Zero-size glyphs (e.g. space) get a 1×1 transparent texture via `CreateEmptyGlyphTexture`.

4. **Layout** (`GlyphLayout.Layout`) — `BuildRawLines` dispatches on `WrapMode`:
   - `None` (`BuildUnwrappedLines`): only `\n` breaks lines.
   - `Anywhere` (`BuildAnywhereWrappedLines`): breaks mid-content when the next glyph would exceed `width`.
   - `Space` (`BuildSpaceWrappedLines`): accumulates words (whitespace-delimited), flushes word-by-word; words longer than `width` fall back to character breaking; trims leading/trailing spaces at break points.

   Line height = `RoundToInt(cache.LineHeight)`. Vertical extent uses the max top bearing (`info.maxY`) and max bottom bearing (`-info.minY`) across all glyphs. `Align()` computes start offsets for `Start`/`Center`/`End`. Each glyph gets an absolute **top-left** position: `x = penX + info.minX`, `y = baselineY - info.maxY`. Whitespace chars only advance the pen.

5. **Stamping** (`TextTexture.StampGlyph`) — for each placed glyph, the top-left layout Y is converted to `Graphics.CopyTexture`'s **bottom-left** convention (`dstY = height - pos.y - src.height`). The glyph rect is clipped against the destination bounds (partial glyphs at edges are cropped), then `Graphics.CopyTexture` blits the glyph texture region into the destination. `Render` first `Clear()`s by copying a cached all-zero "clear texture" of the same dimensions.

6. **Measurement** (`TextTextureRenderer.Measure`) — independent of bounds; walks the string accumulating advances and line heights (with optional `scale`), handling `\n`/`\r`/tab/space, to return a `Vector2Int` size. Used by `TextTexture.MeasureUnbound`.

7. **Disposal / ref-counting** — `TextTexture.Dispose` destroys its destination texture and calls `renderer.OnTextTextureDisposed`, which decrements the font cache's `cacheUse`; when it hits zero the whole `TextGlyphCache` (all glyph textures and the font) is disposed and removed. `Unload()` (singleton teardown) disposes the static clear-texture cache and every font cache.

## Dependencies & patterns

- **Unity**: `Font`, `CharacterInfo`, `Texture2D`, `RenderTexture`, `GL`, `Graphics.CopyTexture`, `Material`. Heavy use of immediate-mode GL for glyph baking.
- **DI**: `[BindSingleton(Contexts = BindAttributeContext.Bootstrapper, Exported = true)]` on the two services — these live at bootstrapper scope and are exported to dependent mods.
- **Bartholomew/IoC lifecycle**: `TextTextureRenderer : IUnloadableSingleton` → `Unload()` cleans up native resources on mod unload.
- **Logging**: `TimberUiUtils.LogVerbose`, `Debug.LogWarning`/`LogError`.
- **Collections**: `FrozenSet`/`ImmutableArray` for the font services; `record struct` value types throughout.
- **Ref-counting cache** pattern: shared per-font caches keyed by `(name, size)`, reference-counted by the number of live `TextTexture`s, freed at zero. Glyphs within a cache are rendered lazily and never evicted until the whole cache is freed.
- **Static texture pools**: blank "clear" textures are cached per `(width, height)` across all `TextTexture` instances (static dict on `TextTexture`).

## Notes

- **Cache key uses resolved font name.** `Create` keys the cache on `font.name` (what Unity actually loaded), so two different `fontNames` lists that resolve to the same OS font correctly share a cache.
- **Glyphs render as white alpha masks.** Each glyph pixel becomes solid white with alpha = max(r,g,b,a) of the source, preserving anti-aliased edge coverage. Tinting is applied downstream via material color. Sub-pixel colored emoji are not supported by this model.
- **`TextTextureFontService.PopulateDropdown`** depends on `DropdownRow<string>` from the wider TimberUi widget set.
