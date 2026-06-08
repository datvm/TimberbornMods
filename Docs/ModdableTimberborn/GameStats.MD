# Game Stats

Game Stats is a string-keyed registry of global game statistics. It decouples consumers — UI, condition checks, dynamic text — from Timberborn's individual data services. Any stat can be queried by name without a direct code dependency on the provider that supplies it.

## Enabling Game Stats

Call `UseGameStats()` on your `ModdableTimberbornRegistry` instance during plugin setup:

```cs
ModdableTimberbornRegistry.Instance.UseGameStats();
```

This registers [`GameStatsConfig`](../../ModdableTimberborn/GameStats/GameStatsConfig.cs) with the registry, which auto-discovers all built-in providers and binds [`GameStatService`](../../ModdableTimberborn/GameStats/GameStatService.cs) as a singleton.

## Reading a stat

Inject `GameStatService` and call one of its lookup methods:

```cs
// Untyped — returns object?
object? raw = statService.GetStat(GameStats.PopulationTotal);

// Typed — throws InvalidCastException on type mismatch
int pop = statService.GetStat<int>(GameStats.PopulationTotal);

// Safe typed — returns false if the key is missing or the type doesn't match
if (statService.TryGetStat<int>(GameStats.PopulationTotal, out var pop))
{
    // use pop
}

// Pre-formatted string (e.g. "42 %" for percent stats)
string display = statService.GetStatFormatted(GameStats.PopulationBeaverPercent);
```

Duplicate stat IDs across providers throw immediately at load — this is intentional. Unknown keys throw `KeyNotFoundException` on all methods except `TryGetStat`, which returns `false`.

`GameStatService` also exposes two provider-lookup methods:

- `GetProvider(string stat)` — returns the `IGameStatProvider` registered for that key; throws `KeyNotFoundException` if the key is unknown.
- `GetProvider<T>(string stat)` — same, but casts the provider to `T`; throws `KeyNotFoundException` on an unknown key and `InvalidCastException` if the provider does not implement `T`.

## Built-in stat keys

Use the constants on [`GameStats`](../../ModdableTimberborn/GameStats/GameStats.cs) instead of duplicating string literals where possible. The key column below shows the actual string value registered by the providers.

### Time values

Provided by [`TimeStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/TimeStatsProvider.cs).

| Constant | Key | Type | Description |
|---|---|---|---|
| `GameStats.TimePartialDay` | `GameDayHours` | `float` | Current day number including partial day progress |
| `GameStats.TimePartialCycleDay` | `GameCycleDayHours` | `float` | Current cycle day including partial day progress |
| `GameStats.TimeTodayHours` | `GameTodayHours24` | `float` | Hours passed today |
| `GameStats.TimeDayNumber` | `GameDayNumber` | `int` | Current game day number |
| `GameStats.TimeCycle` | `GameCycle` | `int` | Current cycle number |
| `GameStats.TimeCycleDay` | `GameCycleDay` | `int` | Current day within the cycle |
| `GameStats.TimeCycleDuration` | `GameCycleDuration` | `int` | Current cycle duration in days |
| `GameStats.TimeDayProgress` | `GameDayProgress` | `float` | Current day progress from 0–1 |
| `GameStats.TimeCycleProgress` | `GameCycleProgress` | `float` | Current cycle progress from 0–1 |

### Population counts (`int`)

Provided by [`GlobalPopulationStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/GlobalPopulationStatsProvider.cs).

| Constant | Key | Description |
|---|---|---|
| `GameStats.PopulationNumberOfAdult` | `NumberOfAdults` | Adult beavers |
| `GameStats.PopulationNumberOfChild` | `NumberOfChildren` | Beaver children |
| `GameStats.PopulationNumberOfBot` | `NumberOfBots` | Bots |
| `GameStats.PopulationNumberOfBeaver` | `NumberOfBeavers` | All beavers (adults + children) |
| `GameStats.PopulationNumberOfHealthyAdult` | `NumberOfHealthyAdults` | Healthy adult beavers |
| `GameStats.PopulationNumberOfHealthyChild` | `NumberOfHealthyChildren` | Healthy beaver children |
| `GameStats.PopulationTotal` | `TotalPopulation` | All beavers and bots combined |
| `GameStats.BedOccupied` | `OccupiedBeds` | Beds currently occupied |
| `GameStats.BedFree` | `FreeBeds` | Unoccupied beds |
| `GameStats.BedTotal` | `TotalBeds` | Occupied + free beds |
| `GameStats.BedHomeless` | `Homeless` | Beavers without a bed |
| `GameStats.ContaminationAdult` | `ContaminatedAdults` | Contaminated adult beavers |
| `GameStats.ContaminationChild` | `ContaminatedChildren` | Contaminated beaver children |
| `GameStats.ContaminationTotal` | `ContaminatedTotal` | All contaminated beavers |

#### Workforce counts (`int`)

Unprefixed keys return the combined beaver + bot total. Use the beaver or bot constants for per-type values.

| Total constant | Total key | Beaver constant | Beaver key | Bot constant | Bot key |
|---|---|---|---|---|---|
| `GameStats.WorkforceEmployable` | `Employable` | `GameStats.WorkforceBeaverEmployable` | `BeaversEmployable` | `GameStats.WorkforceBotEmployable` | `BotEmployable` |
| `GameStats.WorkforceUnemployable` | `Unemployable` | `GameStats.WorkforceBeaverUnemployable` | `BeaversUnemployable` | `GameStats.WorkforceBotUnemployable` | `BotUnemployable` |
| `GameStats.WorkforceTotal` | `Total` | `GameStats.WorkforceBeaverTotal` | `BeaversTotal` | `GameStats.WorkforceBotTotal` | `BotTotal` |

### Population ratios (`float`, formatted as percentage)

Provided by [`GlobalPopulationPercentStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/GlobalPopulationPercentStatsProvider.cs). All values are floats in the range 0–1; `GetStatFormatted` renders them as `"P0"` strings (e.g. `"42 %"`), or `"N/A"` for negative values.

| Constant | Key | Ratio |
|---|---|---|
| `GameStats.PopulationBeaverPercent` | `BeaverPercent` | Beavers as a share of total population |
| `GameStats.PopulationBotPercent` | `BotPercent` | Bots as a share of total population |
| `GameStats.PopulationAdultPercent` | `AdultPercent` | Adults as a share of beavers |
| `GameStats.PopulationChildPercent` | `ChildPercent` | Children as a share of beavers |
| `GameStats.BedHomelessPercent` | `HomelessPercent` | Homeless as a share of beavers |
| `GameStats.ContaminationAdultPercent` | `AdultContaminatedPercent` | Contaminated adults as a share of beavers |
| `GameStats.ContaminationChildPercent` | `ChildContaminatedPercent` | Contaminated children as a share of beavers |
| `GameStats.ContaminationPercent` | `ContaminatedPercent` | All contaminated as a share of beavers |

### Per-good stats

Good IDs match the Timberborn good identifier strings (e.g. `Wood`, `Water`, `Plank`).

#### Amounts and capacity (`int`)

Provided by [`GoodStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/GoodStatsProvider.cs).

| Helper | Key pattern | Description |
|---|---|---|
| `GameStats.GoodAmount(goodId)` | `GoodAmount.<goodId>` | Global available stock of the good |
| `GameStats.GoodCapacity(goodId)` | `GoodCapacity.<goodId>` | Global input/output storage capacity |

Example: `GameStats.GoodAmount("Wood")`, `GameStats.GoodCapacity("Water")`.

#### Fill rate (`float`, formatted as percentage)

Provided by [`GoodPercentStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/GoodPercentStatsProvider.cs).

| Helper | Key pattern | Description |
|---|---|---|
| `GameStats.GoodFill(goodId)` | `GoodFill.<goodId>` | Storage fill rate (0–1) |

Example: `GameStats.GoodFill("Plank")`.

The good-keyed stats are snapshotted at load time from `IGoodService.Goods`. Goods introduced after load will not be present.

## Providing a custom stat

Implement [`IGameStatProvider`](../../ModdableTimberborn/GameStats/IGameStatProvider.cs) (or one of its typed sub-interfaces) and register it with the DI container.

### Choosing an interface

| Interface | Value type | Notes |
|---|---|---|
| `IIntGameStatProvider` | `int` | |
| `IFloatGameStatProvider` | `float` | |
| `IStringGameStatProvider` | `string` | |
| `ISpriteGameStatProvider` | `Sprite?` | |
| `IPercentGameStatProvider` | `float` | `GetStatFormatted` renders as `"P0"` percentage, or `"N/A"` for negative values |
| `IGameStatProvider<T>` | any `T` | Use when none of the above fit |

### Implementing a provider

```cs
public class MyModStatsProvider : IIntGameStatProvider
{
    // All stat IDs this provider owns. Must be globally unique.
    public IEnumerable<string> AvailableStats => ["MyMod.SomeCount", "MyMod.OtherCount"];

    public int? GetStat(string statId) => statId switch
    {
        "MyMod.SomeCount"  => ComputeSomeCount(),
        "MyMod.OtherCount" => ComputeOtherCount(),
        _ => throw new ArgumentOutOfRangeException(nameof(statId)),
    };
}
```

Use a unique prefix (e.g. your mod name) to avoid collisions with other providers. A duplicate key detected at load throws immediately.

### Registering the provider

Auto-discovery via `GameStatsConfig` only scans the `ModdableTimberborn` assembly itself. Providers in your own assembly must be registered manually:

```cs
[Context("Game")]
public class MyGameConfig : Configurator
{
    public override void Configure()
    {
        MultiBind<IGameStatProvider>().To<MyModStatsProvider>().AsSingleton();
    }
}
```
