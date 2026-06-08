# GameModes

## Purpose

Provides a thin runtime layer for reading, persisting, and matching the active game mode (difficulty preset) in a ModdableTimberborn save. It captures which `GameModeSpec` was chosen at new-game time, reconstructs what the live game settings actually are mid-save (in case mods changed them), and scores how closely the live state still matches any known preset.

## Key types

- **`GameDifficultyEnum`** — Simple enum (`Custom`, `Easy`, `Medium`, `Hard`) used as a consumer-friendly label when the raw `GameModeSpec.DisplayNameLocKey` needs to be bucketed into a named difficulty tier.
- **`PersistentGameModeService`** — `[BindSingleton]` service that loads, reconstructs, and saves game-mode state. Implements `ILoadableSingleton` and `ISaveableSingleton`.

## How it fits together

On load, `PersistentGameModeService.Load()` runs two paths:

1. **New game** — reads the chosen `GameModeSpec` directly from `GameSceneParameters.NewGameConfiguration.GameMode` and stores it as both `StartedMode` and `ReconstructedMode`.
2. **Continue save** — deserialises the originally-chosen mode from the save file (JSON under key `"StartedMode"`), then **reconstructs** what the current mode actually looks like by reading live values from several game services (`NeedModificationService`, `TemperateWeatherDurationService`, `DroughtWeather`, `BadtideWeather`, `EffectProbabilityService`, `GoodRecoveryRateService`).

After reconstruction, `TryMatchMode()` iterates all known `GameModeSpec` objects from `GameModeSpecService.GetSpecsOrdered()`, scores each one by counting how many *recoverable* properties match the reconstructed mode, and exposes `BestMatchedMode` + `MatchScore` (0–1 ratio).

`CheckForModifiedStart()` compares `StartedMode` with `ReconstructedMode` to set `StartedModeModified`, signalling that the live game settings have drifted from what was originally chosen.

The extension method `GameModeSpec.GetDifficultyEnum()` in `Helpers/CommonExtensions.cs` maps the game's localisation key to `GameDifficultyEnum` — this is the primary consumer of that enum.

**Consumer usage:** other subsystems inject `PersistentGameModeService` to check which difficulty the player started on (`StartedMode`), what it looks like now (`ReconstructedMode`), and whether settings have been tampered with (`StartedModeModified`).

## Dependencies & patterns

- **DI:** `[BindSingleton]` (Bindito). No explicit configurator — registration is attribute-driven.
- **Save/Load:** Timberborn's `ISingletonLoader` / `ISingletonSaver` with a `SingletonKey("PersistentGameModeService")` and a single `PropertyKey<string>("StartedMode")`.
- **Serialisation:** `JsonConvert` (Newtonsoft.Json) used to serialise/deserialise `GameModeSpec` as JSON. Before saving, `ComponentSpec.Blueprint` is nulled out on `StartedMode` and all its `MinMaxSpec<>` sub-properties to avoid serialising large blueprint references.
- **Reflection:** `RecoverableProperties` and `MinMaxProperties` are computed once at class-init via `typeof(GameModeSpec).GetProperties(...)` and stored as `static readonly FrozenSet`/`ImmutableArray`. `UnrecoverableProperties` excludes starting population/food/water fields from matching (these can't be "recovered" from a running game).
- **Timberborn game types used:** `GameModeSpec`, `GameModeSpecService`, `GameSceneParameters`, `NeedModificationService`, `TemperateWeatherDurationService`, `DroughtWeather`, `BadtideWeather`, `EffectProbabilityService`, `GoodRecoveryRateService`, `ComponentSpec`.

## Notes / gotchas

- `GameModeSpecService` and `GameModeSpec` are Timberborn game types — they do not live in this repo. If the game updates their property set, the reflection-based scoring loop silently adapts, but `UnrecoverableProperties` (hardcoded by name) may need manual updates.
