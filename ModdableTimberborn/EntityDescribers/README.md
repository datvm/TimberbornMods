# EntityDescribers

## Purpose

Provides a UI panel fragment that displays "active effects" on a selected entity (or its worker's workplace) in the Timberborn entity inspector. The subsystem defines a pluggable describer pattern so that any component on an entity (or its workplace) can contribute human-readable effect descriptions to a shared panel without coupling to a central registry.

## Key types

- **`IBaseEntityEffectDescriber`** — root marker interface; carries an `Order` property used to sort describers within the panel.
- **`IEntityEffectDescriber`** — single-effect variant; `Describe(ILoc, IDayNightCycle)` returns one optional `EntityEffectDescription`.
- **`IEntityMultiEffectsDescriber`** — multi-effect variant; `DescribeAll(...)` returns zero or more descriptions.
- **`IWorkplaceWorkerEffectDescriber`** — extends `IEntityEffectDescriber` for workplace components that need to describe a per-`Worker` effect.
- **`IWorkplaceEntityMultiEffectsDescriber`** — multi-effect counterpart of the above for workplaces.
- **`EntityEffectDescription`** — lightweight `readonly record struct` holding `Title`, `Description`, and an optional `RemainingHours` for time-limited effects.
- **`EntityEffectDescriberFragment`** — the `IEntityPanelFragment` / `IEntityFragmentOrder` implementation that drives the "Active Effects" panel section (`Order = -100`, so it sorts near the top).
- **`ModdableEntityDescriberConfigurator`** — `IModdableTimberbornRegistryConfig` that registers `EntityEffectDescriberFragment` as an ordered fragment in the Game context.

## How it fits together

`ModdableEntityDescriberConfigurator` is picked up by `ModdableTimberbornRegistry` and calls `configurator.BindOrderedFragment<EntityEffectDescriberFragment>()`, which registers the fragment with the game's entity inspector panel system.

When an entity is selected, `EntityEffectDescriberFragment.ShowFragment` uses `entity.GetComponents<IBaseEntityEffectDescriber>()` to collect every describer component living on that entity. It also checks whether the entity has a `Worker` with an active workplace, and if so collects `IBaseEntityEffectDescriber` components from the *workplace* entity too — enabling workplace buildings to describe effects they apply to their workers.

`UpdateFragment` iterates both lists in sorted order, dispatches to the appropriate interface variant (`IEntityEffectDescriber` / `IEntityMultiEffectsDescriber` on the entity side; `IWorkplaceWorkerEffectDescriber` / `IWorkplaceEntityMultiEffectsDescriber` on the workplace side), and appends localised lines via `ILoc` (`LV.MT.ActiveEffect`, `LV.MT.ActiveEffectTime`). The panel is hidden entirely when no descriptions are produced.

Consumers add new effect descriptions by implementing one of the four `IBaseEntityEffectDescriber` sub-interfaces directly on a Unity component attached to the relevant entity or building prefab — no central registration is required.

## Dependencies & patterns

- **`ILoc`** — Timberborn localisation service; injected via primary constructor and used for all display strings.
- **`IDayNightCycle`** — Timberborn time service; passed to describers so they can express time-remaining values.
- **`Worker` / `Workplace`** — vanilla Timberborn components looked up via `GetComponent` at runtime; no hard DI dependency.
- **`IEntityPanelFragment` / `IEntityFragmentOrder`** — Timberborn UI fragment contracts; `Order = -100` places this section early in the panel.
- **`IModdableTimberbornRegistryConfig`** — ModdableTimberborn's own DI configurator hook; context restricted to `ConfigurationContext.Game`.
- **`BindOrderedFragment<T>`** — Bindito/ModdableTimberborn helper that registers a UI fragment and respects `IEntityFragmentOrder`.
- No Harmony patches in this folder.

## Notes / gotchas

- Both describer lists (`describers`, `workplaceDescribers`) are `List<IBaseEntityEffectDescriber>` populated via `GetComponents` each time an entity is shown — they are cleared on `ClearFragment`, so there is no stale-reference risk, but it does allocate per-selection.
- `panel` and `lblEffects` are declared `#nullable disable` because they are initialised in `InitializeFragment` rather than the constructor; callers are expected to call `InitializeFragment` before `ShowFragment`.
- Default `Order` on both single- and multi-effect interfaces is `0` via explicit interface implementation; the fragment itself uses `-100`, meaning it will appear above most other fragments that do not override their order.
