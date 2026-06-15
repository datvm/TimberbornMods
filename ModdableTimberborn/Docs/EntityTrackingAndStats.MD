# Entity Tracking & Stats

This page covers two related subsystems:

- **Entity Tracker** — maintains live, categorised sets of in-game entities so your code can query "all adults" or "all workplaces" without walking the full entity list.
- **Updatable Entity Stats** — attaches observable, pausable stat trackers to entities so UI panels can react to value changes without polling.

Both subsystems are opt-in and are activated independently from your `IModStarter`.

---

## Entity Tracker

### Enabling

Call `UseEntityTracker()` on the registry before any configurators run:

```cs
public class MStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .UseEntityTracker()
            .AddConfigurator<MyGameConfig>();
    }
}
```

`UseEntityTracker()` is idempotent — calling it more than once is safe.

### What you get out of the box

Two specialised trackers are always registered when the subsystem is enabled:

#### `CharacterTracker`

Tracks every character entity and routes them into typed sets. Inject it anywhere in a Game context:

```cs
public class MyService(CharacterTracker characterTracker)
{
    public void DoWork()
    {
        // Pre-split sets — no LINQ required
        foreach (var adult in characterTracker.Adults)   { /* CharacterTrackerComponent */ }
        foreach (var child in characterTracker.Children) { /* CharacterTrackerComponent */ }
        foreach (var bot   in characterTracker.Bots)     { /* CharacterTrackerComponent */ }

        // Convenience enumerables
        foreach (var beaver in characterTracker.Beavers)  { /* adults + children */ }
        foreach (var worker in characterTracker.Workers)  { /* adults + bots */ }
    }
}
```

Filter by flag combination using `GetCharacters`:

```cs
// Iterate all adult beavers and bots together
foreach (var c in characterTracker.GetCharacters(CharacterType.AdultBeaver | CharacterType.Bot))
{
    // c is CharacterTrackerComponent
}

// Project directly to another component type
foreach (var hunger in characterTracker.GetCharacters<Hunger>(CharacterType.AdultBeaver))
{
    // hunger is Hunger
}
```

`CharacterTrackerComponent` is the payload attached to each character. Key members:

| Member | Type | Notes |
|---|---|---|
| `CharacterType` | `CharacterType` | `AdultBeaver`, `ChildBeaver`, or `Bot` |
| `Worker` | `Worker?` | Null for children and some bots |
| `IsAdult` / `IsChild` / `IsBot` / `IsBeaver` / `IsWorker` | `bool` | Convenience flags |
| `BonusTracker` | `BonusTracker?` | Present when the Bonus System is also enabled |
| `HasBonus(string id)` | `bool` | Shorthand for checking an active named bonus |

#### `WorkplaceTracker`

Tracks every entity that carries a `Workplace` component:

```cs
public class MyService(WorkplaceTracker workplaceTracker)
{
    public void DoWork()
    {
        foreach (var wp in workplaceTracker.Entities)
        {
            // wp is WorkplaceTrackerComponent
            // wp.Workplace     — the Timberborn Workplace component
            // wp.TemplateName  — prefab template name
            // wp.IsBuilderWorkplace
        }

        // Filtered view — builder workplaces only
        foreach (var bw in workplaceTracker.BuilderWorkplaces) { }
    }
}
```

`WorkplaceTracker` also forwards worker-assignment events from the underlying `Workplace` component so you do not have to subscribe to each building individually:

```cs
workplaceTracker.OnWorkerAssigned   += (comp, worker) => { /* ... */ };
workplaceTracker.OnWorkerUnassigned += (comp, worker) => { /* ... */ };
```

### Tracking a custom component type

To maintain a live set for any `BaseComponent` you define, call `TryTrack<T>()` during bootstrap:

```cs
ModdableTimberbornRegistry.Instance
    .UseEntityTracker()
    .TryTrack<MyBuildingComponent>();
```

This registers a `DefaultEntityTracker<MyBuildingComponent>` singleton. Inject it wherever you need the live set:

```cs
public class MyService(DefaultEntityTracker<MyBuildingComponent> tracker)
{
    public void DoWork()
    {
        foreach (var comp in tracker.Entities) { /* ... */ }
    }
}
```

You can also react to entities arriving and leaving:

```cs
tracker.OnEntityRegistered   += comp => { /* entity appeared */ };
tracker.OnEntityUnregistered += comp => { /* entity removed  */ };
```

`TryTrack<T>` throws `InvalidOperationException` if you pass a type that is already covered by a built-in tracker (`Character`, `WorkplaceSpec`, and their related types). Check the error message — it names the tracker you should inject instead.

### How tracking works internally

`EntityTrackerController` is a singleton that subscribes to `EntityInitializedEvent` and `EntityDeletedEvent` via the Timberborn `EventBus`. On each event it iterates every registered `IEntityTracker` and calls `Track` or `Untrack`. Each tracker calls `entity.GetComponent<T>()` internally and silently skips entities that do not carry the expected component, so the fan-out is safe regardless of entity type.

### The `IEntityTracker<T>` interface

If you inject by interface rather than concrete type, the contract is:

```cs
public interface IEntityTracker<T> : IEntityTracker where T : BaseComponent
{
    IReadOnlyCollection<T> Entities { get; }
    event Action<T>? OnEntityRegistered;
    event Action<T>? OnEntityUnregistered;
}
```

Source: [`../../ModdableTimberborn/EntityTracker/IEntityTracker.cs`](../../ModdableTimberborn/EntityTracker/IEntityTracker.cs)

---

## Updatable Entity Stats

### Enabling

```cs
ModdableTimberbornRegistry.Instance
    .UseUpdatableEntityStats()
    .AddConfigurator<MyGameConfig>();
```

Like all `Use…()` calls this is idempotent and chainable.

### Core concepts

Each *stat* is a singleton factory (`IUpdatableEntityStat`) that knows how to create a *tracker* (`IEntityStatTracker`) for one specific entity. The tracker is the live, observable object: it fires `OnValueChanged` whenever the underlying game state changes and can be paused when the owning UI panel is not visible.

In a Game context, every entity automatically carries an `UpdatableEntityStatComponent` (added as a decorator on every `TemplateSpec` — the decorator registration is guarded by `if (!context.IsGameContext()) return;` in `UpdatableEntityStatsConfig`). Trackers self-register with this component on construction, so when the entity is deleted the component calls `NotifyEntityLost` and disposes all trackers for you.

### Built-in stats

`UpdatableEntityStatService` aggregates every `IUpdatableEntityStat` in the assembly. Inject it to discover what is available:

```cs
public class MyPanel(UpdatableEntityStatService statService) { }
```

| Id | C# type | Value type | Driven by |
|---|---|---|---|
| `"Name"` | `NameStat` | `string` | `NamedEntity.EntityNameChanged` |
| `"Avatar"` | `AvatarStat` | `Sprite?` | `Contaminable.ContaminationChanged` |
| `"ProductionPercent"` | `ProductionPercentStat` | `float` (%) | `Manufactory.ProductionProgressed` |
| `"RecipeIcon"` | `RecipeIconStat` | `Sprite?` | `Manufactory.RecipeChanged` |
| `"StorageStock"` | `InventoryStockStat` | `int` | `Inventory.InventoryChanged` |
| `"StorageStockMax"` | `InventoryStockMax` | `int` | `Inventory.InventoryChanged` |
| `"StoragePercent"` | `InventoryPercent` | `float` (%) | `Inventory.InventoryChanged` |
| `"StockpileIcon"` | `StockpileIconStat` | `Sprite?` | `SingleGoodAllower.DisallowedGoodsChanged` |
| `"Population"` | `PopulationStat` | `int` | `PopulationStatService.OnPopulationChanged` |

Percent stats format `ValueFormatted` as `"P0"` (e.g. `"42 %"`). Image stats expose `Sprite?`; check for null before assigning to a UI element.

### Using a stat in a UI panel

The typical pattern for a panel that opens on a selected entity:

```cs
public class MyEntityPanel(UpdatableEntityStatService statService)
{
    IEntityStatTracker<string>? _nameTracker;

    public void ShowEntity(UpdatableEntityStatComponent entityStatComp)
    {
        // Look up the stat factory by id (or inject NameStat directly)
        if (!statService.TryGetStat("Name", out var stat)) { return; }
        if (!stat.TryGetTracker(entityStatComp, out var tracker)) { return; }

        _nameTracker = (IEntityStatTracker<string>)tracker;
        _nameTracker.OnValueChanged += OnNameChanged;
        _nameTracker.OnEntityLost   += OnEntityLost;
        _nameTracker.Start();

        // ForceUpdating() raises OnValueChanged even if the value has not changed,
        // useful to populate the UI on first open.
        _nameTracker.ForceUpdating();
    }

    public void HidePanel()
    {
        _nameTracker?.Pause();
        // or Dispose() if you will create a fresh tracker next time
    }

    void OnNameChanged(object sender, EventArgs e)
        => nameLabel.text = _nameTracker!.ValueFormatted;

    void OnEntityLost(object sender, UpdatableEntityStatComponent e)
        => HidePanel();
}
```

Key points:
- `TryGetTracker` returns `false` when the entity does not support the stat (e.g. asking for `"ProductionPercent"` on a stockpile). Always guard the return value.
- The tracker constructor registers itself with `UpdatableEntityStatComponent` immediately, so call `Dispose()` if you decide not to use a tracker you created.
- `Pause()` unsubscribes from game events without disposing. `Start()` re-subscribes and calls `UpdateValue()` once.
- To enumerate all available stats, use `statService.AllStats` (every registered `IUpdatableEntityStat`), `statService.AllImageStats` (subset of `IImageStat`), or `statService.AllPercentStats` (subset of `IPercentStat`).

### The `IStatTracker` contract

```cs
public interface IStatTracker : IDisposable  // Dispose() comes from IDisposable
{
    void Start();
    void Pause();
    bool Running { get; }
    void ForceUpdating();
    string ValueFormatted { get; }
    event EventHandler? OnValueChanged;
}

public interface IStatTracker<T> : IStatTracker
{
    T? Value { get; }
    event EventHandler<T?>? OnTypedValueChanged;
}

public interface IEntityStatTracker : IStatTracker
{
    UpdatableEntityStatComponent Component { get; }
    void NotifyEntityLost();
    event EventHandler<UpdatableEntityStatComponent>? OnEntityLost;
}
```

Source: [`../../ModdableTimberborn/UpdatableEntityStats/IEntityStatTracker.cs`](../../ModdableTimberborn/UpdatableEntityStats/IEntityStatTracker.cs)

### Writing a custom stat

Implement `IUpdatableEntityStat` and a paired tracker. The fastest route is to extend the provided base classes:

**1. The tracker** — extend `StatTrackerBase<T>` and implement three abstract members:

```cs
public class MyHealthTracker(MyHealthComponent health, UpdatableEntityStatComponent comp)
    : StatTrackerBase<float>(comp)  // registers with comp immediately
{
    protected override void OnStart()
        => health.HealthChanged += OnHealthChanged;

    protected override void OnPause()
        => health.HealthChanged -= OnHealthChanged;

    protected override float CalculateValue()
        => health.CurrentHealth / health.MaxHealth;

    void OnHealthChanged(object sender, EventArgs e) => UpdateValue();
}
```

**2. The stat factory** — extend `ComponentUpdatableEntityStatBase<T, TComp>` if your stat gates on a single component:

```cs
public class MyHealthStat : ComponentUpdatableEntityStatBase<float, MyHealthComponent>
{
    public override string Id => "MyHealth";
    // DisplayLoc defaults to "LV.MT.UStat.MyHealth"

    protected override IEntityStatTracker<float>? GetComponentTracker(
        UpdatableEntityStatComponent statComp, MyHealthComponent comp)
        => new MyHealthTracker(comp, statComp);
}
```

`UpdatableEntityStatsConfig` discovers `IUpdatableEntityStat` implementations via reflection at startup — but only within the **ModdableTimberborn** assembly (`Assembly.GetExecutingAssembly()`). A stat you define in your own mod assembly is **not** discovered automatically; multibind it in your configurator:

```cs
configurator.MultiBind<IUpdatableEntityStat>().To<MyHealthStat>().AsSingleton();
```

Once bound, it is available through `UpdatableEntityStatService`.

For a percent stat (displayed as `"P0"`), extend `ComponentUpdatableEntityPercentStatBase<TComp>` and implement `GetComponentPercentTracker` instead. The tracker should extend `StatPercentTrackerBase`.

### Population stat

`PopulationStat` has a slightly richer API because population can be scoped to a district or reported globally, and the mode (adult count, total, capacity, etc.) is configurable at call time.

```cs
public class MyWidget(UpdatableEntityStatService statService)
{
    // District-bound (entity must have DistrictCenter)
    public void TrackDistrict(UpdatableEntityStatComponent districtComp)
    {
        var opts = new PopulationCounterOptions(
            PopulationCounterMode.Adults,
            CountBeavers: true,
            CountBots: false);

        statService.PopulationStat.TryGetTracker(opts, districtComp, out var tracker);
        tracker?.Start();
    }

    // Global (not entity-bound — caller owns disposal)
    public void TrackGlobal()
    {
        var opts = new PopulationCounterOptions(PopulationCounterMode.Adults, true, false);
        var tracker = statService.PopulationStat.GetGlobalTracker(opts);
        tracker.Start();
        // Must call tracker.Dispose() manually when done.
    }
}
```

`GlobalPopulationStatTracker` does not attach to any entity, so it is never cleaned up automatically — dispose it yourself when the UI element is closed.

---

## Source files

| File | Purpose |
|---|---|
| [`../../ModdableTimberborn/EntityTracker/EntityTrackerConfig.cs`](../../ModdableTimberborn/EntityTracker/EntityTrackerConfig.cs) | DI wiring + `UseEntityTracker()` / `TryTrack<T>()` registry extensions |
| [`../../ModdableTimberborn/EntityTracker/EntityTrackerController.cs`](../../ModdableTimberborn/EntityTracker/EntityTrackerController.cs) | Event-bus listener; fans out to all `IEntityTracker` instances |
| [`../../ModdableTimberborn/EntityTracker/IEntityTracker.cs`](../../ModdableTimberborn/EntityTracker/IEntityTracker.cs) | `IEntityTracker` and `IEntityTracker<T>` interfaces |
| [`../../ModdableTimberborn/EntityTracker/DefaultEntityTracker.cs`](../../ModdableTimberborn/EntityTracker/DefaultEntityTracker.cs) | Generic tracker used for `TryTrack<T>()` registrations |
| [`../../ModdableTimberborn/EntityTracker/CharacterTracker.cs`](../../ModdableTimberborn/EntityTracker/CharacterTracker.cs) | Built-in character tracker (adults / children / bots) |
| [`../../ModdableTimberborn/EntityTracker/WorkplaceTracker.cs`](../../ModdableTimberborn/EntityTracker/WorkplaceTracker.cs) | Built-in workplace tracker |
| [`../../ModdableTimberborn/EntityTracker/Components/`](../../ModdableTimberborn/EntityTracker/Components/) | `CharacterTrackerComponent` and `WorkplaceTrackerComponent` |
| [`../../ModdableTimberborn/UpdatableEntityStats/UpdatableEntityStatsConfig.cs`](../../ModdableTimberborn/UpdatableEntityStats/UpdatableEntityStatsConfig.cs) | DI wiring + `UseUpdatableEntityStats()` registry extension |
| [`../../ModdableTimberborn/UpdatableEntityStats/IUpdatableEntityStat.cs`](../../ModdableTimberborn/UpdatableEntityStats/IUpdatableEntityStat.cs) | Stat factory interfaces |
| [`../../ModdableTimberborn/UpdatableEntityStats/IEntityStatTracker.cs`](../../ModdableTimberborn/UpdatableEntityStats/IEntityStatTracker.cs) | Tracker lifecycle interfaces |
| [`../../ModdableTimberborn/UpdatableEntityStats/UpdatableEntityStatComponent.cs`](../../ModdableTimberborn/UpdatableEntityStats/UpdatableEntityStatComponent.cs) | Per-entity tracker registry; handles entity deletion |
| [`../../ModdableTimberborn/UpdatableEntityStats/UpdatableEntityStatService.cs`](../../ModdableTimberborn/UpdatableEntityStats/UpdatableEntityStatService.cs) | Singleton aggregating all registered stats |
| [`../../ModdableTimberborn/UpdatableEntityStats/Implementations/`](../../ModdableTimberborn/UpdatableEntityStats/Implementations/) | All built-in stat + tracker pairs; base classes |
