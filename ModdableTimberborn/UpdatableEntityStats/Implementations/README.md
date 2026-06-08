# UpdatableEntityStats/Implementations

## Purpose

Contains the concrete `IUpdatableEntityStat` factory classes and their paired `StatTrackerBase<T>` tracker subclasses for all built-in stat types. Each pair follows the same pattern: a singleton factory that checks feasibility and constructs a tracker, plus a tracker that subscribes to game component events and calls `UpdateValue()` on each change.

## Key types

### Base classes (re-exported from this namespace)

- **`StatTrackerBase<T>`** — Abstract base for all entity-bound trackers. Handles `Start`/`Pause` lifecycle, value diffing, event firing, and self-registration with `UpdatableEntityStatComponent`. Subclasses implement `OnStart()`, `OnPause()`, and `CalculateValue()`.
- **`StatPercentTrackerBase`** — `StatTrackerBase<float>` that also implements `IPercentStatTracker`; formats `ValueFormatted` as `"P0"`.
- **`UpdatableEntityStatBase<T>`** — Abstract base for stat factories. Provides the default `DisplayLoc` convention (`"LV.MT.UStat.<Id>"`).
- **`ComponentUpdatableEntityStatBase<T, TComp>`** — Stat factory that gates on the presence of a Timberborn `BaseComponent` of type `TComp`. Subclasses only implement `GetComponentTracker(statComp, comp)`.
- **`ComponentUpdatableEntityPercentStatBase<TComp>`** — Extends the above for percent stats (`float`), threading through both `IPercentStat` and `IEntityPercentStatTracker`.

### Concrete stat pairs

| Stat class | Tracker class | Id | Value type | Driven by |
|---|---|---|---|---|
| `NameStat` | `NameTracker` | `"Name"` | `string` | `NamedEntity.EntityNameChanged` |
| `AvatarStat` | `AvatarStatTracker` | `"Avatar"` | `Sprite?` | `Contaminable.ContaminationChanged` |
| `ProductionPercentStat` | `ProductionPercentStatTracker` | `"ProductionPercent"` | `float` | `Manufactory.ProductionProgressed` |
| `RecipeIconStat` | `RecipeIconStatTracker` | `"RecipeIcon"` | `Sprite?` | `Manufactory.RecipeChanged` |
| `InventoryStockStat` | `InventoryStatTracker<int>` | `"StorageStock"` | `int` | `Inventory.InventoryChanged` |
| `InventoryStockMax` | `InventoryStatTracker<int>` | `"StorageStockMax"` | `int` | `Inventory.InventoryChanged` |
| `InventoryPercent` | `InventoryPercentStatTracker` | `"StoragePercent"` | `float` | `Inventory.InventoryChanged` |
| `StockpileIconStat` | `StockpileIconStatTracker` | `"StockpileIcon"` | `Sprite?` | `SingleGoodAllower.DisallowedGoodsChanged` |
| `PopulationStat` | `DistrictPopulationStatTracker` / `GlobalPopulationStatTracker` | `"Population"` | `int` | `PopulationStatService.OnPopulationChanged` |

## How it fits together

All concrete stat classes in this folder are picked up automatically by the reflection loop in `UpdatableEntityStatsConfig` and multi-bound as `IUpdatableEntityStat` singletons. `UpdatableEntityStatService` aggregates them.

`InventoryStat<T>` is a shared intermediate base for the three inventory stats; it uses `InventoryFinder.LookForInventories` to locate the relevant `Inventory` component and then delegates tracker creation to subclasses. `InventoryStatTracker<T>` is the generic tracker that accepts a `Func<Inventory, T>` value extractor.

`AvatarStat` is slightly special: `CanTrack` delegates to `EntityBadgeService` rather than checking for a specific component, but the tracker is always created (returns `true`) for any non-null entity — avatar display is considered universally available.

`PopulationStat` lives at the root namespace level (not in `Implementations/`) because it is directly exposed through `UpdatableEntityStatService.PopulationStat`; it also provides a `GetGlobalTracker(options)` factory method for non-entity-bound global population tracking.

## Dependencies & patterns

- All concrete stats and trackers depend solely on Timberborn game components; no Harmony patches are used here.
- Tracker construction is always side-effectful: the `StatTrackerBase<T>` constructor calls `comp.AddTracker(this)`, so a tracker is live the moment it is created. Callers must either call `Start()` promptly or `Dispose()` the tracker if they decide not to use it.

## Notes / gotchas

- **`InventoryStatTracker<T>`** is not abstract and is used directly by `InventoryStockStat` and `InventoryStockMax` via a lambda; `InventoryPercent` subclasses it as `InventoryPercentStatTracker` to override `ValueFormatted`.
