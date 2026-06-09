# WorkSystem

## Purpose

Provides togglable effect/bonus components scoped to a `Workplace` and its assigned `Worker`s. When the component is active, bonuses (or arbitrary effects) are applied to workers as they are assigned and removed when they are unassigned. Mirrors the EnterableSystem pattern, but targets the work-assignment relationship rather than physical building entry.

## Key types

- **`TogglableWorkplaceEffectComponent`** — Abstract `BaseEventTogglableContainerComponent` subclass (one step above the bonus layer). Subclasses override `OnWorkerAssigned(Worker)` and `OnWorkerUnassigned(Worker)` to apply any arbitrary effect. The `CreateData` override wires a `WorkplaceTogglableContainer` against the sibling `Workplace` component.
- **`TogglableWorkplaceBonusComponent`** — Concrete-but-abstract subclass of `TogglableWorkplaceEffectComponent`. Subclasses only need to supply a `BonusTrackerItem Bonuses` property; `OnWorkerAssigned`/`OnWorkerUnassigned` are implemented here to call `worker.GetBonusTracker().AddOrUpdate(Bonuses)` and `.Remove(Bonuses.Id)`.
- **`WorkplaceTogglableContainer`** — Plain C# class (no Unity dependency) extending `BaseEventTogglableContainer<Workplace, Worker>`. Treats `Workplace.AssignedWorkers` as the member enumerable; subscribes to `Workplace.WorkerAssigned` / `WorkerUnassigned` events to call `OnMemberAdded`/`OnMemberRemoved`, which delegate to the constructor-injected `onWorkerAssigned`/`onWorkerUnassigned` action callbacks.

## How it fits together

The three-layer inheritance chain runs:

```
BaseEventTogglableContainerComponent  (Common — Unity component shell)
  └─ TogglableWorkplaceEffectComponent  (WorkSystem — Workplace/Worker wiring)
       └─ TogglableWorkplaceBonusComponent  (WorkSystem — BonusTracker integration)
            └─ [mod-specific concrete class]
```

`Awake` on `BaseEventTogglableContainerComponent` calls `CreateData`, which constructs a `WorkplaceTogglableContainer` backed by the co-located `Workplace` component. The container is dormant until `Toggle(true)` is called. On activation, `BaseEventTogglableContainer.PerformActivate` hooks the `WorkerAssigned`/`WorkerUnassigned` events and immediately fires `OnMemberAdded` for every already-assigned worker, so bonuses are applied retroactively. Deactivation mirrors this: event handlers are removed and `OnMemberRemoved` is called for current members.

For the bonus variant, mod authors subclass `TogglableWorkplaceBonusComponent`, return a `BonusTrackerItem` from `Bonuses`, and attach the resulting component to any building prefab that has a `Workplace`. The building also needs a mechanism to call `Toggle(true/false)` — typically a building-settings component or a game-event listener.

**EnterableSystem parallel:** `EnterableSystem/TogglableEnterableBonusComponent` is structurally identical, but uses `Enterable`/`Enterer` instead of `Workplace`/`Worker`. The two subsystems are independent; there is no shared base between them.

## Dependencies & patterns

- **Timberborn game types:** `Workplace`, `Worker`, `WorkerChangedEventArgs` (Timberborn's work-assignment API); `BaseComponent` (Unity wrapper).
- **BonusSystem:** `BonusTrackerItem`, `worker.GetBonusTracker()` (extension method from BonusSystem). `TogglableWorkplaceBonusComponent` requires `ModdableTimberbornRegistry.UseBonusTracker(bool)` to be called at mod startup so that `BonusTrackerComponent` is decorated onto `Worker` entities. Without this, `GetBonusTracker()` will fail at runtime.
- **Common abstractions:** `BaseEventTogglableContainerComponent`, `BaseEventTogglableContainer`, `ITogglableContainer` (all from `ModdableTimberborn.Common`).
- **No DI registration in this folder.** WorkSystem defines only abstract/base component types; concrete subclasses are registered by the consuming mod's configurator. No `[MultiBind]`, no Bindito configurator, no Harmony patches.
- **No serialization.** Toggle state is not persisted here; if a mod needs toggle state to survive save/load it must handle that itself.

## Notes / gotchas

- `TogglableWorkplaceBonusComponent` requires `ModdableTimberbornRegistry.UseBonusTracker(bool)` to be called at mod startup. If that call is missing, `GetBonusTracker()` will throw a missing-component error at the first worker assignment.
- There is no DI-based or attribute-based auto-registration for these types. A mod that wants to apply the same bonus to all workplaces must enumerate entities itself (e.g., via `WorkplaceTracker` from EntityTracker) and call `Toggle` manually, or wire up a building-settings toggle.
- The folder contains only one file (`TogglableWorkplaceBonusComponent.cs`), which defines all three types. The naming of the file omits the `Effect` variant (`TogglableWorkplaceEffectComponent`) — the file name reflects the most-used concrete layer, not the full contents.
- The `WorkplaceTogglableContainer` does not unsubscribe from `Workplace` events if the containing `GameObject` is destroyed without going through `Toggle(false)` first. Consumers should ensure deactivation before destruction to avoid stale event handlers.
