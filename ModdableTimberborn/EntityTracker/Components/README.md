# EntityTracker/Components

## Purpose

Decorator components that attach to existing Timberborn entity templates to capture and expose the data each specialised tracker needs at registration time. They run `Awake()` once per entity and then sit as lightweight, pre-resolved property bags.

## Key types

- **`CharacterTrackerComponent`** — decorates `Character` entities. Resolves and caches `CharacterType` (adult beaver / child beaver / bot), a `Worker` reference (nullable — not all characters are workers), and an optional `BonusTrackerComponent`. Exposes convenience bool properties (`IsBot`, `IsBeaver`, `IsAdult`, `IsChild`, `IsWorker`) and `HasBonus(string id)`.
- **`WorkplaceTrackerComponent`** — decorates `WorkplaceSpec` entities. Caches the `Workplace` component, a `TemplateName` string (from `TemplateSpec`), and the `IsBuilderWorkplace` flag.

## How it fits together

Both components are registered as template decorators in `EntityTrackerConfig` via `BindTemplateModule`. Because they implement `IAwakableComponent`, the engine calls `Awake()` before the entity is fully initialised. By the time `EntityInitializedEvent` fires and `CharacterTracker` / `WorkplaceTracker` call `Track`, the components are already populated.

`CharacterTracker` uses `CharacterTrackerComponent.CharacterType` to route each entity into the correct internal set (adults, children, bots). `WorkplaceTracker` reads `WorkplaceTrackerComponent.Workplace` to wire up worker-assignment events.

## Dependencies & patterns

- `IAwakableComponent` — Timberborn lifecycle hook; `Awake()` is the only lifecycle method used here.
- `BaseComponent` — Timberborn base for all entity components.
- `Worker`, `Workplace`, `TemplateSpec`, `CharacterType` — standard Timberborn game types resolved via `GetComponent<T>()`.
- `BonusTrackerComponent` — from the BonusTracker subsystem of ModdableTimberborn; resolved optionally in `CharacterTrackerComponent`.

## Notes / gotchas

- `WorkplaceTrackerComponent` uses `#nullable disable` for its three properties because they cannot be set in the constructor (set in `Awake`). Consumers should treat them as always-set after entity initialisation.
- `CharacterTrackerComponent.Worker` is properly nullable (`Worker?`) and guarded by `IsWorker`; bots and children may not have a `Worker` component.
