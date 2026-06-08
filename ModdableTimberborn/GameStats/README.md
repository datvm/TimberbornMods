# GameStats

## Purpose

Provides a unified, string-keyed lookup service for game-wide statistics (population counts, good amounts, fill rates, etc.). It exists so that other subsystems — e.g. UI, condition checkers, or dynamic text — can query any stat by name without hard-coding calls to Timberborn's individual services. The design is intentionally extensible: new stat providers can be dropped in without touching the core service.

## Key types

- **`IGameStatProvider`** — base interface every stat source implements. Declares `AvailableStats` (string keys it owns), `OutputType`, `GetStat(string)`, and a default `GetStatFormatted(string)`.
- **`IGameStatProvider<T>`** — generic typed variant; provides `OutputType` automatically and bridges the typed `GetStat` back to the untyped interface.
- **`IIntGameStatProvider`**, **`IFloatGameStatProvider`**, **`IStringGameStatProvider`**, **`ISpriteGameStatProvider`** — convenience marker interfaces for the four common value types.
- **`IPercentGameStatProvider`** — specialisation of `IGameStatProvider<float>` that overrides `GetStatFormatted` to render the value as `"P0"` percentage (or `"N/A"` when negative).
- **`GameStatService`** — singleton that collects all registered `IGameStatProvider`s at load time, indexes them into a `FrozenDictionary<string, IGameStatProvider>` keyed by stat ID, and exposes `GetStat`, `GetStatFormatted`, `TryGetStat<T>`, and `GetProvider`/`GetProvider<T>`.
- **`GameStatsConfig`** — Bindito configurator; auto-discovers every non-abstract `IGameStatProvider` in the assembly via reflection and multi-binds it, then binds `GameStatService` as a singleton. Also extends `ModdableTimberbornRegistry` with `UseGameStats()`.

## How it fits together

`ModdableTimberbornRegistry.UseGameStats()` is the opt-in entry point — calling it registers `GameStatsConfig` with the DI container. `GameStatsConfig.Configure` scans the assembly for `IGameStatProvider` implementations, multi-binds each one, and binds `GameStatService`. At game load, `GameStatService.Load` aggregates all injected providers into the frozen lookup table; duplicate stat IDs throw immediately to catch registration bugs early.

Consumers call `GameStatService.GetStat("SomeStatId")` (or the typed/formatted overloads) to read any value. Because the lookup is by string key, stats can be referenced from data (JSON/configs) without code dependencies on specific providers.

The `Implementations/` subfolder contains the concrete providers shipped with the library (see its own README).

## Dependencies & patterns

- **Bindito DI**: `[MultiBind]`-style multi-binding done programmatically in `GameStatsConfig`; `ILoadableSingleton` drives `GameStatService.Load`.
- **`IModdableTimberbornRegistryConfig`**: standard ModdableTimberborn configurator pattern for opt-in feature registration.
- **`ModdableTimberbornRegistry` partial class**: `UseGameStats()` extends the registry fluent builder.
- **`FrozenDictionary`** (.NET 8): used for the post-load lookup table — read-only after `Load()`, so freeze is appropriate.
- No Harmony patches in this folder.

## Notes / gotchas

- Stat ID collisions across providers cause an immediate `Exception` in `Load()` — this is intentional and good, but means any third-party provider must use unique prefixes (see the `GoodAmount.`, `GoodCapacity.`, `GoodFill.` prefix pattern in the built-in providers).
- `GetStat<T>` and `GetProvider<T>` cast without checking `OutputType`; a mismatch throws `InvalidCastException` at runtime. Use `TryGetStat<T>` when the type is uncertain.
- `GetStatFormatted` falls back to `ToString()` on the base interface; only `IPercentGameStatProvider` has a custom formatter — other providers would need to override it explicitly.
- Auto-discovery in `GameStatsConfig` scans `typeof(GameStatsConfig).Assembly` only, so providers defined in other assemblies (mods, etc.) must register themselves separately.
