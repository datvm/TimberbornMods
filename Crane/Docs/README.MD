# Crane

**Crane** adds a District Management building: a stationary builder. One worker stays inside the crane and operates it. From that seat they can build, demolish, and gather rubble in a column around the mast, without walking out to each site.

Other mods can give the crane extra work by implementing `ICraneJob` and registering an `ICraneJobProvider`.

Required dependency: [Moddable Timberborn](../../ModdableTimberborn/Docs/README.MD).

---

## What the building does

### Placement and workers

The **Crane** is a 1×1 workplace in the District Management group.

| | |
|---|---|
| Cost | 8 logs, 4 planks, 4 gears, 300 science |
| Workers | 1 (beaver by default; bots unlock for 500 science) |
| Work speed | +33% on top of the worker's own speed |

Selecting a finished crane enters construction mode, so you can place nearby buildings without leaving the panel.

### Range

The crane works in a **3×3** column centered on itself (one tile in each horizontal direction). Vertically it reaches from the ground up to **one tile above the top of the mast**.

Taller masts therefore reach higher buildings, not a wider footprint.

### Crane Sections

Use **Build higher** on the crane panel to stack a **Crane Section** on top. Each finished section raises the mast by one tile and expands the working height.

| | |
|---|---|
| Section cost | 4 planks, 1 gear |
| Science | 0 |
| Refund | Fully refunded when destroyed |

Newly placed sections start at Very High builder priority so the crane tends to extend itself first. A section panel has a **Select Crane** button that jumps back to the base.

The section tool itself is a hidden/dev tool. Players are meant to place sections from the crane, not from the bottom bar.

### Work the crane can do

A finished, unpaused crane that is not marked for demolition claims every matching job in range and shows them in a **job queue**. The worker always progresses the first job that is ready, in this order:

1. Building this crane's own unfinished sections.
2. Higher builder priority first.
3. Material jobs (construction-like) before other jobs at the same priority.

Built-in job types:

| Job | What happens |
|---|---|
| **Build** | Delivers remaining construction materials, then hammers the site. |
| **Demolish** | Progresses demolition of marked buildings (not the crane itself, and not crane sections). |
| **Gather** | Picks up rubble (recovered good stacks) into the crane inventory, then dumps those goods onto nearby material jobs. |

A construction site that is still waiting for materials stays in the queue. The worker skips it until the site has enough stock to resume, and will do demolish/gather work in the meantime.

The crane does not work on itself.

### Materials

The crane has an inventory that haulers can fill. Allowed goods are whatever nearby **material jobs** still need. When stock arrives, the crane immediately dumps it onto those jobs in queue order.

Gathered rubble uses the same inventory: goods come off the pile, then flush to construction sites that want them. Extra stock can be taken back out.

Marking the crane for emptying pauses crane work until the inventory is clear.

### Pause and demolition

Pausing the crane, or marking it for demolition, drops its claimed jobs. They return when the crane is eligible again.

---

## Extending Crane

Other mods add work by doing three things:

1. Put an `ICraneJob` on the entity that should be worked.
2. Register an `ICraneJobProvider` so the crane can find that job.
3. Notify the provider when the job appears or goes away.

The built-in build / demolish / gather jobs follow this same path. You do not need to patch `CraneComponent` or `CraneWorkshop`.

### Project setup

Depend on Crane in `manifest.json`:

```json
{
  "RequiredMods": [
    { "Id": "Crane" }
  ]
}
```

Reference the installed `Crane.dll` (same pattern as other companion mods in this repo):

```xml
<PropertyGroup>
  <CranePath>$(GameModsFolder)Crane\$(TimberbornVersion)\**\*.dll</CranePath>
</PropertyGroup>
<ItemGroup>
  <Reference Include="$(CranePath)" Private="False" />
</ItemGroup>
```

Useful namespaces:

- `Crane.Jobs` — `ICraneJob`, `ICraneJobProvider`, `IMaterialCraneJob`
- `Crane.Components` — `CraneComponent`
- `Crane.Models` — `CraneTower` (`WorkingBounds`, `Jobs`)

Your configurator should inherit from a TimberUi / Moddable Timberborn attribute configurator so `[MultiBind]` and `[AddTemplateModule2]` are discovered.

### How claiming works

`CraneClaimService` listens to every `ICraneJobProvider`.

- On a full refresh (tower height changed, crane paused/unpaused, crane marked/unmarked for demolition) it calls `GetJobForCrane` on every provider and rebuilds that crane's queue.
- When a provider raises `OnPossibleNewJob`, every eligible crane that returns true from `job.IsForCrane` adds the job.
- When a provider raises `OnJobRemoved`, the job is removed from every crane that had it.

A crane is eligible only when it is **finished**, **not paused**, and **not marked for demolition**.

Both paths matter. If you only raise `OnPossibleNewJob` and `GetJobForCrane` returns nothing, the job will vanish the next time the mast height changes.

### `ICraneJob`

```cs
public interface ICraneJob : IPrioritizable
{
    bool IsForCrane(CraneComponent crane);
    bool IsAvailable { get; }
    float Progress { get; }
    event EventHandler? AvailabilityChanged;
    event EventHandler<PriorityChangedEventArgs>? PriorityChanged;
    void ProgressJob(CraneComponent crane, float hours);
    string JobNameLoc { get; }
}
```

`IPrioritizable` is Timberborn's builder-priority contract: `Priority Priority` and `SetPriority(Priority priority)`.

| Member | Contract |
|---|---|
| `IsForCrane` | Whether this job belongs on that crane's queue. Usually a range check against `crane.Tower.WorkingBounds`. |
| `IsAvailable` | Whether the worker may spend time on this job right now. The queue still lists unavailable jobs, in red. |
| `Progress` | `0`–`1`, shown as a percent in the queue. |
| `AvailabilityChanged` | Raise when `IsAvailable` changes. The worker polls availability each tick; raise this for consistency. |
| `PriorityChanged` | Raise when builder priority changes so the crane can re-sort its queue. |
| `ProgressJob` | Advance the job. `hours` already includes the worker's speed multiplier and the crane's +33% bonus. |
| `JobNameLoc` | Localization key for the short action label (`Build`, `Demolish`, `Gather`, or your own). |
| `SetPriority` | The queue's priority toggles call this. Forward it to a `BuilderPrioritizable` when the entity has one. |

The job queue UI casts `ICraneJob` to `BaseComponent` to show the entity name, icon, hover highlight, and click-to-select. Implement the job as a component on the target entity.

`IsAvailable` is the worker gate. If it is `true` but `ProgressJob` is a no-op, the worker still occupies the crane on that job and will not move on. Return `false` until the job can actually make progress.

Built-in construction sites are a special case: they stay visually available while waiting for materials, and `CraneWorkshop` skips them until they can hammer. Custom jobs do not get that skip unless `IsAvailable` is `false`.

### `ICraneJobProvider`

```cs
public interface ICraneJobProvider
{
    IEnumerable<ICraneJob> GetJobForCrane(CraneComponent crane);
    event EventHandler<ICraneJob>? OnPossibleNewJob;
    event EventHandler<ICraneJob>? OnJobRemoved;
}
```

Register it with TimberUi's multibind, and bind the concrete type as well so job components can inject it:

```cs
[MultiBind(typeof(ICraneJobProvider), AlsoBindSelf = true)]
public class MyCraneJobProvider : ICraneJobProvider
{
    public event EventHandler<ICraneJob>? OnPossibleNewJob;
    public event EventHandler<ICraneJob>? OnJobRemoved;

    public IEnumerable<ICraneJob> GetJobForCrane(CraneComponent crane)
    {
        foreach (var entity in myRegistry.Entities)
        {
            var job = entity.GetComponent<MyCraneJob>();
            if (job && job.IsForCrane(crane))
            {
                yield return job;
            }
        }
    }

    public void NotifyNew(ICraneJob job) => OnPossibleNewJob?.Invoke(this, job);

    public void NotifyRemoved(ICraneJob job) => OnJobRemoved?.Invoke(this, job);
}
```

`GetJobForCrane` should only yield jobs that currently belong on that crane. Do not yield finished, deleted, or out-of-range work and expect the crane to filter it later beyond `IsForCrane`.

### Range checks

`CraneTower.WorkingBounds` is the claim volume. Overlap it with the target's occupied box.

Crane ships a helper on `BlockObject`:

```cs
public bool IsForCrane(CraneComponent crane)
    => crane.Tower.WorkingBounds.Overlaps(bo.GetConstructionBounds());
```

`GetConstructionBounds()` is the axis-aligned box of every cell the block object occupies. Prefer that over a single coordinate if the building is larger than 1×1.

A job can be on several cranes at once if their ranges overlap. `ProgressJob` receives the crane that is currently working it.

### Material jobs

Implement `IMaterialCraneJob` when the crane should haul goods to the target:

```cs
public interface IMaterialCraneJob : ICraneJob
{
    IEnumerable<GoodAmount> GetRemainingMaterials();
    IEnumerable<GoodAmount> GetTotalMaterials();
    event EventHandler? MaterialsChanged;
    int AddMaterial(GoodAmount material);
}
```

| Member | Contract |
|---|---|
| `GetRemainingMaterials` | Still-needed amounts. Drives hauler limits on the crane inventory and the queue's `delivered/total` icons. |
| `GetTotalMaterials` | Original recipe / demand, for the same icons. |
| `MaterialsChanged` | Raise when remaining need changes. The crane re-dumps stock and refreshes haul limits. |
| `AddMaterial` | Accept goods from the crane inventory. Return how many were actually added; leftovers are put back. |

At the same builder priority, material jobs are sorted ahead of non-material jobs, so construction-like work tends to run before demolish/gather.

### Example job

A component on some existing building type, listed with the provider above:

```cs
[AddTemplateModule2(typeof(MyWorkable))]
public class MyCraneJob(
    MyCraneJobProvider provider
) : BaseComponent, IInitializableEntity, IDeletableEntity, ICraneJob
{
    public const float HoursToFinish = 1f;

    MyWorkable subject = null!;
    BlockObject bo = null!;
    BuilderPrioritizable? prioritizable;
    BoundsInt workableArea;
    bool listed;
    float done;

    public Priority Priority => prioritizable?.Priority ?? Priority.Normal;

    public bool IsAvailable => subject && subject.HasWork;

    public float Progress => HoursToFinish <= 0f ? 1f : Mathf.Clamp01(done / HoursToFinish);

    public string JobNameLoc => "MyMod.CraneJob";

    public event EventHandler? AvailabilityChanged;
    public event EventHandler<PriorityChangedEventArgs>? PriorityChanged;

    public void InitializeEntity()
    {
        subject = GetComponent<MyWorkable>();
        bo = GetComponent<BlockObject>();
        prioritizable = GetComponent<BuilderPrioritizable>();
        workableArea = bo.GetConstructionBounds();

        if (prioritizable)
        {
            prioritizable!.PriorityChanged += OnPriorityChanged;
        }

        subject.WorkChanged += OnWorkChanged;
        if (IsAvailable)
        {
            ListJob();
        }
    }

    public void DeleteEntity()
    {
        if (prioritizable)
        {
            prioritizable!.PriorityChanged -= OnPriorityChanged;
        }

        subject.WorkChanged -= OnWorkChanged;
        Unlist();
    }

    public bool IsForCrane(CraneComponent crane)
        => crane.Tower.WorkingBounds.Overlaps(workableArea);

    public void ProgressJob(CraneComponent crane, float hours)
    {
        if (!IsAvailable)
        {
            return;
        }

        done += hours;
        if (done < HoursToFinish)
        {
            return;
        }

        subject.CompleteWork();
        done = 0f;
    }

    public void SetPriority(Priority priority) => prioritizable?.SetPriority(priority);

    void OnWorkChanged(object sender, EventArgs e)
    {
        AvailabilityChanged?.Invoke(this, EventArgs.Empty);
        if (IsAvailable)
        {
            ListJob();
        }
        else
        {
            Unlist();
        }
    }

    void OnPriorityChanged(object sender, PriorityChangedEventArgs e)
        => PriorityChanged?.Invoke(this, e);

    void ListJob()
    {
        if (listed)
        {
            return;
        }

        listed = true;
        provider.NotifyNew(this);
    }

    void Unlist()
    {
        if (!listed)
        {
            return;
        }

        listed = false;
        provider.NotifyRemoved(this);
    }
}
```

List the job when it becomes real work, and unlist it when the entity is deleted or no longer a candidate. Keep `GetJobForCrane` in sync with that listing.

### Built-in jobs as references

| Job | Component | Provider | Notes |
|---|---|---|---|
| Build | `ConstructionSiteCraneJob` | `ConstructionCraneJobProvider` | `IMaterialCraneJob` on every `ConstructionSite`. Own mast sections are claimed only by their crane. |
| Demolish | `DemolishableCraneJob` | `DemolishCraneJobProvider` | On every `Demolishable`. Listed while marked. |
| Gather | `RecoveredGoodStackCraneJob` | `RecoveredGoodStackCraneJobProvider` | On every `RecoveredGoodStack`. Moves unreserved stock into `CraneInventory` at 60 items per (already bonus-adjusted) hour. |

### Practical notes

- Do not call `CraneTower.AddJob` / `RemoveJob` yourself. Go through the provider events so every eligible crane stays in sync.
- `ProgressJob` should be safe to call when the job is no longer available (the crane may have started the tick while it still was).
- Several cranes can hold the same job. Design `ProgressJob` so overlapping cranes do not corrupt the target.
- Inventory dumping is automatic for `IMaterialCraneJob`. You do not haul from `ProgressJob` unless the job is something like gather, where the work *is* moving goods.
- There is no extra hook for “always first in the queue.” That slot is reserved for this crane's own unfinished sections.
