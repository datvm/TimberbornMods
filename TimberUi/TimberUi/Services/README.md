# Services

## Purpose
The `Services` namespace is the functional core of TimberUi: a collection of DI-bound singletons and helpers that mod authors consume to do common UI work (show dialogs, prompt for text/strings, look up game icons, style sliders, shake the camera, reorder entity-panel fragments, notify about mod updates, load mod audio, render text to textures, open native file dialogs). This README documents only the three files that live directly in the `Services` root; the sub-namespaces each have their own README and are indexed at the bottom.

The root files are small leaf utilities: a high-level async `DialogService` facade, a `MinMaxSliderInitializer` that restyles Unity's two-handle slider, and a `NamedIconProvider` that caches and resolves sprites by friendly name.

## Key types
- **`DialogService`** (`DialogService.cs`) — `[BindSingleton(Contexts = All)]` partial class. Async facade over `DialogBoxShower`. Provides `Alert`/`AlertAsync` (single confirm), `ConfirmAsync` (ok/cancel returning `bool`, with optional raw or localized button text), and `PromptAsync` (text input returning `string?` — null on cancel). Internally builds a `DialogBoxShower.Builder` and bridges callbacks to `TaskCompletionSource`. `PromptAsync` delegates to `PromptDialogElement` (see Dialogs subfolder).
- **`MinMaxSliderInitializer`** (`MinMaxSliderInitializer.cs`) — `[MultiBind(typeof(IVisualElementInitializer), Contexts = All)]`, also `ILoadableSingleton`. On load it loads three slider textures from assets; for any `MinMaxSlider` carrying the `UiCssClasses.TimberUiMinMaxSlider` class it restyles the two thumbs (size 25, top margin -10, hover swap) and the tracker bar. Hook into TimberUi's `IVisualElementInitializer` pipeline so styling applies automatically when such a slider is initialized.
- **`NamedIconProvider`** (`NamedIconProvider.cs`) — `[BindSingleton(Contexts = All)]`. Two-level sprite cache (`spritesByName` + `spritesByPath`) over `IAssetLoader`. Exposes named convenience properties (Food, Logs, Materials, Science, Water from `sprites/topbar/...`; QuestionMark, Clock, Arrow, ScienceAlt from `ui/images/game/...`) plus generic `GetOrLoad(name, path)`, `GetOrLoadTopbar`, `GetOrLoadGameIcon`, and an indexer `this[name]` that reads `spritesByName` (lookup only — the sprite must have been loaded previously via a `GetOrLoad*` call).

## How it fits together
`DialogService` is the consumer-facing entry for dialogs and sits on top of Timberborn's `DialogBoxShower` plus TimberUi's own `PromptDialogElement`. `MinMaxSliderInitializer` participates in TimberUi's visual-element initialization fan-out (it is multi-bound alongside other `IVisualElementInitializer` implementations). `NamedIconProvider` is a shared sprite registry other UI code can pull from rather than re-loading sprites ad hoc.

## Dependencies & patterns
- Primary-constructor DI throughout; all three are context-bound singletons (`BindSingleton` / `MultiBind`).
- `DialogService` uses the `TaskCompletionSource` callback-to-async bridge pattern. `ILoc t` is used for button-text localization.
- `NamedIconProvider` and `MinMaxSliderInitializer` both depend on `IAssetLoader`. The provider uses lazy get-or-load caching.

## Usage notes
- When passing both raw and localized button text to `ConfirmAsync`, the localized value takes precedence — the raw value is not used for that button.
- The named convenience properties on `NamedIconProvider` (e.g. `Food`, `Logs`, `Water`) are the recommended way to retrieve common sprites; they guarantee the sprite is loaded before being returned. The indexer `this[name]` is a direct cache lookup and requires that the sprite was previously loaded via a `GetOrLoad*` call.
- `MinMaxSliderInitializer` only applies to `MinMaxSlider` elements that also carry the `UiCssClasses.TimberUiMinMaxSlider` CSS class — both conditions must be met.

## Sibling subfolder index
- **CameraShake/** — `CameraShakeService` (screen-shake driver, `IUpdatableSingleton`) + `CameraShakeSettingService` (persisted enable/disable toggle in settings).
- **Dialogs/** — `PromptDialogElement`, the text-input dialog used by `DialogService.PromptAsync`.
- **EntityPanelOrder/** — `EntityFragmentOrderService` + `IEntityFragmentOrder`; reorders entity-panel fragments by an integer Order.
- **ModAudioClip/** — `AudioClipManagementService` (add/replace/remove clips in the game's `AudioClipService`) + `ModAudioClipConverter` (`.wav` → `AudioClip` file converter).
- **ModUpdate/** — `IModUpdateNotifier` + `ModUpdateService`; shows main-menu dialogs when an enabled mod advertises an update.
- **SystemFileDialog/** — `ISystemFileDialogService` with platform implementations (Windows/macOS/Linux) for native open/save file dialogs.
- **TextTexture/** — text-to-texture rendering subsystem (glyph layout/cache, font service, renderer, models) for drawing text onto Unity textures.
