# Effect Components

Effect components are abstract base classes you subclass to apply a togglable effect — often a bonus (see [Bonus System](./BonusSystem.MD)) — to characters based on a specific relationship. All three families follow the same pattern: a component attached to a `GameObject` monitors a set of members (enterers, assigned workers, or any character in water) and applies or removes an effect as membership changes, while respecting an on/off toggle.

The toggle infrastructure is defined in [`Common`](../../ModdableTimberborn/Common/): `ITogglableContainer<TContainer, TMember>` exposes `Toggle(bool)`, convenience `Activate()` / `Deactivate()` helpers, and a `Toggled` event. `BaseTogglableContainer` and `BaseEventTogglableContainer` implement the state machine; `BaseEventTogglableContainerComponent` bridges this into Unity's component lifecycle.

### Activating a component

Registering and decorating a building is not enough — every effect component starts **inactive** after `Awake` and will not run until something calls `Toggle(true)`. The caller is always consumer code, not the framework. Typical patterns:

- **UI fragment** — a panel fragment (e.g. `DemoEnterableBuffFragment`) calls `component.Toggle(v)` from an `onValueChanged` callback, giving the player direct control.
- **Game event / timer** — the component (or a sibling) calls `Toggle(false)` internally once a condition is met (e.g. the demo's auto-turn-off timer in `DemoEnterableBuffComponent.TickEffect`).
- **Save / load** — persistent state restores the last active value via `Toggle` during `Load`, so the effect survives a reload.

None of these are automatic. If you register a component but never call `Toggle(true)`, the effect will never run.

---

## Enterable effects

These components apply an effect while a beaver (an `Enterer`) is physically inside an `Enterable` building. They live in [`EnterableSystem`](../../ModdableTimberborn/EnterableSystem/).

There are two variants depending on how you want to drive the logic.

### Event-driven: `TogglableEnterableEffectComponent`

Fires a callback immediately when an enterer enters or exits. Suitable for one-shot changes (e.g. applying a bonus).

```cs
public abstract class TogglableEnterableEffectComponent
    : BaseEventTogglableContainerComponent<TogglableEventEnterableContainer, Enterable, Enterer>
{
    protected abstract void OnEnter(Enterer enterer);
    protected abstract void OnExit(Enterer enterer);
}
```

When the component is toggled **on**, it subscribes to `Enterable.EntererAdded` / `EntererRemoved` and immediately calls `OnEnter` for every beaver already inside. Toggling **off** calls `OnExit` for current members and unsubscribes.

For the common case of applying a `BonusTrackerItem`, skip this layer entirely and use the ready-made subclass:

```cs
public abstract class TogglableEnterableBonusComponent : TogglableEnterableEffectComponent
{
    protected abstract BonusTrackerItem Bonuses { get; }
    // OnEnter → enterer.GetBonusTracker().AddOrUpdate(Bonuses)
    // OnExit  → enterer.GetBonusTracker().Remove(Bonuses.Id)
}
```

`TogglableEnterableBonusComponent` requires `ModdableTimberbornRegistry.Instance.UseBonusTracker(false)` to be called at startup (`false` = transient, not saved; use `UsePersistentBonusTracker()` / `UseBonusTracker(true)` if bonuses should survive reload).

### Tick-driven: `TogglableEnterableTickEffectComponent`

Runs `TickEffect()` on every game tick while the component is active. Use this when the effect accumulates over time (e.g. draining a need).

```cs
public abstract class TogglableEnterableTickEffectComponent : TickableComponent, ITogglableContainer<Enterable, Enterer>
{
    protected abstract void TickEffect();
    public void Toggle(bool active) { ... }
    // Also: Container, Members, Active, Toggled
}
```

When you also need per-enterer state (e.g. a reference to each beaver's `NeedManager`), use the generic overload:

```cs
public abstract class TogglableEnterableTickEffectComponent<TData> : TogglableEnterableTickEffectComponent
{
    protected abstract TData GetData(Enterer enterer);

    protected Dictionary<Enterer, TData> enterers;
    public IReadOnlyCollection<KeyValuePair<Enterer, TData>> Enterers { get; }
    public IReadOnlyCollection<TData> EnterersData { get; }
}
```

The dictionary is kept in sync via `EntererAdded` / `EntererRemoved` regardless of the `Active` flag, so `EnterersData` always reflects current occupants. Only `TickEffect()` is gated on `Active`.

> **Tick vs. event variant:** Unlike the event-driven `TogglableEnterableEffectComponent` — which subscribes to `EntererAdded`/`EntererRemoved` only while active and unsubscribes on deactivation — the tick component wires those subscriptions unconditionally in `Awake` and never tears them down. There is no `PerformActivate`/`PerformDeactivate` symmetry here; `TogglableEnterableContainer.PerformActivate` and `PerformDeactivate` are both no-ops. The `Active` flag only gates `Tick → TickEffect()`.

> **`base.Awake()` requirement:** The generic subclass `TogglableEnterableTickEffectComponent<TData>` overrides `Awake` to wire those subscriptions. If you override `Awake` in your own subclass, you **must** call `base.Awake()` — otherwise `data` (and `Container`) will be `null` at runtime.

### Registration

Attach your component to every `Enterable` building via a template-module decorator in your configurator:

```cs
public class EnterableBuffConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindTemplateModule(h => h
                .AddDecorator<Enterable, MyEnterableBuffComponent>()
            );
    }
}
```

### Demo example

[`DemoEnterableBuffComponent`](../../ModdableTimberbornDemo/Features/EnterableBuff/DemoEnterableBuffComponent.cs) in the demo project uses `TogglableEnterableTickEffectComponent<NeedManager>` to apply a wet-fur need penalty to every beaver inside a building, with an auto-turn-off timer and save/load support:

```cs
public class DemoEnterableBuffComponent
    : TogglableEnterableTickEffectComponent<NeedManager>, IPersistentEntity, IEntityEffectDescriber
{
    static readonly ContinuousEffect WetFurEffect = new("WetFur", .3f);

    protected override NeedManager GetData(Enterer enterer)
        => enterer.GetComponentFast<NeedManager>();

    protected override void TickEffect()
    {
        // Auto-disable after a set time
        if (turnOffAt <= dayNightCycle.PartialDayNumber)
        {
            Toggle(false);
            return;
        }

        var time = dayNightCycle.FixedDeltaTimeInHours;
        foreach (var needManager in EnterersData)
            needManager.ApplyEffect(WetFurEffect, time);
    }
}
```

Registration is in [`EnterableBuffConfig`](../../ModdableTimberbornDemo/Features/EnterableBuff/EnterableBuffConfig.cs).

---

## Workplace effects

These components apply an effect while a worker (`Worker`) is **assigned** to a `Workplace`. They live in [`WorkSystem`](../../ModdableTimberborn/WorkSystem/) and mirror the enterable pattern exactly.

### `TogglableWorkplaceEffectComponent`

Override `OnWorkerAssigned` and `OnWorkerUnassigned` for arbitrary effects:

```cs
public abstract class TogglableWorkplaceEffectComponent
    : BaseEventTogglableContainerComponent<WorkplaceTogglableContainer, Workplace, Worker>
{
    protected abstract void OnWorkerAssigned(Worker worker);
    protected abstract void OnWorkerUnassigned(Worker worker);
}
```

When toggled **on**, it subscribes to `Workplace.WorkerAssigned` / `WorkerUnassigned` and immediately calls `OnWorkerAssigned` for every already-assigned worker. Deactivation mirrors this: current workers are unassigned and event handlers are removed.

### `TogglableWorkplaceBonusComponent`

For the bonus case, subclass this and supply a `BonusTrackerItem`:

```cs
public abstract class TogglableWorkplaceBonusComponent : TogglableWorkplaceEffectComponent
{
    protected abstract BonusTrackerItem Bonuses { get; }
    // OnWorkerAssigned   → worker.GetBonusTracker().AddOrUpdate(Bonuses)
    // OnWorkerUnassigned → worker.GetBonusTracker().Remove(Bonuses.Id)
}
```

Requires `ModdableTimberbornRegistry.Instance.UseBonusTracker(false)` (or `UseBonusTracker(true)` / `UsePersistentBonusTracker()` for persistent bonuses).

### Registration

```cs
public class WorkplaceBuffConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindTemplateModule(h => h
                .AddDecorator<Workplace, MyWorkplaceBuffComponent>()
            );
    }
}
```

### Demo example

[`DemoWorkplaceBuffComponent`](../../ModdableTimberbornDemo/Features/WorkplaceBuff/DemoWorkplaceBuffComponent.cs) applies movement-speed and working-speed bonuses to assigned workers:

```cs
public class DemoWorkplaceBuffComponent : TogglableWorkplaceBonusComponent, IEntityEffectDescriber
{
    static readonly BonusTrackerItem DemoBonuses = new("DemoWorkplaceBuff", [
        BonusType.MovementSpeed.ToBonusSpec(1f),
        new(BonusSystemHelpers.WorkingSpeedId, 1f),
    ]);

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

Registration is in [`WorkplaceBuffConfig`](../../ModdableTimberbornDemo/Features/WorkplaceBuff/WorkplaceBuffConfig.cs).

---

## Soak effect

The soak effect tracks **per-tick water exposure** for every `Character` in the game. Rather than each mod component sampling the water map itself, one shared `ModdableSoakEffectComponent` does the query once per tick and raises events that interested components subscribe to.

### Enabling

Call `UseSoakEffect()` from your `IModStarter` before any game context initialises:

```cs
ModdableTimberbornRegistry.Instance
    .UseSoakEffect();
```

This registers `ModdableSoakEffectComponent` as a decorator on every `Character` prefab. The component is added once globally — calling `UseSoakEffect()` a second time is a no-op.

### Subscribing

Retrieve the component from the character and subscribe to one or both events:

```cs
var soakEffect = character.GetComponentFast<ModdableSoakEffectComponent>();

// Fires every tick while the character is in water (even if water-resistant).
soakEffect.OnSoakedTick += OnSoakedTick;

// Fires when the character enters or leaves water.
soakEffect.IsInWaterStateChanged += OnWaterStateChanged;
```

### `SoakEffectEventArgs`

`OnSoakedTick` delivers a `SoakEffectEventArgs` value (a `readonly record struct`):

| Member | Type | Description |
|---|---|---|
| `Resistant` | `bool` | True if a sibling `IWaterResistor` component reported the character as water-resistant. Check this before applying damage or debuffs. |
| `WaterColumn` | `ReadOnlyWaterColumn` | Raw column data from the water map, including depth, contamination, and floor height. |
| `Coordinate` | `Vector3Int` | The character's current grid coordinate. |
| `WaterCeiling` | `int` | Pre-computed water surface z: `Ceil(depth + floor)`. |
| `PassedTimeInHours` | `float` | Elapsed in-game time this tick (from `IDayNightCycle.FixedDeltaTimeInHours`). Use this to scale time-based effects. |

`IsInWaterStateChanged` carries a plain `bool` — `true` when the character enters water, `false` when they leave. `ModdableSoakEffectComponent.IsInWater` also exposes the current state as a property (it is `false` when the character is resistant, even if physically submerged).

### Example handler

```cs
void OnSoakedTick(object sender, SoakEffectEventArgs e)
{
    if (e.Resistant) { return; }

    // Scale an effect by contamination level and elapsed time
    var damage = e.WaterColumn.Contamination * e.PassedTimeInHours * DamageRate;
    ApplyDamage(damage);
}

void OnWaterStateChanged(object sender, bool isInWater)
{
    if (isInWater)
        StartWetAnimation();
    else
        StopWetAnimation();
}
```

Source: [`ModdableSoakEffectComponent.cs`](../../ModdableTimberborn/SoakEffect/ModdableSoakEffectComponent.cs), [`ModdableSoakEffectConfig.cs`](../../ModdableTimberborn/SoakEffect/ModdableSoakEffectConfig.cs).
