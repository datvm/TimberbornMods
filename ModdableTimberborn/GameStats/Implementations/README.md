# GameStats/Implementations

## Purpose

Concrete `IGameStatProvider` implementations that ship with ModdableTimberborn. They expose Timberborn's built-in population and goods data as named stats consumable through `GameStatService`.

## Key types

- **`GlobalPopulationStatsProvider`** (`IIntGameStatProvider`) — wraps `PopulationService.GlobalPopulationData` to expose integer counts for population, beds, workforce (beaver/bot/combined), and contamination. Workforce stat IDs are prefixed with the character-type name (e.g. `"BeaversEmployable"`, `"BotEmployable"`) while unprefixed IDs give the combined total.
- **`GlobalPopulationPercentStatsProvider`** (`IPercentGameStatProvider`) — wraps the same `GlobalPopulationData` to expose a small set of ratio stats (e.g. `"BeaverPercent"`, `"HomelessPercent"`, `"ContaminatedPercent"`). Formatted output is `"P0"` percentage via the interface default.
- **`GoodStatsProvider`** (`IIntGameStatProvider`) — iterates `IGoodService.Goods` at query time to expose per-good integer stats: `"GoodAmount.<goodId>"` (available stock) and `"GoodCapacity.<goodId>"` (input/output capacity), delegating to `ResourceCountingService.GetGlobalResourceCount`.
- **`GoodPercentStatsProvider`** (`IPercentGameStatProvider`) — iterates `IGoodService.Goods` to expose `"GoodFill.<goodId>"` (fill rate as a float 0–1).

## How it fits together

All four classes are auto-discovered and registered by `GameStatsConfig` via assembly reflection — no manual wiring needed. They are injected into `GameStatService` as an `IEnumerable<IGameStatProvider>` and their stat IDs are indexed at load time.

The goods providers use a **prefix + good-ID** naming scheme (`GoodAmount.Wood`, `GoodFill.Water`, etc.). `AvailableStats` is evaluated at registration time (during `GameStatService.Load`), so the set of known good IDs is snapshotted at that point.

## Dependencies & patterns

- **Timberborn services injected via constructor**: `PopulationService`, `IGoodService`, `ResourceCountingService`.
- Workforce stat IDs for beaver/bot are built with `nameof(CharacterType.Beavers)` / `nameof(CharacterType.Bot)` as prefixes — coupling the stat name to the Timberborn type name.
- `FrozenSet<string>` used for O(1) category dispatch in `GlobalPopulationStatsProvider`.

## Notes / gotchas

- `GoodStatsProvider` and `GoodPercentStatsProvider` call `IGoodService.Goods` in `AvailableStats` — this is snapshotted at `Load()`. Goods added dynamically after load would not appear.
- All unrecognised stat IDs throw `ArgumentOutOfRangeException` — consistent with the fail-loud policy.
