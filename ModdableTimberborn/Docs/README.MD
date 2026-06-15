# Moddable Timberborn

**ModdableTimberborn** is a foundation library that other Timberborn mods build on. It wraps common but awkward modding tasks — modifying specs and prefabs at load time, adding persistent per-building settings, tracking entities, attaching runtime buffs, exposing game stats — behind small, stable APIs so your mod never has to depend on game internals directly.

Each subsystem is **opt-in**: you enable only the ones you use. See [the Demo project](../../ModdableTimberbornDemo/) for runnable examples of most features.

## Getting started

### 1. Depend on the mod

Add ModdableTimberborn as a dependency in your mod's `manifest.json` (see the demo's [`manifest.json`](../../ModdableTimberbornDemo/manifest.json)) and reference the assembly from your `.csproj`. Also include ModdableTimberborn's own transitive requirements — `Harmony` and `TimberUi` — in your `RequiredMods` list.

### 2. Create your mod's configuration

Your mod's entry point is a class that derives from **`BaseModdableTimberbornConfiguration`** — or **`BaseModdableTimberbornConfigurationWithHarmony`** if your mod also applies Harmony patches. This class *is* your `IModStarter`: it registers itself with `ModdableTimberbornRegistry` automatically, so you don't write a separate starter. Implement `AvailableContexts` (the Bindito context(s) your `Configure` runs in) and `Configure`:

```cs
public class MyModConfig : BaseModdableTimberbornConfiguration
{
    // Which Bindito context(s) Configure runs in. This is a [Flags] value —
    // combine them, e.g. ConfigurationContext.Game | ConfigurationContext.MapEditor.
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindFragment<MyEntityPanelFragment>()
            .BindTemplateModule(h => h
                .AddDecorator<Enterable, MyBuffComponent>()
            )
        ;
    }
}
```

`Configure` runs once per matching context, using the standard Timberborn/`Configurator` API for your bindings.

> Prefer the base class over hand-rolling an `IModStarter` plus a plain `IModdableTimberbornRegistryConfig`. (Registering a plain config manually via `ModdableTimberbornRegistry.Instance.AddConfigurator<T>()` still works, but the base class is the supported pattern — and is required for the `IWithDIConfig` shortcut below.)

### 3. Enable the subsystems you need

Each built-in subsystem is **opt-in** — turn on only the ones you use:

- **Spec & template modifiers:** implement the `IWithDIConfig` marker interface on your config class. The base class detects it and enables the subsystem for you — nothing else needed:

  ```cs
  public class MyModConfig : BaseModdableTimberbornConfiguration, IWithDIConfig { /* ... */ }
  ```

- **Every other subsystem:** override `StartMod`, call `base.StartMod(...)` first, then chain the `Use…()` enablers on the registry (they're chainable and idempotent):

  ```cs
  public override void StartMod(IModEnvironment modEnvironment)
  {
      base.StartMod(modEnvironment);   // registers this config

      ModdableTimberbornRegistry.Instance
          .UseBuildingSettings()
          .UseEntityTracker()
          .UseGameStats();
  }
  ```

The available enablers:

| Enabler | Subsystem | Guide |
|---|---|---|
| `UseBuildingSettings()` | Persistent per-building settings | [Building Settings](./BuildingSettings.MD) |
| `UseEntityTracker()` | Live typed sets of characters/workplaces | [Entity Tracking & Stats](./EntityTrackingAndStats.MD) |
| `UseUpdatableEntityStats()` | Observable per-entity UI stats | [Entity Tracking & Stats](./EntityTrackingAndStats.MD) |
| `UseGameStats()` | String-keyed global game stats | [Game Stats](./GameStats.MD) |
| `UseBonusTracker()` / `UsePersistentBonusTracker()` | Named bonus-multiplier groups on entities | [Bonus System](./BonusSystem.MD) |
| `UseMechanicalSystem()` | Runtime-moddable power input/output | [Mechanical System](./MechanicalSystem.MD) |
| `UseSoakEffect()` | Per-character water-submersion ticks | [Effect Components](./EffectComponents.MD) |
| `UseAreaApis()` | Spatial zone (area) tracking | [Areas](./Areas.MD) |
| `IWithDIConfig` (marker interface) | Spec & template (prefab) modifiers — auto-enabled by the base class. (`UseDependencyInjection()` still works but is `[Obsolete]`.) | [Spec & Template Modifiers](./SpecAndTemplateModifiers.MD) |

## Subsystem guides

- [Building Settings](./BuildingSettings.MD) — add persistent, copy/paste-able settings to buildings.
- [Entity Tracking & Stats](./EntityTrackingAndStats.MD) — query live sets of characters/workplaces and expose observable stats to the UI.
- [Bonus System](./BonusSystem.MD) — attach, replace, and persist named groups of bonus multipliers.
- [Game Stats](./GameStats.MD) — read (and provide) string-keyed global statistics.
- [Mechanical System](./MechanicalSystem.MD) — modify a building's nominal power at runtime.
- [Spec & Template Modifiers](./SpecAndTemplateModifiers.MD) — rewrite component specs and prefab templates as the game loads them.
- [Effect Components](./EffectComponents.MD) — togglable buffs applied while a beaver enters a building, works a job, or is soaked.
- [Areas](./Areas.MD) — track which characters are inside user-defined zones.
- [Utilities](./Utilities.MD) — JSON serialization, optional-mod loading, weather compatibility, and assorted services.
