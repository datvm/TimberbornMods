# Spec & Template Modifiers

This subsystem lets you rewrite Timberborn's component-spec blueprints and prefab template blueprints immediately after the game loads them — without writing Harmony patches yourself. Two Harmony postfixes (one on `SpecService.Load`, one on `TemplateCollectionService.Load`) fire a coordinated runner chain that calls your registered services in declaration order.

## Opt in

Add `IWithDIConfig` to your configuration class. `BaseModdableTimberbornConfiguration.StartMod` detects the interface and activates the subsystem automatically:

```cs
public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; }
        = ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindSpecModifier<MyGoodSpecModifier>()
            .BindTemplateModifier<MyBuildingTemplateModifier>()
        ;
    }
}
```

The older `ModdableTimberbornRegistry.Instance.UseDependencyInjection()` call still works but is `[Obsolete]`; prefer `IWithDIConfig`.

Source: [`ModdableDependencyInjectionConfig.cs`](../../ModdableTimberborn/DependencyInjection/ModdableDependencyInjectionConfig.cs), [`IModdableTimberbornRegistryConfig.cs`](../../ModdableTimberborn/Registry/IModdableTimberbornRegistryConfig.cs), [`BaseModdableTimberbornConfiguration.cs`](../../ModdableTimberborn/Registry/BaseModdableTimberbornConfiguration.cs)

---

## Spec modifiers

Spec modifiers run **after `SpecService.Load`**. The game has already deserialized all blueprint JSON into in-memory `ComponentSpec` records; your modifier receives a batch of those records and returns a (possibly different) batch. The framework writes the result back into `SpecService._cachedBlueprintsBySpecs` so the rest of the game sees the changes.

### When they run

A Harmony postfix on `SpecService.Load` fires `SpecServiceRunner.OnSpecLoaded`, which calls every registered `ISpecServiceTailRunner` in sequence. The built-in tail runner `SpecModifierService` groups all `ISpecModifier`s by their target `ComponentSpec` type and dispatches them in `Order` order.

If your service implements `ISpecServiceFrontRunner` (which extends `ILoadableSingleton`), its `Load()` is called **before** `SpecService.Load` — useful for reading config files or caching data that the modifier will need. A plain `ISpecModifier` registered via `BindSpecModifier` carries no such timing guarantee.

### Base classes

Three entry points exist, from lowest- to highest-level:

| Class | Use when |
|---|---|
| `ISpecModifier` | Full control; you receive and return `IEnumerable<EditableBlueprint>`. |
| `BaseBlueprintModifier<T>` | Sets `Type` for you; you still work with `EditableBlueprint`. |
| `BaseSpecModifier<T>` | Each blueprint has exactly one spec of type `T`; you receive and return `IEnumerable<NamedSpec<T>>`. **Warning:** rebuilds each blueprint from a single spec, so all sibling specs and child blueprints are silently dropped. Only safe when each blueprint has exactly one spec of type `T` and no children need preserving. For multi-spec or children cases use `BaseBlueprintModifier<T>` instead (see [`FactionSpecModifier.cs`](../../ConfigurableTubeZipLine/Services/FactionSpecModifier.cs)). |
| `BaseSpecTransformer<T>` | You transform one `T` at a time; return the modified spec or `null` to leave it unchanged. |

`NamedSpec<T>` is a `readonly record struct` pairing a blueprint name (`string Name`) with a typed spec (`T Spec`).

Source: [`SpecInterfaces.cs`](../../ModdableTimberborn/DependencyInjection/Specs/SpecInterfaces.cs), [`BaseSpecModifier.cs`](../../ModdableTimberborn/DependencyInjection/Specs/BaseSpecModifier.cs), [`BaseSpecTransformer.cs`](../../ModdableTimberborn/DependencyInjection/Specs/BaseSpecTransformer.cs)

### Example — modify existing specs with `BaseSpecModifier<T>`

Append a cross-faction building path to every `TemplateCollectionSpec` entry:

```cs
// Add a Folktails building to every IronTeeth template collection spec.
public class FactionSpecModifier : BaseSpecModifier<TemplateCollectionSpec>
{
    protected override IEnumerable<NamedSpec<TemplateCollectionSpec>> Modify(
        IEnumerable<NamedSpec<TemplateCollectionSpec>> specs)
    {
        foreach (var s in specs)
            yield return s; // pass existing specs through unchanged

        // Inject an entirely new spec entry.
        yield return new("TemplateCollection.Buildings.IronTeeth", new()
        {
            CollectionId = "Buildings.IronTeeth",
            Blueprints = [ /* AssetRef<BlueprintAsset> items */ ],
        });
    }
}
```

Register it:

```cs
configurator.BindSpecModifier<FactionSpecModifier>();
```

Real-world example: [`TemplateCollectionSpecModifier.cs`](../../ConfigurableTubeZipLine/Services/TemplateCollectionSpecModifier.cs)

### Example — transform specs in place with `BaseSpecTransformer<T>`

Change one property on every matching spec without touching blueprint structure:

```cs
public class GoodSpecModifier(TopBarConfigProvider provider) : BaseSpecTransformer<GoodSpec>
{
    public override GoodSpec? Transform(GoodSpec spec)
    {
        // Return null to leave this spec unchanged.
        var compiled = provider.CompileGoodSpecs();
        if (!compiled.TryGetValue(spec.Id, out var item)) { return null; }

        return spec with
        {
            GoodGroupId = item.GroupId,
            GoodOrder   = item.Order,
        };
    }
}
```

For reference: [`GoodSpecModifier.cs`](../../ConfigurableTopBar/Services/GoodSpecModifier.cs) uses this pattern, though it is bound as a plain singleton and invoked manually rather than via `BindSpecModifier`.

### Ordering

Both `ISpecModifier.Order` (default `0`) and `ISpecModifier.ShouldRun` (default `true`) are interface default members — override them when needed:

```cs
public class MyModifier : BaseSpecModifier<GoodSpec>
{
    public override int Order => 10; // run after order-0 modifiers
    public override bool ShouldRun => MSettings.Enabled;

    protected override IEnumerable<NamedSpec<GoodSpec>> Modify(
        IEnumerable<NamedSpec<GoodSpec>> specs)
    { /* ... */ }
}
```

### Low-level tail runner

For full access to `SpecService` — rather than a single spec type — implement `ISpecServiceTailRunner` directly:

```cs
public class MySpecTailRunner : ISpecServiceTailRunner
{
    public void Run(SpecService specService)
    {
        // Inspect or mutate specService as needed.
    }
}
```

Register with:

```cs
configurator.BindSpecTailRunner<MySpecTailRunner>();
```

Source: [`SpecServiceRunner.cs`](../../ModdableTimberborn/DependencyInjection/Specs/SpecServiceRunner.cs), [`SpecModifierService.cs`](../../ModdableTimberborn/DependencyInjection/Specs/SpecModifierService.cs)

---

## Template modifiers

Template modifiers run **after `TemplateCollectionService.Load`** (at `HarmonyPriority.First`, so before any other mod's postfix on the same method). The game has already loaded all prefab template blueprints; your modifier receives each one as an `EditableBlueprint` and can replace component specs inside it. The framework writes changes back and updates `BlueprintSourceService` so Timberborn's provenance tracking stays consistent.

### When they run

A Harmony postfix on `TemplateCollectionService.Load` fires `TemplateCollectionTailRunnerService.Run`, which calls every registered `ITemplateCollectionServiceTailRunner` in `Order` sequence. The built-in runner `TemplateModifierTailRunner` iterates all templates, calls each `ITemplateModifier.ShouldModify` as a gate, and if it returns `true` calls `Modify`.

### The `ITemplateModifier` interface

```cs
public interface ITemplateModifier
{
    int Order => 0;

    bool ShouldModify(string blueprintName, string templateName,
                      TemplateSpec originalTemplateSpec);

    EditableBlueprint? Modify(EditableBlueprint template,
                              TemplateSpec originalTemplateSpec,
                              Blueprint original);
}
```

- `ShouldModify` is called for every template; return `false` to skip it cheaply.
- `Modify` receives the working `EditableBlueprint` (may have been modified by an earlier modifier with lower `Order`), the original `TemplateSpec`, and the original unmodified `Blueprint`. Return `null` to leave the template unchanged; return the (mutated) `EditableBlueprint` to apply the change.

Source: [`ITemplateModifier.cs`](../../ModdableTimberborn/DependencyInjection/TemplateCollection/ITemplateModifier.cs), [`TemplateModifierTailRunner.cs`](../../ModdableTimberborn/DependencyInjection/TemplateCollection/TemplateModifierTailRunner.cs)

### Example — modify a component spec inside a prefab template

Halve the max distance and max connections on every zipline tower template:

```cs
public class ZiplineTemplateModifier : ITemplateModifier
{
    public bool ShouldModify(string blueprintName, string templateName,
                             TemplateSpec originalTemplateSpec)
        => originalTemplateSpec.IsZipline(); // cheap spec-based gate

    public EditableBlueprint? Modify(EditableBlueprint template,
                                     TemplateSpec originalTemplateSpec,
                                     Blueprint original)
    {
        for (int i = 0; i < template.Specs.Count; i++)
        {
            if (template.Specs[i] is ZiplineTowerSpec tower)
            {
                template.Specs[i] = tower with
                {
                    MaxConnections = MSettings.ZiplineMaxConnection,
                    MaxDistance    = MSettings.ZiplineMaxDistance,
                };
            }
        }

        return template; // returning non-null marks the template as changed
    }
}
```

Register it:

```cs
configurator.BindTemplateModifier<ZiplineTemplateModifier>();
```

Real-world example: [`ZiplineTemplateModifier.cs`](../../ConfigurableTubeZipLine/Services/ZiplineTemplateModifier.cs)

### Low-level tail runner

For direct access to `TemplateCollectionService` implement `ITemplateCollectionServiceTailRunner`:

```cs
public class MyTemplateTailRunner : ITemplateCollectionServiceTailRunner
{
    public int Order => 5;

    public void Run(TemplateCollectionService templateCollectionService)
    {
        // Read or rewrite templateCollectionService.AllTemplates as needed.
    }
}
```

Register with:

```cs
configurator.BindTemplateTailRunner<MyTemplateTailRunner>();
```

If the runner also implements `ILoadableSingleton`, its `Load()` runs **before** `TemplateCollectionService.Load`.

Source: [`ITemplateCollectionServiceTailRunner.cs`](../../ModdableTimberborn/DependencyInjection/TemplateCollection/ITemplateCollectionServiceTailRunner.cs), [`TemplateCollectionTailRunnerService.cs`](../../ModdableTimberborn/DependencyInjection/TemplateCollection/TemplateCollectionTailRunnerService.cs)

---

## `EditableBlueprint`

Both pipelines pass data as `EditableBlueprint`, a mutable wrapper around Timberborn's immutable `Blueprint`:

| Member | Type | Purpose |
|---|---|---|
| `Name` | `string` | Blueprint name (e.g. `"TemplateCollection.Buildings.Folktails"`). |
| `Specs` | `List<ComponentSpec>` | All component specs on this blueprint. |
| `Children` | `List<EditableBlueprint>` | Child blueprints, if any. |
| `GetSpec<T>()` | `T` | Returns the first spec of type `T`; throws if absent. |
| `TransformSpec<T>(Func<T, T?>)` | `void` | Replaces every spec of type `T` in place. |
| `ToBlueprint()` | `Blueprint` | Converts back to an immutable `Blueprint`. |

Source: [`EditableBlueprint.cs`](../../ModdableTimberborn/DependencyInjection/EditableBlueprint.cs)

---

## `AssetRefService`

When a spec modifier needs to inject asset references (e.g. adding blueprint paths to a `TemplateCollectionSpec`), inject `AssetRefService` — a bootstrapper-context singleton that wraps `IAssetLoader`:

```cs
public class MySpecModifier(AssetRefService assetRefService) : BaseSpecModifier<TemplateCollectionSpec>
{
    protected override IEnumerable<NamedSpec<TemplateCollectionSpec>> Modify(
        IEnumerable<NamedSpec<TemplateCollectionSpec>> specs)
    {
        foreach (var s in specs)
            yield return s;

        yield return new("TemplateCollection.Buildings.Folktails", new()
        {
            CollectionId = "Buildings.Folktails",
            Blueprints = [..assetRefService.CreateBlueprintAssetRefs([
                "Buildings/Paths/MyBuilding/MyBuilding.Folktails.blueprint",
            ])],
        });
    }
}
```

Source: [`AssetRefService.cs`](../../ModdableTimberborn/DependencyInjection/AssetRefService.cs)

---

## Registration quick-reference

All helpers are extension methods on `Configurator` from [`ConfigurationExtensions.cs`](../../ModdableTimberborn/Helpers/ConfigurationExtensions.cs):

| Method | Registers as |
|---|---|
| `configurator.BindSpecModifier<T>()` | `ISpecModifier` (multi-bound singleton) |
| `configurator.BindSpecTailRunner<T>()` | `ISpecServiceTailRunner` (multi-bound singleton) |
| `configurator.BindTemplateModifier<T>()` | `ITemplateModifier` (multi-bound singleton) |
| `configurator.BindTemplateTailRunner<T>()` | `ITemplateCollectionServiceTailRunner` (multi-bound singleton) |

Pass `alsoBindSelf: true` to any of these if you also need the concrete type injected elsewhere. All bindings are singletons; the game context guard in your `Configure` method determines when they are active.
