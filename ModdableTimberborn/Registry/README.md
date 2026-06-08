# Registry

## Purpose

This subsystem is the central DI configurator hub for ModdableTimberborn. It maintains a singleton registry of all mod configurators, routes Bindito `Configurator.Configure()` calls to the right registered configs based on game lifecycle context (Bootstrapper / MainMenu / Game / MapEditor), and triggers Harmony patching when a config with patches is registered.

## Key Types

- **`ModdableTimberbornRegistry`** (partial singleton) — the core registry. Holds all `IModdableTimberbornRegistryConfig` instances, dispatches `Configure()` calls per context, and delegates Harmony patching. The partial class is extended in `DependencyInjection/ModdableDependencyInjectionConfig.cs` to add opt-in DI support.
- **`ConfigurationContext`** (`[Flags]` enum) — the four lifecycle contexts (`Bootstrapper`, `MainMenu`, `Game`, `MapEditor`) plus convenience combos (`All`, `None`, `NonMenu`). Used to gate which configurators run in which scene.
- **`IModdableTimberbornRegistryConfig`** — the base interface every configurator must implement. Declares `AvailableContexts` (defaults to `All`) and `Configure(Configurator, ConfigurationContext)`.
- **`IModdableTimberbornRegistryWithPatchConfig`** — extends the base interface for configurators that also need Harmony patching. `PatchCategory` selects a named category; `null` patches the whole assembly.
- **`IWithDIConfig`** — marker interface; implementing it on a config class automatically activates the ModdableTimberborn DI subsystem (calls `InternalUseDependencyInjection()`).
- **`BaseModdableTimberbornConfiguration`** — abstract base for mod entry-point configs. Implements `IModStarter`; `StartMod` registers `this` with the registry and optionally enables DI if the subclass also implements `IWithDIConfig`.
- **`BaseModdableTimberbornConfigurationWithHarmony`** — extends the above for configs that also patch. Overridable `PatchCategory` (defaults to `null` → patch whole assembly).
- **`BaseModdableTimberbornAttributeConfiguration`** — convenience base that implements `Configure()` by calling `configurator.BindAttributes(context, assembly, defaultScope)`, so subclasses only need to declare `AvailableContexts` and optionally override `Assembly` / `DefaultScope`.

## How It Fits Together

`ModdableTimberbornRegistry.Instance` is a static singleton created at class-initialisation time. The four Bindito configurator classes in `MConfigs.cs` (`ModBootstrapperConfig`, `ModMainMenuConfig`, `ModGameConfig`, `ModMapEditorConfig`) are annotated with Timberborn's `[Context(...)]` attribute; Timberborn calls `Configure()` on each at the appropriate game lifecycle stage, which simply delegates to `ModdableTimberbornRegistry.Instance.Configure(this, <context>)`.

Mods plug in by subclassing `BaseModdableTimberbornConfiguration` (or one of its variants) and implementing `IModStarter`. When Timberborn calls `StartMod`, the base class registers the config instance with the registry. From that point on, whenever the matching context fires, the registry calls `config.Configure(configurator, context)` and the mod's DI bindings are applied.

Harmony patching is also driven through the registry: `AddConfigurator` checks whether the config implements `IModdableTimberbornRegistryWithPatchConfig` and, if so, calls either `harmony.PatchCategory(category)` or `harmony.PatchAll(assembly)` immediately upon registration. Uncategorized patches are applied at startup via `MStarter` → `ConfigureStarter()` → `harmony.PatchAllUncategorized()`.

`ModdableTimberbornUtils.CurrentContext` is updated by the registry just before each dispatch loop so other code can read the active context at runtime.

## Dependencies & Patterns

- **Bindito (DI):** All `Configure` methods receive a Bindito `Configurator`. Context mapping uses `ConfigurationContext.ToBindAttributeContext()` (extension in `Helpers/ConfigurationExtensions.cs`) to bridge to Bindito's `BindAttributeContext`.
- **Harmony:** A single `Harmony` instance named `"ModdableTimberborn"` is held on `ModdableTimberbornRegistry`. Patching is idempotent per category via `PatchedCategories` — a category is only patched once even if multiple configurators share it.
- **`IModStarter`:** Timberborn's mod entry-point hook. `BaseModdableTimberbornConfiguration` implements it so mods self-register without needing to know about `MConfigs.cs`.
- **DI opt-in pattern:** Implementing `IWithDIConfig` on a config class (no members required) triggers `InternalUseDependencyInjection()`, which registers `ModdableDependencyInjectionConfig` — a guard (`DependencyInjectionUsed` flag) ensures it is only added once. The `UseDependencyInjection()` fluent method exists but is marked `[Obsolete]`.

## Notes / Gotchas

- `ModdableTimberbornRegistry` is `partial`; the DI-related members (`DependencyInjectionUsed`, `InternalUseDependencyInjection`, `UseDependencyInjection`) live in `DependencyInjection/ModdableDependencyInjectionConfig.cs`, not in the `Registry/` folder. Searching for the full class requires looking outside this folder.
- `AddDefaultConfigurators()` always registers `ModdableEntityDescriberConfigurator` and `ModdableTimberbornConfigurator` at construction time; there is no way to opt out.
- `IWithDIConfig` has no members — it is purely a marker interface. Implementing it on a config class activates the ModdableTimberborn DI subsystem; omitting it leaves the DI subsystem inactive for that config.
