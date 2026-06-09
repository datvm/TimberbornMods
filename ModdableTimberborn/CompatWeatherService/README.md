# CompatWeatherService

## Purpose

Provides a stable, mod-friendly abstraction over Timberborn's internal weather system. Because the game's weather API may change between versions (or differ when other mods are present), this subsystem decouples consumers from concrete game types by routing all weather queries through a priority-selected provider. The default implementation wraps the vanilla `WeatherService`, `HazardousWeatherService`, and `GameCycleService`.

## Key Types

- **`CompatWeatherService`** — Singleton entry point. Resolves the highest-priority registered `ICompatWeatherServiceProvider` at construction time and exposes it as the single `Provider` property. Throws if no provider is registered.
- **`ICompatWeatherServiceProvider`** — The abstraction contract. Covers current/next cycle stages, hazardous-weather state, warning status, and the list of known weather types. Implementations declare a `Priority` to win provider selection.
- **`DefaultCompatWeatherServiceProvider`** — The built-in implementation wrapping vanilla game services (priority `-1`, so any mod-supplied provider wins by default). Enumerates the three stock weather types: Temperate, Drought, and Badtide.
- **`CompatWeatherType`** — Readonly record: a weather type's string `Id`, display localisation key (`DisplayLoc`), and `IsBenign` flag.
- **`CompatWeatherCycle`** — Represents a full weather cycle: cycle number and an ordered array of `CompatWeatherCycleStage` entries. `Length` sums all stage lengths (returns 1 for empty stage arrays to avoid a zero-length cycle).
- **`CompatWeatherCycleStage`** — A single stage within a cycle: cycle index, stage index, `StartDay`, `Length`, `WeatherId`, and `IsBenign`. `LastDay` is a derived convenience property.
- **`CompatNextWeatherCycleStage`** — Like `CompatWeatherCycleStage` but with nullable `Length`, `WeatherId`, and `IsBenign` to represent a future stage whose details may not yet be known. Includes an implicit conversion from `CompatWeatherCycleStage`.
- **`CompatWeatherWarning`** — Snapshot of the current hazard-approach warning: `Stage` enum, `DaysToHazardous` (float, nullable), and `NextWeatherId`.
- **`CompatWeatherWarningStage`** (enum) — `NoWarning`, `ShowedToday`, `Showing`, `Hazardous`, `NoHazardous`.

## How It Fits Together

`CompatWeatherService` is a `[BindSingleton]` that receives all registered `ICompatWeatherServiceProvider` instances via DI constructor injection. On creation it picks the one with the highest `Priority` and exposes it as `Provider`. Consumers depend on `CompatWeatherService` and call `Provider.*` for all weather queries — they never touch the game's `WeatherService` directly.

`DefaultCompatWeatherServiceProvider` is registered via `[MultiBind(typeof(ICompatWeatherServiceProvider))]`, making it part of the DI multi-binding collection automatically. Other mods or assemblies can supply competing implementations with a higher priority (any value `>= 0` beats the default's `-1`) to override weather data without touching existing consumers.

The provider builds cycle/stage objects on-the-fly from live game-service data; nothing is cached. Warning logic in `GetWarningStatus` uses `HazardousWeatherApproachingTimer.DaysToHazardousWeather` and the timer's spec to classify the warning into one of the five `CompatWeatherWarningStage` buckets.

## Dependencies & Patterns

- **Bindito (DI):** `[BindSingleton]` on `CompatWeatherService`; `[MultiBind(typeof(ICompatWeatherServiceProvider))]` on `DefaultCompatWeatherServiceProvider` for the provider collection pattern.
- **Timberborn game services injected into `DefaultCompatWeatherServiceProvider`:** `WeatherService`, `GameCycleService`, `HazardousWeatherService`, `HazardousWeatherApproachingTimer`.
- **Unity:** `Mathf.CeilToInt` used in warning-stage boundary calculation.
- **`ImmutableArray<T>`** (System.Collections.Immutable) for the stage list in `CompatWeatherCycle` and the static `GameWeathers` array.
- No Harmony patches in this folder; no serialization attributes.

## Notes / Gotchas

- Priority `-1` for the default is intentional: any externally registered provider with priority `0` or above takes over entirely. Consumers should be aware that `Provider` is fully replaced, not composed — there is no fallback or chaining.
- `GetNextCycleStage` returns a next-cycle stage with `null` Length/WeatherId when the current cycle has no hazardous stage and the game is already in (or past) hazardous weather; consumers must handle nullable fields on `CompatNextWeatherCycleStage`.
