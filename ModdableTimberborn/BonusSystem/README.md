# BonusSystem

## Purpose

Provides a named, keyed layer on top of Timberborn's native `BonusManager`/`BonusSpec` API, letting mod code attach a group of bonus multipliers to an entity under a single string ID and replace or remove them atomically. This avoids the accumulation/leak problem that arises when callers add raw `BonusSpec` entries directly without tracking what they've already applied.

## Key types

- **`BonusTracker`** — Core logic class (not a component). Owns a `Dictionary<string, BonusTrackerItem>` and proxies `BonusManager.AddBonuses` / `RemoveBonuses`. Fires `OnBonusChanged` whenever an entry is added, updated, or removed. Constructed by the component types in `Awake`.
- **`BonusTrackerItem`** — Immutable `readonly record struct` pairing a string `Id` with a list of `BonusSpec` values. Several convenience constructors accept a raw float, a `BonusType` enum, or a single `BonusSpec`.
- **`IBonusTrackerComponent`** — Single-property interface (`BonusTracker BonusTracker { get; }`). The extension methods target this interface, so any component that exposes a tracker gets the fluent API automatically.
- **`BonusTrackerComponent`** — Transient decorator component (no persistence). Implements `IBonusTrackerComponent` + `IAwakableComponent`; creates a `BonusTracker` on `Awake`.
- **`PersistentBonusTrackerComponent`** — Persistent variant; additionally implements `IPersistentEntity` + `IStartableComponent`. Saves/loads the full `CurrentBonuses` dictionary via Timberborn's entity save system, replaying loaded items in `Start` (after `BonusManager` is ready).
- **`BonusTrackerItemSerializer`** — Singleton `IValueSerializer<BonusTrackerItem>`; used by `PersistentBonusTrackerComponent` to read/write items as parallel `Id`/`BonusIds`/`Values` lists.
- **`BonusSystemExtensions`** — Static helper forwarding `AddOrUpdate`, `Remove`, and `CurrentBonuses` directly from any `IBonusTrackerComponent`, removing the need to drill through `.BonusTracker`.
- **`BonusSystemHelpers`** — String constants for the six standard bonus IDs (`CarryingCapacity`, `CuttingSuccessChance`, `GrowthSpeed`, `LifeExpectancy`, `MovementSpeed`, `WorkingSpeed`).
- **`BonusSystemConfig`** — `IModdableTimberbornRegistryConfig` configurator; registers whichever decorator component variant(s) are enabled as `BindTemplateModule` decorators on `BonusManager`.

## How it fits together

Mod startup calls `ModdableTimberbornRegistry.Instance.UseBonusTracker()` or `UsePersistentBonusTracker()`. Either call sets a flag on the registry's partial class (defined here) and, on the first call, registers `BonusSystemConfig` as a configurator. At game-scene binding time, `BonusSystemConfig.Configure` decorates every `BonusManager` entity with `BonusTrackerComponent` or `PersistentBonusTrackerComponent` via Bindito's `BindTemplateModule`.

Consumers obtain the component via Timberborn's normal entity-component lookup, cast it (or receive it already typed) as `IBonusTrackerComponent`, and call `AddOrUpdate` / `Remove` (directly or through the extension methods). `BonusTracker` internally calls `BonusManager.RemoveBonuses` for the old item then `AddBonuses` for the new one, keeping the underlying multipliers in sync.

For the persistent variant, loaded bonus data is stashed as a `List<BonusTrackerItem>` during `Load` and replayed in `Start` so that `BonusManager` is guaranteed to be initialized before bonuses are applied. `Save` skips writing entirely when `CurrentBonuses` is empty.

## Dependencies & patterns

- **Timberborn internals:** `BonusManager`, `BonusSpec`, `BonusType` (game's native bonus system); `IPersistentEntity` / `IEntityLoader` / `IEntitySaver` (save/load); `BaseComponent`, `IAwakableComponent`, `IStartableComponent`.
- **Bindito (DI):** `BindTemplateModule` + `AddDecorator` to inject components onto existing entity templates without patching their prefabs.
- **Registry pattern:** `ModdableTimberbornRegistry` (partial class, extended here) acts as a global opt-in switch; calling `UseBonusTracker` / `UsePersistentBonusTracker` is the only required configuration step for consumers.
- **No Harmony patches** in this folder — integration is entirely through Bindito decorators.
- **Serialization:** Custom `IValueSerializer<BonusTrackerItem>` using Timberborn's `PropertyKey`/`ListKey` API; parallel lists for bonus IDs and multiplier deltas.

## Notes / gotchas

- Only one variant (transient or persistent) should be registered per game context; both flags can technically be set but the guards (`if (!BonusTrackerUsed && !PersistentBonusTrackerUsed)`) only prevent double-registering the configurator, not double-decorating the entity.
- `AddOrUpdateOrRemove` on `BonusTracker` silently removes an item if all its `MultiplierDelta` values are zero — convenient but can be surprising if callers construct a zero-delta item expecting a no-op add.
- The `pending` list in `PersistentBonusTrackerComponent` is `null` unless `Load` is called, which is the expected game-save path. Cold-start (new game) never calls `Load`, so `Start` becomes a safe no-op.
- `BonusTrackerItemSerializer` is a singleton accessed via `Instance`; it is not DI-registered and must be passed explicitly wherever serialization is needed.
