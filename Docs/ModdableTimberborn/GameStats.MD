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
object? raw = statService.GetStat("TotalPopulation");

// Typed — throws InvalidCastException on type mismatch
int pop = statService.GetStat<int>("TotalPopulation");

// Safe typed — returns false if the key is missing or the type doesn't match
if (statService.TryGetStat<int>("TotalPopulation", out var pop))
{
    // use pop
}

// Pre-formatted string (e.g. "42 %" for percent stats)
string display = statService.GetStatFormatted("BeaverPercent");
```

Duplicate stat IDs across providers throw immediately at load — this is intentional. Unknown keys throw `KeyNotFoundException` on all methods except `TryGetStat`, which returns `false`.

`GameStatService` also exposes two provider-lookup methods:

- `GetProvider(string stat)` — returns the `IGameStatProvider` registered for that key; throws `KeyNotFoundException` if the key is unknown.
- `GetProvider<T>(string stat)` — same, but casts the provider to `T`; throws `KeyNotFoundException` on an unknown key and `InvalidCastException` if the provider does not implement `T`.

## Built-in stat keys

### Population counts (`int`)

Provided by [`GlobalPopulationStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/GlobalPopulationStatsProvider.cs).

| Key | Description |
|---|---|
| `NumberOfAdults` | Adult beavers |
| `NumberOfChildren` | Beaver children |
| `NumberOfBots` | Bots |
| `NumberOfBeavers` | All beavers (adults + children) |
| `NumberOfHealthyAdults` | Healthy adult beavers |
| `NumberOfHealthyChildren` | Healthy beaver children |
| `TotalPopulation` | All beavers and bots combined |
| `OccupiedBeds` | Beds currently occupied |
| `FreeBeds` | Unoccupied beds |
| `TotalBeds` | Occupied + free beds |
| `Homeless` | Beavers without a bed |
| `ContaminatedAdults` | Contaminated adult beavers |
| `ContaminatedChildren` | Contaminated beaver children |
| `ContaminatedTotal` | All contaminated beavers |

#### Workforce counts (`int`)

Unprefixed keys return the combined beaver + bot total. Prefix with `Beavers` or `Bot` for per-type values.

| Key | `Beavers` variant | `Bot` variant |
|---|---|---|
| `Employable` | `BeaversEmployable` | `BotEmployable` |
| `Unemployable` | `BeaversUnemployable` | `BotUnemployable` |
| `Total` | `BeaversTotal` | `BotTotal` |

### Population ratios (`float`, formatted as percentage)

Provided by [`GlobalPopulationPercentStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/GlobalPopulationPercentStatsProvider.cs). All values are floats in the range 0–1; `GetStatFormatted` renders them as `"P0"` strings (e.g. `"42 %"`), or `"N/A"` for negative values.

| Key | Ratio |
|---|---|
| `BeaverPercent` | Beavers as a share of total population |
| `BotPercent` | Bots as a share of total population |
| `AdultPercent` | Adults as a share of beavers |
| `ChildPercent` | Children as a share of beavers |
| `HomelessPercent` | Homeless as a share of beavers |
| `AdultContaminatedPercent` | Contaminated adults as a share of beavers |
| `ChildContaminatedPercent` | Contaminated children as a share of beavers |
| `ContaminatedPercent` | All contaminated as a share of beavers |

### Per-good stats

Good IDs match the Timberborn good identifier strings (e.g. `Wood`, `Water`, `Plank`).

#### Amounts and capacity (`int`)

Provided by [`GoodStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/GoodStatsProvider.cs).

| Key pattern | Description |
|---|---|
| `GoodAmount.<goodId>` | Global available stock of the good |
| `GoodCapacity.<goodId>` | Global input/output storage capacity |

Example: `"GoodAmount.Wood"`, `"GoodCapacity.Water"`.

#### Fill rate (`float`, formatted as percentage)

Provided by [`GoodPercentStatsProvider`](../../ModdableTimberborn/GameStats/Implementations/GoodPercentStatsProvider.cs).

| Key pattern | Description |
|---|---|
| `GoodFill.<goodId>` | Storage fill rate (0–1) |

Example: `"GoodFill.Plank"`.

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
