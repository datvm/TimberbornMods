# Game Stats

Game Stats is a string-keyed registry of global game statistics. It decouples consumers — UI, condition checks, dynamic text — from Timberborn's individual data services. Any stat can be queried by name without a direct code dependency on the provider that supplies it.

## Built-in Stat Quick Reference

Use these stat IDs with `GameStatService` or any system that reads game stats.

| Stat ID | Type | Description |
|---|---|---|
| **Time** | | Current game time and cycle progress. |
| `TimePartialDay` | `float` | Current day number including partial day progress. |
| `TimePartialCycleDay` | `float` | Current cycle day including partial day progress. |
| `TimeTodayHours` | `float` | Hours passed today. |
| `TimeDayNumber` | `int` | Current game day number. |
| `TimeCycle` | `int` | Current cycle number. |
| `TimeCycleDay` | `int` | Current day within the cycle. |
| `TimeCycleDuration` | `int` | Current cycle duration in days. |
| `TimeDayProgress` | `percent` | Current day progress from 0 to 1. |
| `TimeCycleProgress` | `percent` | Current cycle progress from 0 to 1. |
| **Weather** | | Current weather and warning information. |
| `WeatherCurrent` | `string` | Current weather ID. |
| `WeatherWarningStage` | `string` | Current weather warning stage. |
| `WeatherWarningNextWeather` | `string` | Weather ID for the next warned hazardous weather. |
| `WeatherWarningDaysToHazardous` | `float` | Days until the warned hazardous weather, or `-1` when unavailable. |
| `WeatherCurrentDuration` | `int` | Duration of the current weather stage in days. |
| `WeatherCurrentBenignDuration` | `int` | Duration of the current cycle's benign weather stage in days. |
| `WeatherCurrentHazardousDuration` | `int` | Duration of the current cycle's hazardous weather stage in days, or `0` when none is scheduled. |
| **Population** | | Population of beavers and bots. |
| `PopulationNumberOfAdult` | `int` | Number of adult beavers. |
| `PopulationNumberOfChild` | `int` | Number of child beavers. |
| `PopulationNumberOfBot` | `int` | Number of bots. |
| `PopulationNumberOfBeaver` | `int` | Number of all beavers, adult and child. |
| `PopulationNumberOfHealthyAdult` | `int` | Number of healthy adult beavers. |
| `PopulationNumberOfHealthyChild` | `int` | Number of healthy child beavers. |
| `PopulationTotal` | `int` | Total number of beavers and bots. |
| `PopulationBeaverPercent` | `percent` | Beavers as a share of total population. |
| `PopulationBotPercent` | `percent` | Bots as a share of total population. |
| `PopulationAdultPercent` | `percent` | Adults as a share of beavers. |
| `PopulationChildPercent` | `percent` | Children as a share of beavers. |
| **Beds** | | Settlement bed availability and homelessness. |
| `BedOccupied` | `int` | Number of occupied beds. |
| `BedFree` | `int` | Number of free beds. |
| `BedHomeless` | `int` | Number of beavers without a bed. |
| `BedTotal` | `int` | Number of occupied and free beds. |
| `BedHomelessPercent` | `percent` | Homeless beavers as a share of beavers. |
| `BedHousedPercent` | `percent` | Housed beavers as a share of beavers. |
| **Workforce** | | Employable and unemployable workers. |
| `WorkforceEmployable` | `int` | Total employable beavers and bots. |
| `WorkforceUnemployable` | `int` | Total unemployable beavers and bots. |
| `WorkforceTotal` | `int` | Total beaver and bot workforce. |
| `WorkforceBeaverEmployable` | `int` | Employable beavers. |
| `WorkforceBeaverUnemployable` | `int` | Unemployable beavers. |
| `WorkforceBeaverTotal` | `int` | Total beaver workforce. |
| `WorkforceBotEmployable` | `int` | Employable bots. |
| `WorkforceBotUnemployable` | `int` | Unemployable bots. |
| `WorkforceBotTotal` | `int` | Total bot workforce. |
| **Contamination** | | Contaminated beaver counts and ratios. |
| `ContaminationAdult` | `int` | Number of contaminated adult beavers. |
| `ContaminationChild` | `int` | Number of contaminated child beavers. |
| `ContaminationTotal` | `int` | Number of all contaminated beavers. |
| `ContaminationAdultPercent` | `percent` | Contaminated adults as a share of beavers. |
| `ContaminationChildPercent` | `percent` | Contaminated children as a share of beavers. |
| `ContaminationPercent` | `percent` | All contaminated beavers as a share of beavers. |
| **Map** | | Current map identity and dimensions. |
| `MapName` | `string` | Current map name, when available. |
| `MapSizeX` | `int` | Map size on the X axis. |
| `MapSizeY` | `int` | Map size on the Y axis. |
| `MapSizeTerrainZ` | `int` | Terrain height. |
| `MapSizeTotalZ` | `int` | Total map height. |
| **Goods** | | Per-good stock, capacity, and fill. Replace `<goodId>` with a Timberborn good ID such as `Water` or `Plank`. |
| `Science` | `int` | Current science points. |
| `GoodAmount.<goodId>` | `int` | Global available stock of the good. |
| `GoodAmount.Science` | `int` | Current science points. Prefer `Science` for new usages. |
| `GoodCapacity.<goodId>` | `int` | Global input/output storage capacity for the good. |
| `GoodFill.<goodId>` | `percent` | Storage fill rate from 0 to 1. |

> [!INFO]  
> A `percent` stat is stored as a `float` from 0 to 1 internally. When formatted as text, it is automatically displayed as a percentage.

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



