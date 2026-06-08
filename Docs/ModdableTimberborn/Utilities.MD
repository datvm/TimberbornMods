# Utilities

ModdableTimberborn ships several small, self-contained subsystems that are useful across many mod types. This page covers four of them: JSON serialization helpers for Timberborn's property-bag type, optional-mod DLL loading, a weather-API compatibility layer, and a pair of injectable services for day-scheduled actions and coordinated destruction.

---

## JSON Serialization

Timberborn's [`SerializedObject`](../../ModdableTimberborn/SerializationSystem/ModdableTimberbornSerializedObjectExtensions.cs) is a structured property bag that standard Newtonsoft.Json converters cannot handle out of the box. The serialization subsystem bridges that gap with two types.

**[`SerializedObjectJsonConverter`](../../ModdableTimberborn/SerializationSystem/SerializedObjectJsonConverter.cs)** is a singleton `JsonConverter<SerializedObject>` that recursively serializes and deserializes `SerializedObject` trees to and from JSON. It supports nested objects, arrays, and the primitive types `int`, `float`, `bool`, `string`, and `null`. Unsupported token types throw `JsonSerializationException` — there is no silent fallback.

**[`ModdableTimberbornSerializationExtensions`](../../ModdableTimberborn/SerializationSystem/ModdableTimberbornSerializedObjectExtensions.cs)** (namespace `Timberborn.SerializationSystem`) exposes three convenience helpers that pre-wire the converter:

| Member | Kind | Description |
|---|---|---|
| `DeserializeSerializedObject(string json)` | static method | Deserializes a JSON string into a `SerializedObject`. |
| `obj.SerializeToJson()` | extension | Serializes a `SerializedObject` to a JSON string. |
| `obj.DeserializeTo<T>()` | extension | Round-trips a `SerializedObject` through JSON into a strongly-typed `T`. |

Because the extensions live in the `Timberborn.SerializationSystem` namespace — the same namespace as `SerializedObject` itself — no additional `using` directive is needed at the call site.

### Example

```cs
// Serialize a SerializedObject received from Timberborn's pipeline
string json = mySerializedObject.SerializeToJson();

// Reconstruct from a stored string
SerializedObject? restored = ModdableTimberbornSerializationExtensions
    .DeserializeSerializedObject(json);

// Map directly into a typed record
MyBuildingData data = mySerializedObject.DeserializeTo<MyBuildingData>();
```

> `DeserializeTo<T>()` performs a serialize-then-deserialize round-trip internally. Custom `JsonSerializerSettings` passed to it apply only to the final deserialization step, not to the `SerializedObject` read — the converter is always active on the first pass. **Caution:** caller-supplied settings do not include `SerializedObjectJsonConverter`, so any nested `SerializedObject` fields inside `T` will not receive converter handling during that deserialization pass.

---

## Optional Mods

Sometimes a mod provides extra integration with a second mod, but should not hard-reference that second mod's assembly — doing so would cause load failures for users who don't have it installed. The optional-mod system solves this by loading additional DLLs at runtime only when their target companion mod is enabled.

### File naming convention

Place the integration DLL in your mod's directory (any subfolder is fine) and name it:

```
<anything>.dll.<targetModId>.optional
```

Where `<targetModId>` is the exact mod ID of the companion mod that must be enabled for this DLL to load. For example, if your mod's base name is `MyMod` and the companion mod has ID `SomeCoolMod`:

```
MyMod.dll.SomeCoolMod.optional
```

At startup, `OptionalModsLoader` scans every enabled mod's directory tree for files matching `*.dll.*.optional`. For each file it extracts the `<targetModId>` segment and checks whether that mod is currently enabled. If it is, the DLL bytes are read from disk and loaded via `Assembly.Load(bytes)`; if not, the file is verbose-logged then skipped.

### Implementing the entry point

The loaded DLL must contain at least one concrete `IModStarter` implementation. `OptionalModsLoader` reflects over all exported types, finds concrete `IModStarter` classes, and calls `StartMod` on each one — exactly the same lifecycle as a regular Timberborn mod.

```cs
// Inside your optional DLL:
public class MySomeCoolModIntegrationStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        // Register Bindito configurators, apply Harmony patches, etc.
        new Harmony("MyMod.SomeCoolMod").PatchAll();
    }
}
```

No changes to the host mod's configurator or Harmony setup are required — `OptionalModsLoader` handles discovery automatically.

> The `.optional` extension is part of the full file name, not a separate file extension known to the OS. Ensure your build output preserves the full name; a post-build copy step is the most reliable approach.

---

## Weather Compatibility

Timberborn's internal weather API (`WeatherService`, `HazardousWeatherService`, etc.) may change between game updates. The `CompatWeatherService` subsystem provides a stable abstraction so mods never depend on those types directly.

### How it works

[`CompatWeatherService`](../../ModdableTimberborn/CompatWeatherService/CompatWeatherService.cs) is a `[BindSingleton]` that collects all registered [`ICompatWeatherServiceProvider`](../../ModdableTimberborn/CompatWeatherService/ICompatWeatherServiceProvider.cs) implementations at construction time, picks the one with the highest `Priority`, and exposes it as the `Provider` property. A missing provider causes a hard throw at startup.

[`DefaultCompatWeatherServiceProvider`](../../ModdableTimberborn/CompatWeatherService/DefaultCompatWeatherServiceProvider.cs) wraps the vanilla game services and registers itself with priority `-1`. Any mod-supplied provider with priority `0` or above takes over entirely — there is no fallback chaining.

### `ICompatWeatherServiceProvider` at a glance

```cs
public interface ICompatWeatherServiceProvider
{
    IReadOnlyList<CompatWeatherType> WeatherTypes { get; }
    int Priority { get; }
    int ApproachingNotificationDays { get; }
    string? CurrentCycleHazardousId { get; }
    bool IsHazardous { get; }
    bool IsNextDayHazardous { get; }

    CompatWeatherCycle GetCurrentCycle();
    CompatWeatherCycleStage GetCurrentCycleStage();
    CompatWeatherCycleStage? GetCurrentCycleBenignStage();
    CompatWeatherCycleStage? GetCurrentCycleHazardousStage();
    CompatNextWeatherCycleStage GetNextCycleStage();
    CompatWeatherWarning GetWarningStatus();
}
```

The built-in weather IDs are available as constants on `CompatWeatherService`: `TemperateId`, `DroughtId`, `BadtideId`.

### Consuming the service

Inject `CompatWeatherService` and read from `Provider`:

```cs
[BindSingleton]
public class MyWeatherReactor(CompatWeatherService compatWeather) : ILoadableSingleton
{
    public void Load()
    {
        var provider = compatWeather.Provider;

        if (provider.IsHazardous)
        {
            string weatherId = provider.CurrentCycleHazardousId!;
            // React to active hazard...
        }

        CompatWeatherWarning warning = provider.GetWarningStatus();
        if (warning.Stage == CompatWeatherWarningStage.Showing)
        {
            float daysLeft = warning.DaysToHazardous!.Value;
            // Show approaching-hazard UI...
        }
    }
}
```

### Supplying a custom provider

If your mod introduces an entirely different weather system, register an `ICompatWeatherServiceProvider` with a priority above `-1`:

```cs
[MultiBind(typeof(ICompatWeatherServiceProvider))]
public class MyWeatherProvider : ICompatWeatherServiceProvider
{
    public int Priority => 10; // beats the default
    // ... implement remaining members
}
```

All existing consumers automatically use your provider without any changes on their end.

---

## Services

### DayTimerService

[`DayTimerService`](../../ModdableTimberborn/Services/DayTimerService.cs) is a `[BindSingleton]` that lets you schedule an `Action` to fire at a precise in-game hour offset after each day starts. It listens on the `EventBus` for `CycleDayStartedEvent` and uses `ITimeTriggerFactory` to fire each bucket at the correct fractional-day offset — callbacks do not run at midnight unless you ask for delay `0`.

**Public API:**

```cs
// Fire every day, 0.25 in-game hours after midnight (default delay)
DayTimerReference Register(Action action, float delayed = 0.25f)

// Fire once on the next day, then auto-remove
DayTimerReference RegisterOnce(Action action, float delayed = 0.25f)

// Overload with explicit repeat flag
DayTimerReference Register(Action action, float delayed, bool repeat)

// Cancel a previously registered timer
void Unregister(DayTimerReference r)
```

`DayTimerReference` is a plain `record(float DelayedHours, Action Action, bool Repeat)`. Hold on to the returned reference and call `Unregister` when your component is torn down.

**Example:**

```cs
[BindSingleton]
public class MyDailyTask(DayTimerService dayTimer) : ILoadableSingleton, IUnloadableSingleton
{
    DayTimerReference _timer = null!;

    public void Load()
    {
        // Run every in-game day, 6 hours after midnight
        _timer = dayTimer.Register(OnEachDay, delayed: 6f);
    }

    public void Unload()
    {
        dayTimer.Unregister(_timer);
    }

    void OnEachDay()
    {
        // ...
    }
}
```

---

### DestructionService

[`DestructionService`](../../ModdableTimberborn/Services/DestructionService.cs) is a `[BindSingleton]` (excluded from the menu scene via `BindAttributeContext.NonMenu`) that manages the full lifecycle of block-object and terrain destruction: querying the affected set, highlighting it in the UI, and executing deletion.

The API follows a three-step pipeline:

| Step | Method | Description |
|---|---|---|
| Query | `QueryDestructingEntities(IEnumerable<Vector3Int> terrains)` | Returns a `DestroyingEntities` snapshot of all block objects and terrain tiles that would be removed, including physics-dependent objects above the given tiles. |
| Query | `QueryDestructingEntities(IEnumerable<BlockObject> blockObjects)` | Same, but starting from block objects instead of terrain. |
| Highlight | `HighlightDestructionEntities(DestroyingEntities entities)` | Highlights the set in the game's negative (red) color. |
| Highlight | `HighlightDestructionEntities(DestroyingEntities entities, Color color)` | Highlights in a specific color. |
| Unhighlight | `UnhighlightDestructionEntities()` | Removes all destruction highlights (call on cancel). |
| Destroy | `DestroyEntities(DestroyingEntities entities)` | Deletes all block objects via `EntityService` and all terrain tiles via `TerrainDestroyer`. |

`DestroyingEntities` is a `readonly record struct` carrying `ImmutableArray<BlockObject> BlockObjects` and `ImmutableArray<Vector3Int> Terrains`.

**Example:**

```cs
// In a tool that destroys whatever the cursor is over:
var terrains = GetSelectedTerrainCoords(); // your logic

DestroyingEntities affected = destructionService.QueryDestructingEntities(terrains);
destructionService.HighlightDestructionEntities(affected);

// On confirm:
destructionService.UnhighlightDestructionEntities();
destructionService.DestroyEntities(affected);

// On cancel:
destructionService.UnhighlightDestructionEntities();
```

> Do not reuse a `DestroyingEntities` value after the selection changes — the `ImmutableArray` snapshots it holds are stable and safe to keep across frames, but they reflect the selection at query time. Start a fresh query whenever the selection changes to get an up-to-date set.

---

### ContainerRetriever

[`ContainerRetriever`](../../ModdableTimberborn/Services/ContainerRetriever.cs) exposes static access to the Bindito `IContainer` for use in static helpers or Harmony patches where constructor injection is not available. It is **not** registered through DI — a Harmony postfix on `ContainerCreator.CreateContainer` constructs it once the container exists (see [`ContainerRetrieverPatches`](../../ModdableTimberborn/Patches/ContainerRetrieverPatches.cs)). It holds a `WeakReference<IContainer>` and exposes the container through static accessors.

**Public surface:**

| Member | Description |
|---|---|
| `ContainerRetriever.Instance` | Returns the current retriever, or throws `InvalidOperationException` if the container has been collected. |
| `ContainerRetriever.InstanceOrNull` | Same, but returns `null` instead of throwing. |
| `ContainerRetriever.GetInstance<T>()` | Resolves an instance of `T` from the container. |
| `ContainerRetriever.GetInstance(Type)` | Non-generic overload. |
| `ContainerRetriever.GetInstances<T>()` | Resolves all bound instances of `T`. |
| `ContainerRetriever.GetInstances(Type)` | Non-generic overload. |

**Example (Harmony patch context):**

```cs
[HarmonyPatch(typeof(SomeVanillaClass), nameof(SomeVanillaClass.SomeMethod))]
static class SomeVanillaPatch
{
    static void Postfix()
    {
        var myService = ContainerRetriever.GetInstance<MyService>();
        myService.DoSomething();
    }
}
```

> Prefer constructor injection wherever possible — `ContainerRetriever` is a last resort for code paths the DI container cannot reach.
