# ModAudioClip

## Purpose
Loads mod-supplied audio (`.wav`) into Timberborn's audio system and lets mods register/replace/remove named clips at runtime. Two pieces: `ModAudioClipConverter` is a file converter that turns `.wav` files discovered in mod folders into Unity `AudioClip`s during mod asset loading; `AudioClipManagementService` is the runtime facade for adding, replacing, removing, and enumerating clips in the game's `AudioClipService`.

## Key types
- **`ModAudioClipConverter`** (`ModAudioClipConverter.cs`) — `IModFileConverter<AudioClip>`. `ValidExtensions` = `{ ".wav" }` (FrozenSet). `CanConvert` matches by lowercased extension. `TryConvert` decodes via `WavUtility.ToAudioClip(fullPath)` (wrapping any failure in `InvalidDataException`), records a `ModAudioClip(FullName, AudioClip)`, and returns true. `Reset` destroys all tracked `AudioClip`s (`UnityEngine.Object.Destroy`) and clears the list. Exposes `IReadOnlyList<ModAudioClip> AudioClips`.
- **`ModAudioClip`** (same file) — `readonly record struct (string FullName, AudioClip AudioClip)`.
- **`AudioClipManagementService`** (`AudioClipManagementService.cs`) — runtime facade over `AudioClipService`. `AllAudioClips` exposes the game's `_audioClips` dictionary read-only. `AddOrReplace(filePath, name?)` loads via `TimberUiUtils.LoadAudioClipFrom` and registers the clip; the effective registry key is the `name` argument when provided, or the file name (without extension) when `name` is null — this value is passed through to `AudioClip.Create` and then read back as the clip's `name`. `AddOrReplace(name, clip)` writes directly into `_audioClips`. `Remove(name)` removes from it.

## How it fits together
The converter participates in TimberUi/Timberborn's mod file-conversion pipeline (anything implementing `IModFileConverter<T>` is invoked over mod files); it produces `AudioClip`s and keeps a list for lifecycle cleanup via `Reset`. The management service is the imperative counterpart for code that wants to inject or swap clips by name into the live `AudioClipService` after load. They are independent entry points to the same underlying audio store.

## Dependencies & patterns
- Converter ← `WavUtility` (decode), `OrderedFile`/`SerializedObject` metadata from the converter framework, `FrozenSet` for extension matching.
- Management service ← `AudioClipService` (accesses its `_audioClips` field).
- `ModAudioClip` record-struct as the converter's tracking unit.

## Usage notes
- Only `.wav` files are supported. Decode failures throw `InvalidDataException` immediately, so problems surface at load time.
- When calling `AddOrReplace(filePath, name?)`, the `name` parameter becomes the clip's registry key. Pass an explicit `name` to control the key; omit it (or pass `null`) to use the file name without extension.
- The converter's `Reset` method destroys the `AudioClip` objects it created. Clips that were also registered with the live `AudioClipService` should be removed from it before or after `Reset` is called, as the converter and the management service do not coordinate cleanup automatically.
