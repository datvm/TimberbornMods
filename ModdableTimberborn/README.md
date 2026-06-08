# ModdableTimberborn

## Purpose

ModdableTimberborn (mod id `ModdableTimberborn`, version 10.7.0) is a shared modding-support library for Timberborn (minimum game version 1.0.5). It exposes APIs — entity stats, bonus systems, mechanical system hooks, serialization helpers, and more — that other mods depend on rather than re-implementing. Consuming mods reference the DLL and call `ModdableTimberbornRegistry.Instance.AddConfigurator<T>()` at startup to register their own Bindito bindings and Harmony patches.

## Entry Points / Lifecycle

### `MStarter : IModStarter`
The Timberborn mod loader calls `IModStarter.StartMod(IModEnvironment)` once at boot. The implementation just calls `ModdableTimberbornRegistry.Instance.ConfigureStarter()`, which re-applies `harmony.PatchAllUncategorized()`. (Harmony is also initialised in the `ModdableTimberbornRegistry` static constructor, so patches that are categorised fire earlier via `PatchAllUncategorized` in the constructor itself.)

### `MConfigs.cs` — four `Configurator` classes
One class per Bindito DI context (`Bootstrapper`, `MainMenu`, `Game`, `MapEditor`). Each is decorated with `[Context("...")]` so Timberborn's DI wiring discovers them automatically. All four simply delegate to `ModdableTimberbornRegistry.Instance.Configure(this, context)`, which iterates the registered `IModdableTimberbornRegistryConfig` objects that opted into that context and calls `Configure(configurator, context)` on each.

### `ModdableTimberbornConfigurator : IModdableTimberbornRegistryConfig`
The library's own default DI configurator. It opts into `Game | MapEditor` (i.e., `ConfigurationContext.NonMenu`) and calls `configurator.BindAttributes(context)`, which wires all attribute-annotated types discovered in those contexts. Registered as a default by the registry alongside `ModdableEntityDescriberConfigurator`.

### `ModdableTimberbornRegistry` (in `Registry/`)
A static singleton. Manages two collections: a flat set of all registered `IModdableTimberbornRegistryConfig` objects and a per-context list for fast dispatch. On `AddConfigurator`, if the config also implements `IModdableTimberbornRegistryWithPatchConfig`, its Harmony patches are applied immediately (either by category or by whole assembly). The four `MConfig` classes call `Configure(configurator, context)` during Bindito scene setup; `ConfigureStarter()` is called once by `MStarter`.

### Harmony integration
A single `Harmony` instance named `"ModdableTimberborn"` is created in the static constructor. It immediately calls `PatchAllUncategorized()`. Categorised patches are applied lazily when a configurator that owns them is added; uncategorized patches are applied at both construction and `ConfigureStarter`.

## Build / Project Notes

- **Target framework:** `netstandard2.1`, `LangVersion: preview`, nullable enabled.
- **Version conditionality:** The shared `Common.targets` supports two Timberborn release streams: `version-0.7` (U7, `TIMBER7` constant) and `version-1.0` (V1, `TIMBERV1` constant). Files under `V1\` / `*.V1.cs` or `U7\` / `*.U7.cs` are compiled selectively. Currently set to `version-1.0`.
- **Publicized game assemblies:** `GameAssemblyPublicizer\out\common\**\*.dll` — `AllowUnsafeBlocks` is required for the `IgnoresAccessChecks` trick that bypasses member-visibility enforcement at runtime.
- **Harmony:** referenced from the Steam Workshop mod folder (`3284904751\0Harmony.dll`), declared as a `RequiredMods` entry in the manifest.
- **TimberUi:** referenced from `GameModsFolder\TimberUi\version-1.0\**\*.dll`, also a `RequiredMods` dependency (minimum 10.1.5).
- **Bindito:** pulled in via the publicized game assemblies (no separate NuGet package).
- **ModAnalyzers:** a source generator / Roslyn analyzer from the game solution; currently the project reference is commented out and the pre-built `.dll` is used directly.
- **Internals visible to** `ModdableTimberborn.Tests`.
- **Post-build copy:** `Common.targets` copies the output DLL, `manifest.json`, localization files, assets, etc. straight into `GameModsFolder\ModdableTimberborn\version-1.0\` — no separate deployment step needed.

## Subsystems (Table of Contents)

Each subsystem has its own README under `.notes/ModdableTimberborn/<FolderName>/README.md`.

- **Areas** — geographic/zone area definitions and helpers used by other subsystems.
- **BonusSystem** — generic value-bonus/modifier pipeline; provides typed modifier collections consumed by mechanical, entity-stat, and other systems.
- **BuildingSettings** — exposes per-building configurable settings (UI and data) so mods can add settings panels to buildings.
- **Common** — shared primitives: `ValueModifierCollection`, utility extension methods, and base types used across the whole library.
- **CompatWeatherService** — compatibility shim / wrapper around the game's weather/drought service for cross-version safety.
- **DependencyInjection** — Bindito DI helpers: attribute scanning, configurator base classes, and `BindAttributes` extension that drives automatic binding.
- **EnterableSystem** — hooks into the game's enterable-building system so mods can intercept or extend enter/exit logic.
- **EntityDescribers** — the `IModdableEntityDescriber` pattern; lets mods attach descriptor fragments to entities without subclassing game types.
- **EntityTracker** — generic entity-tracking service; maintains live collections of entities matching a given predicate or type, used by other subsystems for efficient queries.
- **GameModes** — abstractions for game mode detection (e.g., sandbox vs. campaign), used to gate behaviour per mode.
- **GameStats** — global game-level statistics tracking (resources produced, buildings placed, etc.) exposed for mod consumption.
- **Helpers** — miscellaneous utility classes and extension methods that don't belong to a specific subsystem.
- **Localizations** — helpers for registering and resolving localization keys from mod-supplied `.txt`/`.po` files.
- **MechanicalSystem** — moddable wrappers around Timberborn's mechanical-power network: power producers, consumers, and the `MechanicalNodeModifierCollection` alias defined in `zGlobalUsings.cs`.
- **OptionalMods** — infrastructure for declaring soft/optional dependencies on other mods; loads integration code only when a target mod is present.
- **Patches** — Harmony patch classes that apply game-level fixes or hooks needed by the library itself (not by individual subsystems).
- **Registry** — core registry infrastructure: `ModdableTimberbornRegistry` singleton, `ConfigurationContext` flags enum, `IModdableTimberbornRegistryConfig` interface, and base configurator classes.
- **SerializationSystem** — helpers for serializing/deserializing custom mod data with Timberborn's save/load pipeline.
- **Services** — general-purpose service base classes and interfaces (e.g., tick listeners, singleton service helpers).
- **SoakEffect** — support for the game's water-soak effect on entities; allows mods to attach soak-related modifiers or reactions.
- **UpdatableEntityStats** — per-entity stat values that update each tick; integrates with `BonusSystem` so modifiers are applied automatically. See also the dedicated sub-tree note.
- **WorkSystem** — moddable extensions to the worker/work-place system: work speed, efficiency modifiers, and related hooks.

## Notes / Gotchas

- **`zGlobalUsings.cs`** is prefixed with `z` so it sorts last in editors; it contains one non-trivial line — a `global using` alias for `MechanicalNodeModifierCollection` that parameterises the generic `ValueModifierCollection` with the mechanical-node types. If that alias is missing from the compilation the whole `MechanicalSystem` subsystem breaks.
- **`ModdableTimberbornRegistry` is a static singleton initialised at class-load time.** Consuming mods must call `AddConfigurator` in their own `IModStarter.StartMod`, before Bindito scenes are constructed.
- **The `ModPath` and `AssemblyPath` in `CommonProperties.targets` are machine-local paths** that must be overridden when building on a different machine (typically via a `Directory.Build.props` or a local override targets file not tracked in git).
- **`workshop_data.json` is present** alongside `manifest.json`; it is copied one level above the mod folder (i.e., next to the mod directory, not inside it) — this is how the Timberborn mod uploader expects it.
