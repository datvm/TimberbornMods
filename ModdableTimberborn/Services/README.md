# Services

## Purpose

A small collection of cross-cutting singleton services that sit outside any single feature area. They provide globally accessible infrastructure — DI container introspection, in-game day-relative timing, and coordinated terrain/block-object destruction — that other subsystems consume directly.

## Key Types

- **`ContainerRetriever`** — Static-access shim over the Bindito `IContainer`. Stores the container in a `WeakReference` so it goes away when the scene unloads. Exposes `GetInstance<T>()` / `GetInstances<T>()` as static helpers for places where normal constructor injection is not possible.
- **`DayTimerService`** — `[BindSingleton]` that lets callers schedule an `Action` to fire at a specific in-game hour offset after each day starts (repeating) or once on the next day. Returns a `DayTimerReference` token used for unregistration.
- **`DayTimerReference`** — Lightweight `record` carrying the delay, action, and repeat flag; used as the unregistration handle.
- **`DestructionService`** — `[BindSingleton(Contexts = BindAttributeContext.NonMenu)]` that handles the full lifecycle of block-object/terrain destruction: querying the affected set (including physics-dependent objects above terrain), highlighting in the UI, and executing deletion.
- **`DestroyingEntities`** — Immutable `readonly record struct` bundling `ImmutableArray<BlockObject>` and `ImmutableArray<Vector3Int>` returned by a destruction query; passed between the query, highlight, and destroy steps.

## How It Fits Together

`ContainerRetriever` is registered alongside the main DI configuration (presumably in a configurator elsewhere); it self-registers its static `instance` field on construction, making it available to Harmony patches or static factory methods that cannot receive injected dependencies normally. The `Refresh()` method cleans up the stale reference after scene teardown.

`DayTimerService` listens on the `EventBus` for `CycleDayStartedEvent`. On each new day it iterates every registered delay bucket and fires an `ITimeTriggerFactory`-created timer for each, so actions execute at the correct fractional-day offset rather than immediately. Callers call `Register` / `RegisterOnce`, hold the returned `DayTimerReference`, and call `Unregister` when the owning component is torn down.

`DestructionService` provides a two-phase API: **query** (returns a `DestroyingEntities` snapshot) → optional **highlight** (feeds `RollingHighlighter` and `TerrainHighlightingService`) → **destroy** (delegates to `EntityService.Delete` and `TerrainDestroyer.DestroyTerrain`). Callers are expected to call `UnhighlightDestructionEntities` when the operation is cancelled. The service reuses two internal `HashSet` scratch collections, clearing them after each `ReturnQuery()` call — it is not thread-safe.

## Dependencies & Patterns

| Type | Source |
|---|---|
| `IContainer`, `[BindSingleton]`, `[MultiBind]` | Bindito (Timberborn DI) |
| `ILoadableSingleton` | Timberborn lifecycle interface |
| `EventBus`, `[OnEvent]`, `CycleDayStartedEvent` | Timberborn event system |
| `ITimeTriggerFactory` | Timberborn timing infrastructure |
| `ITerrainPhysicsService`, `TerrainDestroyer`, `IBlockService` | Timberborn terrain/block layer |
| `EntityService` | Timberborn entity management |
| `TerrainHighlightingService`, `RollingHighlighter` | Timberborn UI highlight layer |
| `ISpecService`, `BrushColorSpec` | Timberborn spec/config system |

`DestructionService` uses `BindAttributeContext.NonMenu` to exclude it from the main-menu scene container.

## Notes / Gotchas

- `ContainerRetriever.Instance` is a **mutable static singleton** — the last constructed instance wins. This works because Timberborn constructs a fresh DI container per scene, but be aware in tests or multi-container scenarios.
- `DayTimerService` snapshots `timers.Keys.ToArray()` before iterating, so registrations made from inside a timer callback during the same `OnNewDay` event are safely deferred to the following day.
