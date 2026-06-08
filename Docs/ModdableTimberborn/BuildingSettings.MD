# Building Settings

The Building Settings subsystem gives buildings **persistent, copy/paste-able configuration**. Each handler knows how to snapshot one `IDuplicable` component to JSON and reapply it to another instance of the same type — whether for a single building or when pasting settings across saves with different entity IDs.

ModdableTimberborn ships with handlers for every stock Timberborn `IDuplicable` component. You can also write your own handlers; they are discovered and registered automatically.

## Enabling the subsystem

Call `UseBuildingSettings()` from your `IModStarter`:

```cs
public class MStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .UseBuildingSettings()
            // ... other subsystems
        ;
    }
}
```

This is idempotent and chainable. Internally it adds [`BuildingSettingsConfig`](../../ModdableTimberborn/BuildingSettings/BuildingSettingsConfig.cs), which registers `BuildingSettingsResolver` as a singleton and auto-discovers every concrete `IBuildingSettings` in the assembly.

## Using the resolver

[`BuildingSettingsResolver`](../../ModdableTimberborn/BuildingSettings/BuildingSettingsResolver.cs) is the main entry point for consumers. Inject it wherever you need to snapshot or apply settings:

```cs
// Snapshot all settings for a building
BuildingSettingsPair[] pairs = resolver.Get(myBuilding);

// Reapply stored settings to a target building
foreach (var pair in pairs)
{
    string json = pair.Settings.Serialize(pair.Duplicable);
    pair.Settings.Deserialize(json, pair.Duplicable);
}
```

`BuildingSettingsPair` is a `readonly record struct(IDuplicable Duplicable, IBuildingSettings Settings)`. The `Get(BaseComponent)` overload queries all `IDuplicable` components on the building and returns one pair per component that has a registered handler.

An optional `HashSet<IBuildingSettings>` filter can restrict which handlers are applied — useful when a UI only exposes a subset of settings.

### Entity-GUID remapping (cross-save paste)

Some settings store references to other entities (automators, relays, timers). When pasting across saves the GUIDs differ. Before deserializing, check for `IEntityIdBuildingSettings` and pass the `idMappings` dictionary:

```cs
foreach (var pair in pairs)
{
    if (pair.Settings is IEntityIdBuildingSettings entitySettings)
    {
        entitySettings.Deserialize(json, pair.Duplicable, idMappings);
    }
    else
    {
        pair.Settings.Deserialize(json, pair.Duplicable);
    }
}
```

## Writing a custom handler

### 1. Define a model record

The model is a plain record that Newtonsoft.Json serializes. For a simple scalar value, use one of the shared primitives in [`CommonSettingModels.cs`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/CommonSettingModels.cs) (`BoolSettingModel`, `IntSettingModel`, `FloatSettingModel`, `StringSettingModel`). For anything richer, define your own:

```cs
public record MyComponentSettingsModel(int Threshold, bool IsEnabled);
```

Use `[JsonIgnore]` on any computed or cached properties that should not be persisted.

### 2. Extend `BuildingSettingsBase<T, TModel>`

[`BuildingSettingsBase<T, TModel>`](../../ModdableTimberborn/BuildingSettings/BuildingSettingsBase.cs) is the abstract base for most handlers. You must override three members:

| Member | Responsibility |
|---|---|
| `GetModel(T duplicable)` | Snapshot the component's current state to a model record. |
| `ApplyModel(TModel model, T target)` | Push the model's values back onto the target component. Return `true` if the apply succeeded. |
| `DescribeModel(TModel model)` | Return a short, localized one-line summary of the model for display in the UI. |

`T` must be a `BaseComponent` that also implements `IDuplicable<T>`. The base class resolves the localized `Name` via the key `LV.MT.BldSet.<TypeName>` during game load.

#### Minimal example

Here is a handler for a hypothetical `MyComponent` that exposes a single `int` worker count, modeled on the built-in [`WorkplaceSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/WorkplaceSettings.cs):

```cs
public class MyComponentSettings(ILoc t) 
    : BuildingSettingsBase<MyComponent, IntSettingModel>(t)
{
    // Snapshot: read the current state off the component.
    protected override IntSettingModel GetModel(MyComponent duplicable)
        => duplicable.CurrentCount;

    // Apply: push the stored value back, with any guards you need.
    protected override bool ApplyModel(IntSettingModel model, MyComponent target)
    {
        target.SetCount(Math.Min(model.Value, target.MaxCount));
        return true;
    }

    // Describe: one-line summary shown in the paste-preview UI.
    public override string DescribeModel(IntSettingModel model)
        => model.Value.ToString();
}
```

You can override `CanDeserialize(T? target)` to add extra guards before the model is applied (the base implementation checks `target != null && target.IsDuplicable`).

#### Controlling apply order

The default `Order` is `1000`. Override `IBuildingSettings.Order` explicitly if your handler must run before others — for example the built-in `PausableBuildingSettings` uses `100` so pause state is set before any other settings are applied:

```cs
public class MyEarlySettings(ILoc t) : BuildingSettingsBase<MyComponent, BoolSettingModel>(t), IBuildingSettings
{
    int IBuildingSettings.Order => 50;
    // ...
}
```

### 3. Handlers that reference other entities

If your model stores a reference to another entity (by `Guid`), inherit [`EntityIdBuildingSettingsBase<T, TModel>`](../../ModdableTimberborn/BuildingSettings/EntityIdBuildingSettingsBase.cs) instead, and let your model record extend `EntityIdModelBase`. The base class handles GUID substitution generically when the caller provides an `idMappings` dictionary.

```cs
// Model: expose each entity GUID through the EntityIds slot array.
public record MyLinkedSettingsModel(Guid? LinkedId)
    : EntityIdModelBase([LinkedId])
{
    public Guid? LinkedId
    {
        get => EntityIds[0];
        set => EntityIds[0] = value;
    }
}

public class MyLinkedSettings(EntityRegistry entityRegistry, ILoc t)
    : EntityIdBuildingSettingsBase<MyLinkedComponent, MyLinkedSettingsModel>(t)
{
    protected override MyLinkedSettingsModel GetModel(MyLinkedComponent duplicable)
        => new(duplicable.LinkedEntity?.EntityId);

    protected override bool ApplyModel(MyLinkedSettingsModel model, MyLinkedComponent target)
    {
        var entity = entityRegistry.TryGetEntity(model.LinkedId);
        target.SetLinked(entity);
        return true;
    }

    public override string DescribeModel(MyLinkedSettingsModel model)
        => entityRegistry.DescribeEntity(model.LinkedId, t);
}
```

See [`AutomatableSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/AutomatableSettings.cs) for the canonical real-world example.

### 4. No manual registration needed

`BuildingSettingsConfig` scans the assembly at startup and registers every non-abstract `IBuildingSettings` class it finds as a multibind singleton. Add the class; nothing else is required.

## Built-in handlers

ModdableTimberborn includes handlers for all stock Timberborn `IDuplicable` components — floodgates, manufactories, workplaces, sensors, levers, and more. They live in [`BuiltInSettings/`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/) and are good reading material for patterns:

- **Simple scalar** — [`WorkplaceSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/WorkplaceSettings.cs), [`WaterSourceSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/WaterSourceSettings.cs)
- **Bool toggle** — [`PausableBuildingSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/PausableBuildingSettings.cs), [`DemolishableSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/DemolishableSettings.cs)
- **Rich model with guards** — [`FloodgateSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/FloodgateSettings.cs), [`ManufactorySettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/ManufactorySettings.cs)
- **String-ID lookup with lazy caching** — [`SingleGoodAllowerSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/SingleGoodAllowerSettings.cs) (uses `CachableStringSettingModel<GoodSpec>`)
- **Entity-linked** — [`AutomatableSettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/AutomatableSettings.cs), [`RelaySettings`](../../ModdableTimberborn/BuildingSettings/BuiltInSettings/RelaySettings.cs)

## Key types at a glance

| Type | Role |
|---|---|
| [`IBuildingSettings`](../../ModdableTimberborn/BuildingSettings/IBuildingSettings.cs) | Core interface: serialize, deserialize, describe, order. |
| [`BuildingSettingsBase<T, TModel>`](../../ModdableTimberborn/BuildingSettings/BuildingSettingsBase.cs) | Abstract base; override `GetModel`, `ApplyModel`, `DescribeModel`. |
| [`EntityIdBuildingSettingsBase<T, TModel>`](../../ModdableTimberborn/BuildingSettings/EntityIdBuildingSettingsBase.cs) | Variant for handlers whose model contains entity GUIDs. |
| [`IEntityIdBuildingSettings`](../../ModdableTimberborn/BuildingSettings/IEntityIdBuildingSettings.cs) | Marker interface; callers use it to decide whether to pass `idMappings`. |
| [`IEntityIdModel` / `EntityIdModelBase`](../../ModdableTimberborn/BuildingSettings/IEntityIdModel.cs) | Model base that exposes the `Guid?[]` slot array for generic remapping. |
| [`BuildingSettingsResolver`](../../ModdableTimberborn/BuildingSettings/BuildingSettingsResolver.cs) | Singleton; resolves handlers by type and returns `BuildingSettingsPair[]` for a building. |
| [`BuildingSettingsPair`](../../ModdableTimberborn/BuildingSettings/BuildingSettingsResolver.cs) | `readonly record struct(IDuplicable, IBuildingSettings)` returned by the resolver. |
| [`BuildingSettingsConfig`](../../ModdableTimberborn/BuildingSettings/BuildingSettingsConfig.cs) | Registry config; activated by `UseBuildingSettings()`. Auto-discovers all handlers. |
