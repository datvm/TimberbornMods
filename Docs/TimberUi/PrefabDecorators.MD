# Decorating Game Prefabs with Template Modules

Timberborn builds each in-game entity from a prefab **template**: a fixed list of component types assembled once at load time. A **template-module decorator** lets your mod inject an extra `BaseComponent` onto every entity that already carries a particular component — without touching any game prefab asset. This is how you bolt new behaviour onto stock buildings, workers, or resources entirely in code.

This guide covers both registration paths — the zero-boilerplate attribute path and the fully-imperative path — and explains how they interact with Timberborn's four game contexts.

**Relevant source:**
- [`../../TimberUi/TimberUi/DIAttributes/AddTemplateModuleAttribute.cs`](../../TimberUi/TimberUi/DIAttributes/AddTemplateModuleAttribute.cs) — `[AddTemplateModule2]` and the obsolete `[AddTemplateModule]`
- [`../../TimberUi/TimberUi/DIAttributes/AttributeBinder.cs`](../../TimberUi/TimberUi/DIAttributes/AttributeBinder.cs) — assembly scanning and decorator collection
- [`../../TimberUi/TimberUi/Extensions/ConfiguratorHelpers.cs`](../../TimberUi/TimberUi/Extensions/ConfiguratorHelpers.cs) — `BindTemplateModule` extension methods on `Configurator`
- [`../../TimberUi/TimberUi/Helpers/ConfiguratorHelpers.cs`](../../TimberUi/TimberUi/Helpers/ConfiguratorHelpers.cs) — `TemplateModuleHelper.AddDecorator`

---

## 1. How template-module decoration works

When the game starts a session it calls every registered `TemplateModule` in sequence, each of which may call `AddDecorator<TSubject, TDecorator>()` on the template builder. The result is that every entity whose template contains `TSubject` also receives a freshly-instantiated `TDecorator` component. Decoration happens once at template-build time; the component is then present on every subsequent entity instance for the lifetime of the session.

From your mod's perspective:

1. Write a `BaseComponent` subclass — the **decorator** — that holds whatever state or logic you need.
2. Tell TimberUi which existing game component type — the **subject** — should trigger its injection.
3. Register the pair in a configurator for the **Game** context (or **MapEditor** if you also need it there).

The decorator receives full constructor injection from the DI container, so you can inject services, `ILoc`, or any other registered type exactly as you would in any other component.

> **Timberborn-provided types.** `BaseComponent`, `TemplateModule`, `Workplace`, `Dwelling`, and `Building` are all Timberborn game types, not TimberUi types. TimberUi provides only the registration plumbing described here.

---

## 2. The attribute path — `[AddTemplateModule2]`

### Write the decorator

```cs
// Injects a work-quota tracker onto every Workplace entity.
// Workplace is a Timberborn game component (not part of TimberUi).
[AddTemplateModule2(typeof(Workplace))]
public class WorkQuotaTracker(ILoc t) : BaseComponent
{
    int _dailyQuota;

    public void SetQuota(int quota) => _dailyQuota = quota;

    public int GetQuota() => _dailyQuota;
}
```

That single attribute is all the registration code you need. When `AttributeConfigurator` scans your assembly it discovers the attribute, records the `(Workplace → WorkQuotaTracker)` pair, and at the end of the scan calls `BindTemplateModule` with all collected pairs in a single batch.

### Attribute properties

| Property | Type | Default | Meaning |
|---|---|---|---|
| `Subject` (constructor arg) | `Type` | required | The game component type whose presence triggers injection of the decorator |
| `Contexts` | `BindAttributeContext` | `Game` | Which DI contexts this decoration is active in |
| `AlsoBindTransient` | `bool` | `true` | Also bind the decorator class itself as transient so it can be resolved by other DI consumers |

### Context filtering

Template-module attributes are only processed in a **NonMenu** context (`Game | MapEditor` — the attribute scanner skips this code path entirely in `MainMenu` and `Bootstrapper`). The `Contexts` property then further restricts within NonMenu:

- `Contexts = BindAttributeContext.Game` (default) — active in-game only; ignored in the map editor.
- `Contexts = BindAttributeContext.MapEditor` — active in the map editor only; ignored in-game.
- `Contexts = BindAttributeContext.NonMenu` — active in both the game and map-editor scenes.

To attach your decorator in both scenes:

```cs
[AddTemplateModule2(typeof(Workplace), Contexts = BindAttributeContext.NonMenu)]
public class WorkQuotaTracker(ILoc t) : BaseComponent
{
    // ...
}
```

### Auto-discovery requirement

`[AddTemplateModule2]` is only auto-discovered when your configurator inherits from `GameAttributeConfigurator` (or `MapEditorAttributeConfigurator`) and calls `base.Configure()` so the attribute scan runs. See [Dependency Injection](./DependencyInjection.MD) for the full configurator setup.

```cs
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator { }
```

No other code is needed — the empty subclass is sufficient.

### `AlsoBindTransient`

By default (`AlsoBindTransient = true`) the attribute scanner also calls `configurator.Bind(decoratorType).AsTransient()`, so other services can inject your decorator directly. If you are already binding the decorator type through a `[Bind…]` attribute or an explicit call in `Configure()`, set `AlsoBindTransient = false` to avoid the duplicate binding:

```cs
[BindSingleton]                                      // explicit binding
[AddTemplateModule2(typeof(Workplace), AlsoBindTransient = false)]
public class WorkQuotaTracker(ILoc t) : BaseComponent { /* ... */ }
```

---

## 3. The imperative path — `BindTemplateModule`

When attributes are not enough — conditional registration, types loaded at runtime, or mixing decorator wiring with other custom configurator logic — use the `BindTemplateModule` extension method on `Configurator`.

### One-call lambda form (recommended)

```cs
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure(); // attribute scan runs first

        // Imperatively add an extra decorator that attributes cannot cover,
        // for example because the condition depends on runtime configuration.
        this.BindTemplateModule(h => h
            .AddDecorator<Dwelling, DwellingCapacityTracker>()
            .AddDecorator<Workplace, WorkQuotaTracker>()
        );
    }
}
```

The lambda receives a `TemplateModuleHelper`, you call `AddDecorator` on it, return it, and `Bind()` is called for you automatically. The method itself returns `Configurator` so it chains with other calls.

### `TemplateModuleHelper.AddDecorator<TSubject, TDecorator>`

```cs
public TemplateModuleHelper AddDecorator<TSubject, TDecorator>(bool addTransient = true)
    where TDecorator : class
```

| Parameter | Default | Meaning |
|---|---|---|
| `TSubject` | — | The existing game component type to decorate |
| `TDecorator` | — | Your `BaseComponent` subclass to inject |
| `addTransient` | `true` | Also call `configurator.BindTransient<TDecorator>()` so the type is resolvable |

Pass `addTransient: false` if you are binding the decorator type separately:

```cs
this.BindTemplateModule(h => h
    .AddDecorator<Workplace, WorkQuotaTracker>(addTransient: false)
);
this.BindSingleton<WorkQuotaTracker>(); // bound separately as singleton
```

When the subject or decorator type is only known at runtime, use the non-generic overload that takes `Type` arguments instead:

```cs
public TemplateModuleHelper AddDecorator(Type subject, Type decorator, bool addTransient = true)
```

```cs
this.BindTemplateModule(h => h.AddDecorator(typeof(Workplace), myDecoratorType));
```

### Two-call form

When you need to conditionally add decorators before committing, obtain the helper explicitly and call `Bind()` yourself:

```cs
public override void Configure()
{
    base.Configure();

    var helper = this.BindTemplateModule();
    helper.AddDecorator<Workplace, WorkQuotaTracker>();

    if (SomeCondition)
    {
        helper.AddDecorator<Dwelling, DwellingCapacityTracker>();
    }

    helper.Bind(); // must be called exactly once
}
```

> **Call `Bind()` exactly once.** Each `Bind()` call registers a new `TemplateModule` provider, so calling it twice on the same helper registers duplicate modules. The lambda form of `BindTemplateModule` calls `Bind()` for you (and never hands you the helper), so prefer it whenever you don't need the two-call form — it makes the mistake impossible.

### When to prefer the imperative path

- You need to register decorators conditionally (feature flags, mod-compatibility checks).
- Your decorator type is determined at runtime.
- You want to keep all configurator wiring in one place and not scatter `[AddTemplateModule2]` attributes across the codebase.
- You need to mix decorator registration with other advanced wiring in the same `Configure()` override.

---

## 4. Complete worked example

The following shows a small mod that tracks a custom "priority score" on every `Building` entity. The decorator is registered with the attribute and exposes a simple property that an entity-panel fragment can read.

```cs
// MyMod/Decorators/BuildingPriorityScore.cs

/// <summary>
/// Decorator attached to every Building entity. Holds the player-assigned
/// priority score used by the mod's scheduling logic.
/// </summary>
/// <remarks>
/// Building is a Timberborn game component. This class is injected by
/// TimberUi's template-module mechanism — do not instantiate directly.
/// </remarks>
[AddTemplateModule2(typeof(Building))]
public class BuildingPriorityScore : BaseComponent
{
    /// <summary>Gets or sets the priority score for this building (0–100).</summary>
    public int Score { get; set; }
}
```

```cs
// MyMod/Configs/GameConfig.cs

/// <summary>Game-context configurator. Auto-discovers [AddTemplateModule2] attributes.</summary>
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator { }
```

At runtime, every entity that has a `Building` component will also receive a `BuildingPriorityScore` component. A fragment or service can retrieve it with `building.GetComponent<BuildingPriorityScore>()` (or via constructor injection if the decorator is resolved from the container directly).

### Adding map-editor support

If your mod has a map-editor configurator and the decorator should also be present there, change the attribute's `Contexts`:

```cs
[AddTemplateModule2(typeof(Building), Contexts = BindAttributeContext.NonMenu)]
public class BuildingPriorityScore : BaseComponent { /* ... */ }
```

Then add the matching configurator:

```cs
[Context("MapEditor")]
public class MyModMapEditorConfig : MapEditorAttributeConfigurator { }
```

---

## 5. Migration from `[AddTemplateModule]`

The original `[AddTemplateModule]` attribute is marked `[Obsolete]` and should not be used in new code. The behavioural difference is:

| Attribute | When active |
|---|---|
| `[AddTemplateModule]` | Only when the active context is exactly `Game` (the `isGame` flag). |
| `[AddTemplateModule2]` | Whenever the active context is `NonMenu` (`Game` or `MapEditor`), then filtered further by the attribute's own `Contexts` property. |

To migrate, replace the attribute name and add an explicit `Contexts` if you need a context other than the default `Game`:

```cs
// Before (obsolete):
[AddTemplateModule(typeof(Workplace))]
public class WorkQuotaTracker : BaseComponent { /* ... */ }

// After:
[AddTemplateModule2(typeof(Workplace))]           // Contexts defaults to Game — same behaviour
public class WorkQuotaTracker : BaseComponent { /* ... */ }
```

Both attributes share the same `AlsoBindTransient` property with the same default (`true`), so no change is needed there.

---

## Key types

| Type | Role | Source |
|---|---|---|
| `[AddTemplateModule2]` | Primary attribute for template-module decorator registration; properties `Subject`, `Contexts`, `AlsoBindTransient`. | [`../../TimberUi/TimberUi/DIAttributes/AddTemplateModuleAttribute.cs`](../../TimberUi/TimberUi/DIAttributes/AddTemplateModuleAttribute.cs) |
| `[AddTemplateModule]` | Obsolete predecessor; only active in the `Game` context; no `Contexts` property. | [`../../TimberUi/TimberUi/DIAttributes/AddTemplateModuleAttribute.cs`](../../TimberUi/TimberUi/DIAttributes/AddTemplateModuleAttribute.cs) |
| `BindAttributeContext` | `[Flags]` enum: `None`, `MainMenu (1)`, `Game (2)`, `MapEditor (4)`, `Bootstrapper (8)`, `All` (excludes Bootstrapper), `NonMenu = Game \| MapEditor`. | [`../../TimberUi/TimberUi/DIAttributes/BindAttribute.cs`](../../TimberUi/TimberUi/DIAttributes/BindAttribute.cs) |
| `AttributeBinder` | Static engine called by `AttributeConfigurator.Configure()`; collects all `[AddTemplateModule2]` entries and registers them via a single `BindTemplateModule` call. | [`../../TimberUi/TimberUi/DIAttributes/AttributeBinder.cs`](../../TimberUi/TimberUi/DIAttributes/AttributeBinder.cs) |
| `TemplateModuleHelper` | Fluent builder; obtained via `configurator.BindTemplateModule()`; `AddDecorator<TSubject, TDecorator>(bool addTransient = true)` accumulates pairs; `Bind()` commits them. | [`../../TimberUi/TimberUi/Helpers/ConfiguratorHelpers.cs`](../../TimberUi/TimberUi/Helpers/ConfiguratorHelpers.cs) |
| `BindTemplateModule(Func<…>)` | `Configurator` extension; passes a `TemplateModuleHelper` to a lambda and calls `Bind()` automatically. | [`../../TimberUi/TimberUi/Extensions/ConfiguratorHelpers.cs`](../../TimberUi/TimberUi/Extensions/ConfiguratorHelpers.cs) |
| `BindTemplateModule()` | `Configurator` extension; returns a bare `TemplateModuleHelper` for the two-call form; caller must call `Bind()`. | [`../../TimberUi/TimberUi/Extensions/ConfiguratorHelpers.cs`](../../TimberUi/TimberUi/Extensions/ConfiguratorHelpers.cs) |
| `GameAttributeConfigurator` | Base class for the Game-context configurator; `Configure()` runs the attribute scan. | [`../../TimberUi/TimberUi/AttributeConfigurator.cs`](../../TimberUi/TimberUi/AttributeConfigurator.cs) |
| `MapEditorAttributeConfigurator` | Base class for the MapEditor-context configurator; same scan, different context flag. | [`../../TimberUi/TimberUi/AttributeConfigurator.cs`](../../TimberUi/TimberUi/AttributeConfigurator.cs) |
