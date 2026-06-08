# Bonus System

The Bonus System lets you attach, replace, and remove named groups of bonus multipliers on any character entity — and optionally persist those groups across saves. It sits on top of Timberborn's native `BonusManager`/`BonusSpec` API and adds a keyed dictionary layer that prevents the common leak problem of adding raw bonuses without tracking what was already applied.

**Relevant source:** [`../../ModdableTimberborn/BonusSystem/`](../../ModdableTimberborn/BonusSystem/)

## Enabling the subsystem

Call one of the opt-in methods on `ModdableTimberbornRegistry.Instance` from your `IModStarter`:

```cs
ModdableTimberbornRegistry.Instance
    .UseBonusTracker()           // transient — bonuses are not saved
    .UsePersistentBonusTracker() // persistent — bonuses survive save/load
```

`UseBonusTracker(bool persistent)` is also available if the choice is determined at runtime. Both methods are chainable and safe to call multiple times.

Internally, either call registers `BonusSystemConfig`, which decorates every `BonusManager` entity with the appropriate component at game-scene binding time:

- `UseBonusTracker()` → adds `BonusTrackerComponent` to each entity that has a `BonusManager`
- `UsePersistentBonusTracker()` → adds `PersistentBonusTrackerComponent` instead

Only one variant should be active per game context. If both flags are set, `BonusSystemConfig.Configure` adds **both** decorators independently, so every `BonusManager` entity receives both a `BonusTrackerComponent` and a `PersistentBonusTrackerComponent` — pick exactly one.

## Key types

| Type | Purpose |
|---|---|
| [`BonusTrackerItem`](../../ModdableTimberborn/BonusSystem/BonusTrackerItem.cs) | Immutable struct pairing a string `Id` with a list of `BonusSpec` values |
| [`BonusTracker`](../../ModdableTimberborn/BonusSystem/BonusTracker.cs) | Core logic; owns the keyed dictionary and proxies `BonusManager` |
| [`IBonusTrackerComponent`](../../ModdableTimberborn/BonusSystem/IBonusTrackerComponent.cs) | Common interface for both component variants; extension methods target this |
| [`BonusTrackerComponent`](../../ModdableTimberborn/BonusSystem/BonusTrackerComponent.cs) | Transient decorator component (no persistence) |
| [`PersistentBonusTrackerComponent`](../../ModdableTimberborn/BonusSystem/PersistentBonusTrackerComponent.cs) | Persistent variant; implements `IPersistentEntity` |
| [`BonusSystemHelpers`](../../ModdableTimberborn/BonusSystem/BonusSystemHelpers.cs) | String constants for the six standard bonus IDs |
| [`BonusSystemExtensions`](../../ModdableTimberborn/BonusSystem/BonusSystemExtensions.cs) | Fluent `AddOrUpdate` / `Remove` / `CurrentBonuses` on any `IBonusTrackerComponent` |

The `BonusType` enum ([`../../ModdableTimberborn/Helpers/CommonEnums.cs`](../../ModdableTimberborn/Helpers/CommonEnums.cs)) mirrors the same six bonuses as typed members: `CarryingCapacity`, `CuttingSuccessChance`, `GrowthSpeed`, `LifeExpectancy`, `MovementSpeed`, `WorkingSpeed`.

## Constructing a BonusTrackerItem

A `BonusTrackerItem` groups one or more bonuses under a single string key. Several constructors are available:

```cs
// Multiple bonuses — using BonusType enum (extension method .ToBonusSpec)
// or using string constants from BonusSystemHelpers
var item = new BonusTrackerItem("MyBuff", [
    BonusType.MovementSpeed.ToBonusSpec(0.5f),            // +50 % movement speed
    new(BonusSystemHelpers.WorkingSpeedId, 0.25f),        // +25 % working speed
]);

// Single bonus from a BonusType enum value
var item = new BonusTrackerItem("MyBuff", BonusType.MovementSpeed, 0.5f);

// Single bonus from a raw string ID
var item = new BonusTrackerItem("MyBuff", BonusSystemHelpers.WorkingSpeedId, 0.25f);
```

The `MultiplierDelta` values are additive modifiers on top of the base multiplier — `0.5f` means +50 %.

Declare static items at class level so they are only constructed once:

```cs
static readonly BonusTrackerItem MyBuffBonuses = new("MyBuff", [
    BonusType.MovementSpeed.ToBonusSpec(0.5f),
    new(BonusSystemHelpers.WorkingSpeedId, 0.25f),
]);
```

## Obtaining the tracker component

Use the extension methods from [`../../ModdableTimberborn/Helpers/CharacterExtensions.cs`](../../ModdableTimberborn/Helpers/CharacterExtensions.cs):

```cs
// Transient variant
BonusTrackerComponent tracker = worker.GetBonusTracker();

// Persistent variant
PersistentBonusTrackerComponent tracker = worker.GetPersistentBonusTracker();
```

Both return `null` if the corresponding subsystem has not been enabled or if the entity has no `BonusManager`. Both types implement `IBonusTrackerComponent`, so `BonusSystemExtensions` methods work on either.

> **Note:** The helpers are generic (`where T : BaseComponent`) and `BonusSystemConfig` decorates any entity that has a `BonusManager` component — not only characters. You can call `GetBonusTracker()` / `GetPersistentBonusTracker()` on any `BaseComponent` that belongs to such an entity.

## Applying and removing bonuses

Call `AddOrUpdate` to apply a bonus group, and `Remove` to take it away. If a group with the same `Id` already exists, `AddOrUpdate` atomically removes the old bonuses and applies the new ones — there is no double-application.

```cs
// Apply (or replace) the bonus group
tracker.AddOrUpdate(MyBuffBonuses);

// Remove it by ID
tracker.Remove(MyBuffBonuses.Id);
```

`AddOrUpdate` and `Remove` fire the `BonusTracker.OnBonusChanged` event with a `BonusTrackerChangeEventArgs` that carries the old and new `BonusTrackerItem` values, useful for UI reactions.

To check whether a specific bonus group is currently applied — and retrieve it — use `BonusTracker.TryGetBonus`:

```cs
if (tracker.BonusTracker.TryGetBonus("MyBuff", out BonusTrackerItem? item))
{
    // item.Bonuses contains the active BonusSpec list
}
```

This is more targeted than iterating `CurrentBonuses` when you only care about one key.

`BonusTracker` also exposes `AddOrUpdateOrRemove(BonusTrackerItem)` directly. It calls `Remove` automatically when every `MultiplierDelta` in the item is zero, and `AddOrUpdate` otherwise — convenient when the bonus magnitude is computed at runtime and may drop to zero.

## Worked example — workplace buff

The demo's [`DemoWorkplaceBuffComponent`](../../ModdableTimberbornDemo/Features/WorkplaceBuff/DemoWorkplaceBuffComponent.cs) applies bonuses to every worker assigned to a workplace. It extends `TogglableWorkplaceBonusComponent`, which handles the `WorkerAssigned`/`WorkerUnassigned` bookkeeping automatically:

```cs
using ModdableTimberborn.BonusSystem;

public class DemoWorkplaceBuffComponent : TogglableWorkplaceBonusComponent, IEntityEffectDescriber
{
    static readonly BonusTrackerItem DemoBonuses = new("DemoWorkplaceBuff", [
        BonusType.MovementSpeed.ToBonusSpec(1f),           // Either use BonusType enum
        new(BonusSystemHelpers.WorkingSpeedId, 1f),        // Or constant from BonusSystemHelpers
    ]);

    public int Order { get; }
    protected override BonusTrackerItem Bonuses { get; } = DemoBonuses;

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => Active
            ? new("Demo Workplace Buff",
                  t.T("LV.DemoMT.WorkplaceBuffDesc",
                      DemoBonuses.Bonuses[0].MultiplierDelta,
                      DemoBonuses.Bonuses[1].MultiplierDelta))
            : null;
}
```

Register the component as a decorator on `Workplace` from your configurator:

```cs
public class WorkplaceBuffConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindTemplateModule(h => h
                .AddDecorator<Workplace, DemoWorkplaceBuffComponent>()
            );
    }
}
```

`TogglableWorkplaceBonusComponent` (source: [`../../ModdableTimberborn/WorkSystem/`](../../ModdableTimberborn/WorkSystem/)) requires `UseBonusTracker()` (non-persistent) to be enabled. When `Active` is true the component calls `worker.GetBonusTracker().AddOrUpdate(Bonuses)` on assignment and `.Remove(Bonuses.Id)` on unassignment.

## Persistence behavior

When `UsePersistentBonusTracker()` is enabled, `PersistentBonusTrackerComponent` saves the full `CurrentBonuses` dictionary to the entity's save data. On load the saved items are held in a staging list and replayed in `Start` (after `BonusManager` is fully initialized). If `CurrentBonuses` is empty at save time nothing is written.

The non-persistent `BonusTrackerComponent` applies no save/load logic — any bonuses applied at runtime are lost when the save is loaded and must be reapplied by your code.

## Standard bonus IDs

`BonusSystemHelpers` provides string constants, and `BonusType` provides the equivalent enum values, for the six built-in bonuses:

| Constant | Enum value | Effect |
|---|---|---|
| `BonusSystemHelpers.CarryingCapacityId` | `BonusType.CarryingCapacity` | Carry capacity multiplier |
| `BonusSystemHelpers.CuttingSuccessChanceId` | `BonusType.CuttingSuccessChance` | Tree-cutting success chance |
| `BonusSystemHelpers.GrowthSpeedId` | `BonusType.GrowthSpeed` | Growth speed multiplier |
| `BonusSystemHelpers.LifeExpectancyId` | `BonusType.LifeExpectancy` | Life expectancy multiplier |
| `BonusSystemHelpers.MovementSpeedId` | `BonusType.MovementSpeed` | Movement speed multiplier |
| `BonusSystemHelpers.WorkingSpeedId` | `BonusType.WorkingSpeed` | Working speed multiplier |

Any `BonusSpec` ID accepted by Timberborn's `BonusManager` can be used — these constants cover the most common cases.
