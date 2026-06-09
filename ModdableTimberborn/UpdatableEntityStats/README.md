# UpdatableEntityStats

## Purpose

Provides a framework for attaching observable, pausable stat trackers to Timberborn game entities. Consumers (e.g. UI widgets) ask a stat object to produce a tracker for a specific entity; the tracker pushes `OnValueChanged` events whenever the underlying game state changes, and can be paused/resumed to avoid subscriptions while off-screen. This decouples UI update logic from the specifics of each game component.

## Key types

- **`IUpdatableEntityStat` / `IUpdatableEntityStat<T>`** — Factory interface. Each stat has a string `Id`, a localization key `DisplayLoc`, and the ability to answer `CanTrack(comp)` and produce a typed `IEntityStatTracker` for a given entity component.
- **`IStatTracker` / `IStatTracker<T>`** — Lifecycle interface for a live tracker: `Start()`, `Pause()`, `Running`, `Value`, `ValueFormatted`, `OnValueChanged`, `OnTypedValueChanged`.
- **`IEntityStatTracker` / `IEntityStatTracker<T>`** — Extends `IStatTracker` with the owning `UpdatableEntityStatComponent` and an `OnEntityLost` event fired when the entity is deleted.
- **`IPercentStat` / `IPercentStatTracker` / `IEntityPercentStatTracker`** — Marker interfaces for `float` stats that display as a percentage (`"P0"` format). `IPercentStatTracker` provides a default `ValueFormatted` implementation.
- **`IImageStat` / `IImageStatTracker`** — Marker interfaces for `Sprite?` stats (entity avatars, recipe icons, stockpile icons).
- **`UpdatableEntityStatComponent`** — `BaseComponent` / `IDeletableEntity` attached to every entity via the template module. Owns the set of live trackers; on `DeleteEntity()` it calls `NotifyEntityLost()` on each then disposes them all.
- **`UpdatableEntityStatService`** — Singleton registry of all `IUpdatableEntityStat` instances (injected as a multi-bind collection). Exposes typed filtered arrays (`AllImageStats`, `AllPercentStats`) and a by-`Id` lookup. Also surfaces the `PopulationStat` directly.
- **`PopulationStatService`** — Singleton that wraps `SamplingPopulationService` and the game `EventBus`, converting `PopulationChangedEvent` into a simple `OnPopulationChanged` action. Used by population trackers to avoid each tracker subscribing to the event bus independently.
- **`UpdatableEntityStatsConfig`** — `IModdableTimberbornRegistryConfig`; the DI configurator. Registers services, attaches `UpdatableEntityStatComponent` to all `TemplateSpec`s, and auto-discovers every concrete `IUpdatableEntityStat` in the assembly via reflection for multi-bind registration.

## How it fits together

`UpdatableEntityStatsConfig` is activated by calling `registry.UseUpdatableEntityStats()` during mod bootstrap. It scans the executing assembly for all concrete `IUpdatableEntityStat` implementations and multi-binds them, so `UpdatableEntityStatService` receives the complete collection at injection time.

At runtime, a consumer (typically a UI panel) holds a reference to `UpdatableEntityStatService`. When the player focuses an entity, the consumer calls `stat.TryGetTracker(entityStatComp, out tracker)` for each stat it cares about. The returned tracker is independent per entity—its constructor registers it with the `UpdatableEntityStatComponent`—so `DeleteEntity()` can sweep them all when the entity is removed.

The consumer calls `tracker.Start()` to begin receiving events and `tracker.Pause()` or `tracker.Dispose()` when the panel closes. `ForceUpdating()` triggers an unconditional re-raise of change events regardless of whether the value actually changed (useful for first-display initialization).

`StatTrackerBase<T>` (in `Implementations/`) provides the standard template: `OnStart()`/`OnPause()` subscribe/unsubscribe from underlying game events; `CalculateValue()` computes the new value; `UpdateValue()` compares to the previous value and fires events if changed.

## Dependencies & patterns

- **Bindito (DI):** `MultiBind<IUpdatableEntityStat>` via reflection in `UpdatableEntityStatsConfig`; services registered as singletons. Entry point: `ModdableTimberbornRegistry.UseUpdatableEntityStats()`.
- **Timberborn `BaseComponent` / template system:** `UpdatableEntityStatComponent` is added as a decorator on `TemplateSpec` so every entity has it automatically.
- **Timberborn `IDeletableEntity`:** Drives the entity-lost notification path.
- **Timberborn `EventBus`:** Used only in `PopulationStatService` to receive `PopulationChangedEvent`; other trackers subscribe directly to component-level C# events.
- **Timberborn game components used by built-in stats:** `DistrictCenter`, `Manufactory`, `Inventory`, `Stockpile` / `SingleGoodAllower`, `NamedEntity`, `Contaminable`, `EntityBadgeService`, `IGoodService`, `SamplingPopulationService`.
- **Localization key convention:** `DisplayLoc` defaults to `"LV.MT.UStat.<Id>"`.

## Notes / gotchas

- **`PopulationStat.TryGetTracker(options, comp, out tracker)`** mutates the tracker's `Options` after construction (via `internal set`). If `TryGetTracker` is called again for the same entity, it overwrites the options on the existing tracker rather than creating a new one — callers must be aware of tracker identity.
- **`GlobalPopulationStatTracker`** is not an `IEntityStatTracker`; it does not attach to any `UpdatableEntityStatComponent` and must be disposed manually by the caller.
- **Auto-discovery via reflection** in `UpdatableEntityStatsConfig` means adding a new stat class is zero-config, but it also means any concrete `IUpdatableEntityStat` in the assembly (including test helpers) will be registered if the config runs.
- **`InventoryFinder.LookForInventories`** is called by inventory stats but is defined elsewhere in ModdableTimberborn; its semantics (which inventory it picks when multiple exist) are not visible here.
