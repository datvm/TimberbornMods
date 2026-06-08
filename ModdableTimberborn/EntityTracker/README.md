# EntityTracker

## Purpose

Maintains live, categorised sets of in-game entities (characters, workplaces, and any mod-registered component type) so that other systems can query "all adults", "all builder workplaces", etc. without walking the full entity list themselves. It hooks into Timberborn's `EntityInitializedEvent` / `EntityDeletedEvent` to keep the sets up to date automatically.

## Key types

- **`IEntityTracker`** — non-generic base interface; exposes `Track(EntityComponent)` and `Untrack(EntityComponent)`. Every tracker must implement this so `EntityTrackerController` can fan out events to all of them.
- **`IEntityTracker<T>`** — generic extension; adds `Entities: IReadOnlyCollection<T>` and `OnEntityRegistered` / `OnEntityUnregistered` events for typed consumers.
- **`DefaultEntityTracker<T>`** — generic, sealed implementation of `IEntityTracker<T>`. Used automatically for any component type registered via `ModdableTimberbornRegistry.TryTrack<T>()`. Silently skips entities that don't carry the requested component.
- **`CharacterTracker`** — specialised tracker for characters. Maintains separate sets for adult beavers, child beavers, and bots, plus convenience enumerables (`Beavers`, `Workers`). Provides `GetCharacters(CharacterType)` and `GetCharacters<T>(CharacterType)` for filtered iteration.
- **`WorkplaceTracker`** — specialised tracker for workplaces. Adds `BuilderWorkplaces` view and forwards `Workplace.WorkerAssigned` / `WorkerUnassigned` events as `OnWorkerAssigned` / `OnWorkerUnassigned` on the tracker itself.
- **`EntityTrackerController`** — singleton `ILoadableSingleton`. Registers with the `EventBus` on load and dispatches `EntityInitializedEvent` / `EntityDeletedEvent` to every `IEntityTracker` in the multi-binding.
- **`EntityTrackerConfig`** — `IModdableTimberbornRegistryConfig` that wires up the DI graph. Also hosts the partial `ModdableTimberbornRegistry` extension that adds `UseEntityTracker()` and `TryTrack<T>()`.

## How it fits together

`EntityTrackerController` is the single listener for entity lifecycle events. On each event it iterates every `IEntityTracker` registered in the multi-binding and calls `Track` or `Untrack`. Each tracker's `Track`/`Untrack` implementation calls `entity.GetComponent<T>()` and no-ops if the component is absent, making the fan-out safe regardless of entity type.

Consumers inject the specific typed tracker they need (e.g. `CharacterTracker`, `WorkplaceTracker`, or `DefaultEntityTracker<MyComp>`) and either iterate `Entities` or subscribe to the registered/unregistered events.

Mod authors opt in via the registry fluent API:

```csharp
ModdableTimberbornRegistry.Instance
    .UseEntityTracker()          // activates EntityTrackerConfig
    .TryTrack<MyComponent>();    // registers DefaultEntityTracker<MyComponent>
```

`TryTrack<T>` guards against redundantly registering the built-in character/workplace types and throws an `InvalidOperationException` with a helpful message if misused.

## Dependencies & patterns

- **Bindito (DI):** `MultiBind` / `MultiBindAndBindSingleton` for `IEntityTracker`; `BindSingleton` for individual trackers and the controller. Open-generic `DefaultEntityTracker<T>` is registered at runtime via `MakeGenericType`.
- **Timberborn events:** `EventBus` + `[OnEvent]` attributes on `EntityTrackerController`. Relies on `EntityInitializedEvent` and `EntityDeletedEvent`.
- **Entity component model:** `EntityComponent`, `BaseComponent`, `IAwakableComponent` from Timberborn's entity system.
- **Template decoration:** `BindTemplateModule` adds `CharacterTrackerComponent` and `WorkplaceTrackerComponent` as decorators on `Character` and `WorkplaceSpec` prefabs respectively.
- **Registry pattern:** `EntityTrackerConfig` is a partial class split across the `ModdableTimberborn.EntityTracker` namespace (config impl) and `ModdableTimberborn.Registry` namespace (registry extension), both in the same file.

## Notes / gotchas

- The `#nullable disable` block in `WorkplaceTrackerComponent` suppresses nullability on `Workplace`, `IsBuilderWorkplace`, and `TemplateName` because they are set in `Awake()` rather than the constructor.
- `CharacterTrackerComponent` carries a reference to `BonusTrackerComponent` (from a different subsystem), making it a small aggregation hub for per-character cross-cutting concerns.
