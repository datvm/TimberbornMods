# Moddable Timberborn

**ModdableTimberborn** is a foundation library that other Timberborn mods build on. It wraps common but awkward modding tasks — modifying specs and prefabs at load time, adding persistent per-building settings, tracking entities, attaching runtime buffs, exposing game stats — behind small, stable APIs so your mod never has to depend on game internals directly.

Each subsystem is **opt-in**: you enable only the ones you use. See [the Demo project](../../ModdableTimberbornDemo/) for runnable examples of most features.

## Getting started

### 1. Depend on the mod

Add ModdableTimberborn as a dependency in your mod's `manifest.json` (see the demo's [`manifest.json`](../../ModdableTimberbornDemo/manifest.json)) and reference the assembly from your `.csproj`. Also include ModdableTimberborn's own transitive requirements — `Harmony` and `TimberUi` — in your `RequiredMods` list.

### 2. Enable the subsystems you need

Everything is registered through the singleton `ModdableTimberbornRegistry.Instance`, from your `IModStarter`:

```cs
public class MStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            // Turn on the built-in subsystems you use:
            .UseBuildingSettings()
            .UseEntityTracker()
            .UseGameStats()

            // Register your own configurators:
            .AddConfigurator<MyGameConfig>()
        ;
    }
}
```

The `Use…()` calls turn on a built-in subsystem (they're chainable and idempotent). `AddConfigurator<T>()` registers *your* code; `T` must have a parameterless constructor (`where T : IModdableTimberbornRegistryConfig, new()`). The full set of enablers:

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
| `IWithDIConfig` (on a `BaseModdableTimberbornConfiguration` subclass) | Spec & template (prefab) modifiers — auto-activates when a config deriving from `BaseModdableTimberbornConfiguration` implements `IWithDIConfig` (see note below). (`UseDependencyInjection()` still works but is `[Obsolete]`.) | [Spec & Template Modifiers](./SpecAndTemplateModifiers.MD) |

### 3. Write a configurator

Your configurator implements `IModdableTimberbornRegistryConfig`. It's invoked once per Bindito **context** (Bootstrapper, MainMenu, Game, MapEditor) — guard for the context you want and bind your services with the standard Timberborn/`Configurator` API:

```cs
public class MyGameConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindFragment<MyEntityPanelFragment>()
            .BindTemplateModule(h => h
                .AddDecorator<Enterable, MyBuffComponent>()
            )
        ;
    }
}
```

> **Heads-up — `IWithDIConfig` needs the base class.** The plain-interface configurator above is fine for binding, but the `IWithDIConfig` opt-in (for [Spec & Template Modifiers](./SpecAndTemplateModifiers.MD)) is only detected when your config derives from `BaseModdableTimberbornConfiguration`. That base type doubles as your `IModStarter`: it auto-registers itself and, when it implements `IWithDIConfig`, enables the DI subsystem for you — so you don't need the separate `MStarter` from step 2. Adding `IWithDIConfig` to a *plain* `IModdableTimberbornRegistryConfig` (as in the example above) has no effect.

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
