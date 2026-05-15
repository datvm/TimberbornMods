# Moddable Weathers Design Summary

Moddable Weathers is a Timberborn mod that replaces the game's fixed temperate/drought/badtide flow with generated weather cycles made from configurable weather stages and optional modifiers. It depends on ModdableTimberborn and uses Bindito service registration plus Harmony-based replacement patches.

## Startup and Binding

`MConfigs.cs` is the composition root. `ModdableWeatherConfig` registers the mod in both bootstrapper and game contexts, applies attribute-driven Harmony patches at startup, removes or replaces selected vanilla services, and binds the core registries, cycle services, built-in weathers, built-in modifiers, UI panels, settings panels, and component decorators.

Replacement classes use attributes from `Patches/` such as `ReplaceSingleton`, `ReplaceMethod`, `ReplaceProperty`, `BypassMethods`, and `ThrowMethods`. `AttributePatcher` scans the assembly and redirects vanilla members to modded implementations.

## Weather Model

Weather implementations live under `Weathers/`. `IModdableWeather` defines the common contract: id, spec, active/enabled state, lifecycle events, start/end behavior, chance selection, and duration selection. Benign and hazardous weathers are separated with marker interfaces.

`ModdableWeatherBase` handles shared lifecycle state and loads display/assets from `ModdableWeatherSpecService`. Weather specs are blueprint assets under `Root/ModdableWeatherSpecs/` and provide localized display text, progress images, notification art, icons, and optional sounds.

The mod wraps existing game weathers with `GameBuiltIns/` and adds custom built-ins in `ModBuiltIns/`, including Rain, Monsoon, and Surprisingly Refreshing.

## Weather Modifiers

Weather modifiers live under `WeatherModifiers/`. `IModdableWeatherModifier` defines a second layer that can attach effects to a selected weather stage. Modifiers have specs, settings, lifecycle events, chance logic, and optional decision mutation through `IModdableWeatherDecisionModifier`.

`ModdableWeatherModifierBase<TSettings>` loads modifier specs and settings, initializes compatible-weather settings, blocks incompatible modifiers, and calculates whether a modifier can apply to the chosen weather. Modifier specs are blueprint assets under `Root/ModdableWeatherModifierSpecs/`.

Built-in modifiers include drought, badtide, rain, monsoon, and refreshing effects. For example, `RainModifier` starts visual rain, moistens land, blocks contamination, and applies soak effects while active.

## Cycle Generation and History

`WeatherGenerator` builds generated `WeatherCycle` data one cycle ahead. It chooses stage definitions, rolls whether each stage is benign or hazardous, selects a weather by weighted chance, calculates duration, and rolls compatible modifiers. Empty fallback weathers ensure every cycle has at least one valid stage.

`WeatherCycleService` is the runtime controller. It keeps current, next, and tomorrow stage references, updates the game's cycle duration, transitions weather and modifiers on cycle-day events, and emits weather transition events.

`WeatherHistoryRegistry` stores generated cycle data, while `WeatherHistoryService` converts stored ids back into detailed weather/modifier objects, maintains previous/current/next cycle references, and counts weather or modifier occurrences for chance logic.

## Game Effects

Services in `Services/` adapt generated weather state back into Timberborn systems. `ModdableWeatherService` replaces the vanilla `WeatherService` surface, while hazardous weather, approaching timers, temperate duration, water strength, water contamination, land moisture, contamination blocking, and soak application are handled by focused services.

Component decorators in `Components/` attach behavior to game entities. Water source decorators alter strength and contamination, replacement components disable vanilla drought/badtide behavior where needed, and `WeatherSoakEffectApplier` applies soaked effects to outdoor beavers when rain-like modifiers are active.

## UI and Settings

`UI/` replaces or extends weather-facing UI such as the date panel, weather panel, weather sound player, game music player, and hazardous notification panel. The weather panel uses current and next `ModdableWeatherSpec` assets for progress backgrounds and displays active modifier effects.

`UI/Settings/` contains the in-game settings dialog. It exposes global settings, general weather settings, per-weather settings, modifier settings, weather/modifier association controls, cycle-stage controls, and export support.

## Assets and Localization

`Root/` contains blueprint specs, localization, sounds, and weather images. These assets are referenced by weather and modifier specs, then loaded into runtime services and UI panels through Timberborn's asset and localization systems.

