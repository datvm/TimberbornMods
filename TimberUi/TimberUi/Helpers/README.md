# Helpers

## Purpose
General-purpose utility code for the TimberUi library: dependency-injection / configurator convenience wrappers, small LINQ-style collection extensions, serializable 4-component numeric structs (with broad implicit conversions to Unity vector/color/rect types and Timberborn `IValueSerializer` support), and audio (WAV PCM encode/decode) utilities. This folder is a grab-bag — the pieces are unrelated to each other and are consumed across the rest of the library.

Note: `TimberUiUtils.cs` physically lives in this folder but declares `namespace TimberUi` (not `TimberUi.Helpers`); everything else here is in `TimberUi.Helpers`.

## Key types
- **`TemplateModuleHelper`** (`ConfiguratorHelpers.cs`) — fluent builder around Timberborn's `TemplateModule.Builder`. `AddDecorator<TSubject,TDecorator>()` registers a decorator on the builder and (by default) also binds the decorator as transient in the `Configurator`. A non-generic `AddDecorator(Type, Type)` overload reflects into `TemplateModule.Builder.AddDecorator` via a cached `MethodInfo`. `Bind()` multi-binds the built `TemplateModule` as a singleton provider.
- **`MassRebindingHelper`** (`ConfiguratorHelpers.cs`) — fluent helper to bulk-replace/remove DI bindings. `Replace<TRemove,TReplace>()` (with `TReplace : TRemove`) and `Remove<T>()` accumulate intent; `Bind()` first calls `configurator.MassRemoveBindings(...)` on all keys, then re-adds the replacements via `BindSingleton`. Relies on extension methods defined elsewhere in TimberUi (`MassRemoveBindings`, `BindSingleton(src,dst,bool)`).
- **`ListExtensions`** (`ListExtensions.cs`) — uses the new C# `extension` member syntax. `IReadOnlyList<T>.Randomize()` returns a random element (or `default` when empty) using `UnityEngine.Random`. `IEnumerable<T>.FindIndex(Predicate<T>)` returns the index of the first match or -1.
- **`SerializableFloats`** (`SerializableFloats.cs`) — `readonly record struct (float X,Y,Z,W)` with a semicolon-delimited `Serialize()`/`Deserialize()`, multiple `Deconstruct` arities, and a large set of implicit conversions to/from `Vector2/3/4`, `Color`, `Quaternion`, `Rect` (plus one-way conversions from the `Int`/`Vector*Int`/`RectInt`/`SerializableInts` types). Exposes a static `Serializer` (`IValueSerializer<SerializableFloats>`) for Timberborn save data.
- **`SerializableInts`** (`SerializableInts.cs`) — integer analogue of the above; fewer conversions (`Vector2Int/Vector3Int/RectInt`), same serialize format and nested `SerializableIntsSerializer`.
- **`TimberUiUtils`** (`TimberUiUtils.cs`) — static grab-bag: a `FrozenSet<string>` of loaded assembly names (computed in the static ctor), `HasMoreModLogs` flag, the Timberborn `SteamId` constant, standard UI `Color`s (Success/Neutral/Warning/Danger/Transparent), `LogVerbose`/`LogDev` logging, `KillProcess`/`Restart` (process control), `LoadAudioClipFrom` (delegates to `WavUtility`), and `GetSortedEnumValues<T>()`.
- **`WavUtility`** (`WavUtility.cs`) — third-party (deadlyfingers/UnityWav) static class. `ToAudioClip(...)` decodes 8/16/24/32-bit PCM WAV bytes into a Unity `AudioClip`; `FromAudioClip(...)` encodes an `AudioClip` back to 16-bit PCM WAV bytes (optionally writing a timestamped file under `Application.persistentDataPath/recordings`).

## How it fits together
`TimberUiUtils.LoadAudioClipFrom` is the public entry point that forwards to `WavUtility.ToAudioClip`; mods that need to load custom sounds go through there. The `SerializableFloats`/`SerializableInts` structs are the library's interchange type between Unity geometry/color types and Timberborn's string-based save serialization — their static `Serializer` instances plug into Timberborn's `IValueSerializer<T>` system. The two configurator helpers wrap Timberborn's DI (`Configurator`, `TemplateModule`) for mods doing decoration or wholesale binding replacement. `ListExtensions` and the color constants are leaf utilities consumed ad hoc.

## Dependencies & patterns
- Heavy reliance on Timberborn types not in this folder: `Configurator`, `TemplateModule(.Builder)`, `IValueSerializer<T>`, `IValueLoader`/`IValueSaver`, `Obsoletable<T>`, and the assumed-existing `Configurator` extension methods (`MultiBind`, `BindTransient`, `MassRemoveBindings`, `BindSingleton`).
- UnityEngine types (`Vector*`, `Color`, `Quaternion`, `Rect`, `AudioClip`, `Random`, `Debug`, `Application`) resolved via global usings (no explicit `using` lines in most files).
- Patterns: fluent builders returning `this`; record structs with implicit conversions; cached reflection `MethodInfo`; static-ctor-computed readonly caches.

## Notes
- **`TimberUiUtils.LogDev` is `[Obsolete]`** — it is intentionally marked obsolete as a debugging aid ("Remember to remove this log after debugging"), so any call site raises a compile-time warning by design.
- **`Randomize()` returns `default` for empty lists** (i.e. `null` for reference types) rather than throwing. Callers should null-check the result.
- **`WavUtility`** is third-party code (deadlyfingers/UnityWav). `FromAudioClip` encodes to 16-bit PCM regardless of source bit depth. WAV header parsing assumes a well-formed canonical header layout.
