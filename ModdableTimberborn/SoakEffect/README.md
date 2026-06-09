# SoakEffect

## Purpose

Provides per-tick water-exposure tracking for every `Character` in the game. Each tick, the component checks whether the character's grid coordinate is submerged, fires events so other components can react (e.g. apply damage, contamination effects), and maintains a simple in/out state flag. It exists to give mod components a single, efficient subscription point for water contact rather than each one querying the water map independently.

## Key types

- **`ModdableSoakEffectComponent`** — `TickableComponent` / `IAwakableComponent` decorator added to every `Character`. Queries `ThreadSafeWaterMap` each tick, fires `OnSoakedTick` while submerged and `IsInWaterStateChanged` when the in-water state changes. Respects any `IWaterResistor` siblings on the same entity.
- **`SoakEffectEventArgs`** — Readonly record struct passed with `OnSoakedTick`. Carries `Resistant` flag, the raw `ReadOnlyWaterColumn` (depth, contamination, floor), the character's `Vector3Int` grid coordinate, a pre-computed `WaterCeiling` (water surface z), and `PassedTimeInHours` from `IDayNightCycle`.
- **`ModdableSoakEffectConfig`** — Singleton `IModdableTimberbornRegistryConfig`. Binds `ModdableSoakEffectComponent` as a template-module decorator on `Character` when running in a Game context.
- **`ModdableTimberbornRegistry.UseSoakEffect()`** (partial, defined here) — Opt-in registration method; guards against double-registration via `SoakEffectUsed` flag.

## How it fits together

Activation is opt-in: a mod calls `registry.UseSoakEffect()` during its configuration phase (e.g. `BeavVsMachine`'s `MConfigs.cs`). This registers `ModdableSoakEffectConfig`, which uses Bindito's template-module system to inject `ModdableSoakEffectComponent` as a decorator on every `Character` prefab.

At runtime, `Awake()` collects all `IWaterResistor` sibling components so resistance checks are cheap per-tick. Each `Tick()`, `UpdateState()` converts the character's world position to a grid coordinate, samples the water column, computes whether the character is actually submerged (water ceiling >= character z), and returns a `SoakEffectEventArgs` if so.

Consumers subscribe to `OnSoakedTick` on the `ModdableSoakEffectComponent` they fetch via `GetComponentFast<>()`. The `BotWaterDamageComponent` in `BeavVsMachine` is the primary consumer: it scales durability loss by `PassedTimeInHours` and the column's `Contamination` value.

`IsInWaterStateChanged` is available for state-transition logic (e.g. entering/leaving water effects) but has no subscribers in the current codebase.

## Dependencies & patterns

- **Bindito DI:** `[Inject]` on `Inject(IThreadSafeWaterMap, IDayNightCycle)`. Registered as a template-module decorator (`AddDecorator<Character, ModdableSoakEffectComponent>()`), so the DI container instantiates one per character prefab.
- **Timberborn engine types:** `ThreadSafeWaterMap` / `IThreadSafeWaterMap`, `ReadOnlyWaterColumn`, `NavigationCoordinateSystem`, `IDayNightCycle`, `TickableComponent`, `IAwakableComponent`, `Character`, `IWaterResistor`.
- **Unity:** `Mathf.CeilToInt`, `Vector3Int`, `Transform.position`.
- **No Harmony patches.** Pure Bindito decorator injection.
- **`IWaterResistor`** — interface expected to be on sibling components; no implementation found in this folder (must be provided by Timberborn core or another mod component).

## Notes / gotchas

- `IWaterResistor` has no definition inside this repo's `SoakEffect` folder; it is a Timberborn core interface provided by the engine.
- `IsInWaterStateChanged` fires on every state flip, including re-entering water after a brief gap, so subscribers should not assume it fires only once per submersion event.
- `IsInWater` reflects unresisted water exposure: it is `false` when the character is water-resistant, even if physically submerged.
