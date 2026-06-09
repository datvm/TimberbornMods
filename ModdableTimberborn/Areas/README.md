# Areas

## Purpose

Provides spatial-query APIs for tracking which game entities (characters, block objects) occupy or intersect one or more user-defined rectangular volumes. It exists because Timberborn has no built-in "which characters are inside this zone right now" service; this subsystem fills that gap in a performance-aware way using a 2-D segment grid to prune unnecessary checks.

## Key types

- **`AreaSegmentService`** — Singleton. Partitions the map into 26-tile-wide segments on load. Converts coordinates and `BoundsInt` ranges into segment indices, used by every tracker to skip irrelevant handles.
- **`AreaTrackerRegistration`** / **`CharacterAreaTrackerRegistration`** / **`BlockObjectAreaTrackerRegistration`** — Plain data bags callers fill in to describe the area(s) they want to watch and the entity filter criteria (character type flags, template names, finished-only, etc.).
- **`AreaTrackerHandle<T, TRegistration>`** — Abstract base that owns the live `Entities` set, the enter/exit events (`OnEntityEntered`, `OnEntityExited`), and the deferred-mutation collection (`DeferredHashSet`). Subclasses supply `IsEntityInAreas`.
- **`CharacterAreaTrackerHandle`** — Concrete handle for characters. Filters by `CharacterType` flags and checks `BoundsInt.Contains(cell)`.
- **`CharacterAreaTrackerService`** — Singleton. Manages all registered `CharacterAreaTrackerHandle` instances. Hooks into `CharacterTracker` for entity lifecycle, and into each `CharacterPositionTracker.OnCellChanged` (only for types that have live handles, lazy subscribe/unsubscribe). Routes cell-change events through segment buckets to notify only relevant handles.
- **`CharacterPositionTracker`** — `TickableComponent` decorator on `Character`. Each tick, detects world-position change, updates `Cell` (floor-to-int) and `Segment`, fires `OnPositionChanged` / `OnCellChanged` accordingly.
- **`BlockObjectBound`** — `BaseComponent` decorator on `BlockObject`. Lazily caches the object's `BoundsInt` and segment list; re-computes only when `Coordinates` changes.
- **`AreaCondition`** — Enum: `Intersects` or `Contains`. Evaluated by extension methods in `AreaExtensions`.
- **`AreaExtensions`** (in `ModdableTimberborn.Helpers`) — C# 13 `extension` blocks adding `GetBounds()` to `BlockObject`, `Evaluate(BoundsInt, BoundsInt)` to `AreaCondition`, and `Intersects`/`Contains` to `BoundsInt` (Unity's `BoundsInt` lacks these natively).
- **`SerializableBounds`** / **`SerializableBoundsInts`** — Readonly record structs with implicit casts to/from Unity `Bounds`/`BoundsInt`, wrapping the project's `SerializableFloats`/`SerializableInts` helpers for JSON-safe serialization.

## How it fits together

Opt-in via `ModdableTimberbornRegistry.UseAreaApis()` (idempotent). This registers `AreaSegmentService` and `CharacterAreaTrackerService` as singletons, and attaches `CharacterPositionTracker` and `BlockObjectBound` as decorators on every `Character` and `BlockObject` entity respectively (through Bindito's template-module decorator pattern).

A mod that wants to know which characters are inside a zone calls `CharacterAreaTrackerService.RegisterArea(registration)` at runtime (typically on load or when a building activates), receiving a `CharacterAreaTrackerHandle`. It then subscribes to `handle.OnEntityEntered` / `OnEntityExited`, or polls `handle.Entities`. When done (building destroyed, etc.) it calls `UnregisterArea(handle)`.

On every game tick, `CharacterPositionTracker.Tick()` fires `OnCellChanged` when a character crosses a tile boundary. `CharacterAreaTrackerService` listens only on character types that have at least one registered handle, and uses the old and new segment indices to look up only the small subset of handles that overlap those segments — avoiding O(handles × characters) checks per tick.

`BlockObjectBound` is wired in but a `BlockObjectAreaTrackerHandle`/service is not yet present in this folder — the registration type (`BlockObjectAreaTrackerRegistration`) exists, suggesting it is planned.

## Dependencies & patterns

- **Bindito DI:** `AreaConfig` implements `IModdableTimberbornRegistryConfig` and is registered by the registry. `AreaSegmentService` and `CharacterAreaTrackerService` are singletons bound with `.BindSingleton<>()`. `CharacterPositionTracker` and `BlockObjectBound` are bound as entity decorators via `.BindTemplateModule(h => h.AddDecorator<...>())`.
- **Timberborn entity systems:** `CharacterTracker` (for entity registration events), `CharacterTrackerComponent` (character identity), `BlockObject` / `MapSize`.
- **`ILoadableSingleton`:** Both services implement this to perform initialization after the map is fully loaded.
- **`TickableComponent`:** `CharacterPositionTracker` is a per-tick component; it uses `transform.position` rather than a grid-cell component, so it tracks sub-tile movement too.
- **`DeferredHashSet<T>`:** Used internally to allow mutation during iteration (avoid modification-during-enumeration bugs in event callbacks).
- **Unity math types:** `BoundsInt`, `Bounds`, `Vector3`, `Vector3Int`, `RectInt` — extended by `AreaExtensions` to fill gaps in Unity's API.
- **C# 13 `extension` blocks** in `AreaExtensions.cs` — requires a modern C# language version in the project file.

## Notes / gotchas

- **Segment size is a hardcoded constant** (`AreaSegmentService.SegmentSize = 26`). There is no tuning API.
- **Z-axis not segmented.** `AreaSegmentService` intentionally only segments X/Y; the comment says Z is "typically small enough." Area containment checks still use all three axes via `BoundsInt`.
- **`CharacterType.ArrayLength`** is used as an array size, implying `CharacterType` is a flags enum whose values are used directly as array indices — callers must be careful not to pass combined flags as array indices.
- **`AreaExtensions` lives in the `ModdableTimberborn.Helpers` namespace**, not `ModdableTimberborn.Areas` — a minor namespace inconsistency compared to the rest of the folder.
- **`UseAreaApis()` calls `UseEntityTracker()` and `TryTrack<>`**, so enabling Areas implicitly enables the entity tracker subsystem.
