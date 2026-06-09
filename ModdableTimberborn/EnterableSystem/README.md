# EnterableSystem

## Purpose

Provides base components that hook into Timberborn's `Enterable`/`Enterer` system so that effects (bonuses, tick-based logic) can be applied to characters while they are inside an enterable building, with the whole effect optionally toggled on/off. It bridges the generic togglable-container infrastructure in `Common` with Timberborn's enterable events.

## Key types

- **`TogglableEnterableEffectComponent`** (abstract) — Unity `BaseComponent` that reacts to enter/exit events. Subclass and implement `OnEnter(Enterer)` / `OnExit(Enterer)`. Backed by `TogglableEventEnterableContainer`.
- **`TogglableEnterableBonusComponent`** (abstract) — Concrete specialisation of the above for bonus-tracker use: on enter it calls `enterer.GetBonusTracker().AddOrUpdate(Bonuses)`, on exit it removes the bonus by id. Requires `ModdableTimberbornRegistry.UseBonusTracker(bool)` to be registered.
- **`TogglableEventEnterableContainer`** — Non-component data object wiring `Enterable.EntererAdded/EntererRemoved` events to `OnMemberAdded/OnMemberRemoved` in the `BaseEventTogglableContainer` pattern. Only hooks the events while the container is active.
- **`TogglableEnterableTickEffectComponent`** (abstract) — A `TickableComponent` variant. Implements `ITogglableContainer<Enterable, Enterer>` directly; calls `TickEffect()` every tick when active. Backed by `TogglableEnterableContainer`.
- **`TogglableEnterableTickEffectComponent<TData>`** (abstract) — Extends the above with a per-`Enterer` data dictionary. Override `GetData(Enterer)` to supply per-enterer state; the dictionary is kept in sync via `EntererAdded/Removed` events wired in `Awake`.
- **`TogglableEnterableContainer`** — Non-component data object wrapping `Enterable` for the tick variant. Both `PerformActivate` and `PerformDeactivate` are no-ops (toggle state is tracked but no event subscription is needed at the container level; the component layer handles events directly).

## How it fits together

Both component families follow the same structural pattern defined in `Common`:

1. A **component** (`TogglableEnterableEffectComponent` or `TogglableEnterableTickEffectComponent`) attaches to a building `GameObject` that already has an `Enterable` component.
2. `Awake()` locates the sibling `Enterable` via `GetComponent<Enterable>()` and creates the corresponding data/container object.
3. The container object tracks toggled state and, when active, either subscribes to `EntererAdded`/`EntererRemoved` events (event variant) or keeps a live `Dictionary<Enterer, TData>` that is updated from the same events (tick variant).
4. Consumers call `Toggle(bool)` / `Activate()` / `Deactivate()` (from `ITogglableContainer`) to enable or disable the effect; the `Toggled` event fires on state change.

The event variant (`TogglableEnterableEffectComponent`) fires its callbacks immediately on enter/exit and is appropriate for stateless or one-shot effects. The tick variant (`TogglableEnterableTickEffectComponent`) runs `TickEffect()` every game tick and is appropriate for continuous or accumulating effects.

`TogglableEnterableBonusComponent` is the ready-to-use concrete subclass for the common case of applying a `BonusTrackerItem` to enterers; mod code just declares a `BonusTrackerItem Bonuses` property and lets the base class do the work.

## Dependencies & patterns

- **Timberborn:** `Enterable`, `Enterer`, `EntererAddedEventArgs`, `EntererRemovedEventArgs`, `TickableComponent`, `BaseComponent` — all Timberborn game types.
- **Common (this repo):** `BaseEventTogglableContainerComponent<,,>`, `BaseEventTogglableContainer<,>`, `BaseTogglableContainer<,>`, `ITogglableContainer<,>` in `ModdableTimberborn.Common`.
- **BonusSystem (this repo):** `BonusTrackerItem`, `GetBonusTracker()` extension (from `ModdableTimberborn.Helpers.CharacterExtensions`). `TogglableEnterableBonusComponent` requires the BonusTracker feature to be opted-in via `ModdableTimberbornRegistry.UseBonusTracker(bool)`.
- No Harmony patches, no DI configurators, and no serialization in this folder. The components are plain Unity MonoBehaviours/`BaseComponent`s; DI wiring is the caller's responsibility.

## Notes / gotchas

- `TogglableEnterableContainer.PerformActivate()` and `PerformDeactivate()` are empty. The tick component wires events directly in `Awake` (unconditionally) rather than in activate/deactivate — meaning the internal `enterers` dictionary is updated regardless of the `Active` flag. Only `TickEffect()` is gated on `Active`. This differs from the event-variant pattern where event subscription is itself toggled.
