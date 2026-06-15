# Areas

The Areas subsystem answers the question: _which characters are inside this zone right now?_ Timberborn has no built-in spatial query for runtime entity position, so this subsystem fills that gap. You define one or more rectangular volumes (`BoundsInt`), register them with a service, and get back a live handle that maintains an up-to-date set of matching characters and fires enter/exit events as they cross the boundary.

Performance is kept in check by a **segment grid**: the map is partitioned into 26×26-tile squares on load. Each registered area is mapped to the segments it overlaps; on every tile-boundary crossing only the handles whose segments match the character's old or new position are evaluated — avoiding a full O(handles × characters) scan per tick.

## Enabling the subsystem

Call `UseAreaApis()` from your `IModStarter` before the game loads:

```cs
public class MStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .UseAreaApis();
    }
}
```

`UseAreaApis()` is idempotent. It also calls `UseEntityTracker()` internally, so you do not need to enable the Entity Tracker separately when using Areas.

## Core types

| Type | Role |
|---|---|
| [`AreaCondition`](../../ModdableTimberborn/Areas/AreaCondition.cs) | Enum: `Intersects` or `Contains`. Used by `AreaExtensions.Evaluate`. |
| [`CharacterAreaTrackerRegistration`](../../ModdableTimberborn/Areas/AreaTrackerRegistration.cs) | Data bag describing the zone(s) and character filter for a single tracking request. |
| [`CharacterAreaTrackerHandle`](../../ModdableTimberborn/Areas/CharacterAreaTrackerHandle.cs) | Live handle returned by the service. Exposes the `Entities` set and enter/exit events. |
| [`CharacterAreaTrackerService`](../../ModdableTimberborn/Areas/CharacterAreaTrackerService.cs) | Singleton service. Register and unregister handles here. |
| [`AreaSegmentService`](../../ModdableTimberborn/Areas/AreaSegmentService.cs) | Singleton spatial index. Used internally; rarely needed directly. |
| [`SerializableBoundsInts`](../../ModdableTimberborn/Areas/SerializableBounds.cs) | JSON-safe wrapper for `BoundsInt`, with implicit casts. Useful when storing zone definitions in spec files. |

### `CharacterAreaTrackerRegistration`

```cs
public class CharacterAreaTrackerRegistration : AreaTrackerRegistration
{
    public BoundsInt[] Areas { get; set; } = [];
    public CharacterType CharacterTypes { get; set; } = CharacterType.All;
}
```

- **`Areas`** — One or more rectangular volumes. At least one is required; the handle throws `ArgumentException` otherwise.
- **`CharacterTypes`** — Flags controlling which character types are tracked. Defaults to `CharacterType.All`. `CharacterType.Unknown` is not valid and will throw.

  Valid `CharacterType` flag values:

  | Value | Members |
  |---|---|
  | `Bot` | Bots only |
  | `AdultBeaver` | Adult beavers only |
  | `ChildBeaver` | Child beavers only |
  | `Beavers` | `AdultBeaver \| ChildBeaver` |
  | `Workers` | `AdultBeaver \| Bot` |
  | `All` | `Beavers \| Bot` (every tracked type) |

> **Immutability:** `Areas` and `Segments` are computed once in the handle constructor and are read-only. To change a zone, call `UnregisterArea` on the existing handle and `RegisterArea` a new registration.

### `CharacterAreaTrackerHandle`

Returned by `CharacterAreaTrackerService.RegisterArea`. Inherits from `AreaTrackerHandle<CharacterPositionTracker, CharacterAreaTrackerRegistration>`.

```cs
// Live set of characters currently inside the zone(s):
IReadOnlyCollection<CharacterPositionTracker> Entities { get; }

int Count { get; }
bool HasAny { get; }

// Fired when a character enters any of the areas:
event EventHandler<CharacterPositionTracker> OnEntityEntered;

// Fired when a character leaves all of the areas:
event EventHandler<CharacterPositionTracker> OnEntityExited;
```

## Worked example

This example tracks all beavers inside a single building footprint and logs each arrival and departure.

```cs
public class ZoneWatcher : ILoadableSingleton, IUnloadableSingleton
{
    readonly CharacterAreaTrackerService _areaService;
    CharacterAreaTrackerHandle? _handle;

    public ZoneWatcher(CharacterAreaTrackerService areaService)
    {
        _areaService = areaService;
    }

    public void Load()
    {
        // Define the zone — for example, a 4×4 footprint at tile (10, 5, 0):
        var zone = new BoundsInt(new Vector3Int(10, 5, 0), new Vector3Int(4, 4, 1));

        var registration = new CharacterAreaTrackerRegistration
        {
            Areas          = [zone],
            CharacterTypes = CharacterType.Beavers,  // beavers only (AdultBeaver | ChildBeaver)
        };

        _handle = _areaService.RegisterArea(registration);
        _handle.OnEntityEntered += OnEntered;
        _handle.OnEntityExited  += OnExited;
    }

    public void Unload()
    {
        if (_handle is null) { return; }

        _handle.OnEntityEntered -= OnEntered;
        _handle.OnEntityExited  -= OnExited;
        _areaService.UnregisterArea(_handle);
        _handle = null;
    }

    void OnEntered(object sender, CharacterPositionTracker tracker)
        => Plugin.Log.LogInfo($"{tracker.Character.name} entered the zone.");

    void OnExited(object sender, CharacterPositionTracker tracker)
        => Plugin.Log.LogInfo($"{tracker.Character.name} left the zone.");
}
```

### Polling instead of events

If you only need a snapshot rather than per-crossing callbacks, skip the event subscriptions and read `_handle.Entities` (or `_handle.Count` / `_handle.HasAny`) directly:

```cs
int beaversPresent = _handle.Count;

foreach (var tracker in _handle.Entities)
{
    // tracker.Character, tracker.Cell, tracker.Position, etc.
}
```

### Multiple zones

Pass more than one `BoundsInt` in `Areas` to track a union of rectangles with a single handle. A character is considered inside if it is in **any** of the specified areas:

```cs
var registration = new CharacterAreaTrackerRegistration
{
    Areas = [zoneA, zoneB, zoneC],
    CharacterTypes = CharacterType.All,
};
```

### Serializing zone definitions

Use [`SerializableBoundsInts`](../../ModdableTimberborn/Areas/SerializableBounds.cs) to store `BoundsInt` values in JSON-backed specs. The struct has implicit casts in both directions:

```cs
SerializableBoundsInts stored = myBoundsInt;   // implicit cast to serializable form
BoundsInt restored = stored;                   // implicit cast back
```

## How it works at runtime

1. `UseAreaApis()` attaches a [`CharacterPositionTracker`](../../ModdableTimberborn/Areas/CharacterPositionTracker.cs) `TickableComponent` decorator to every `Character` entity. Each tick it compares `transform.position` to the previous frame, updates `Cell` (floor-to-int) and `Segment`, and fires `OnCellChanged` when a tile boundary is crossed.
2. [`CharacterAreaTrackerService`](../../ModdableTimberborn/Areas/CharacterAreaTrackerService.cs) subscribes to `OnCellChanged` lazily — only for character types that have at least one registered handle — and unsubscribes when the last handle for a type is removed.
3. On each cell-change event, the service uses the character's old and new segment indices to look up only the handles that overlap those segments, then evaluates `BoundsInt.Contains(cell)` for each candidate.
4. The handle maintains its `Entities` set through a `DeferredHashSet` (allowing mutation during event callbacks) and fires `OnEntityEntered` / `OnEntityExited` when membership changes.

## Block-object area tracking — not yet available

[`BlockObjectAreaTrackerRegistration`](../../ModdableTimberborn/Areas/AreaTrackerRegistration.cs) and [`BlockObjectBound`](../../ModdableTimberborn/Areas/BlockObjectBound.cs) are defined and `BlockObjectBound` is attached as a decorator on every `BlockObject` at load time, but there is no `BlockObjectAreaTrackerHandle` or corresponding service yet. The block-object tracking path is **scaffolded but incomplete** and cannot be used in the current release. Character area tracking is the only fully supported path.
