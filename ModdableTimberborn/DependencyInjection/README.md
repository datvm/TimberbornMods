# DependencyInjection

## Purpose

Provides the infrastructure for mods to intercept and modify Timberborn's data-loading pipeline at two points: the `SpecService` (component-spec blueprints) and the `TemplateCollectionService` (prefab templates). It introduces a hook-and-runner pattern so individual mod services can register modifications without patching game code directly, and supplies a few utility types (`AssetRefService`, `EditableBlueprint`) that support those modifications.

## Key types

- **`ModdableDependencyInjectionConfig`** — Bindito configurator that wires all runners and services for this subsystem. Also contains a partial extension to `ModdableTimberbornRegistry` for opt-in registration. Implements `IModdableTimberbornRegistryWithPatchConfig`, so it also activates the Harmony patches in the `Patches/` subfolder.
- **`EditableBlueprint`** — Mutable wrapper around an immutable `Blueprint`. Holds `Name`, `Children`, and `Specs` as editable lists; converts back to `Blueprint` via `ToBlueprint()` / implicit cast. The main currency passed between `ISpecModifier` and `ITemplateModifier` implementations.
- **`AssetRefService`** — Thin wrapper around `IAssetLoader` that lazily creates `AssetRef<T>` instances. Registered as a bootstrapper-context singleton so it is available very early in startup.
- **`ISpecModifier` / `BaseBlueprintModifier<T>`** — Interface and abstract base for modifying a batch of `EditableBlueprint`s keyed by `ComponentSpec` type. Supports `Order` and `ShouldRun` guards.
- **`BaseSpecModifier<T>`** — Convenience base that unpacks blueprints into `NamedSpec<T>` records so implementors only deal with named specs, not full blueprints. Use only when each blueprint has exactly one spec of type `T`.
- **`BaseSpecTransformer<T>`** — Convenience base that applies a single `Transform(T spec) -> T?` call per blueprint in-place; simpler than `BaseSpecModifier` when the blueprint structure does not change.
- **`SpecModifierService`** — `ISpecServiceTailRunner` that groups all registered `ISpecModifier`s by their target `ComponentSpec` type, then applies them in `Order` sequence after `SpecService.Load`.
- **`SpecServiceRunner`** — Coordinator (`IBlueprintModifierProvider`, `ILoadableSingleton`) that fires all `ISpecServiceTailRunner`s after `SpecService.Load`; also ensures `ISpecServiceFrontRunner`s are constructed (and thus their `ILoadableSingleton.Load` runs) before the spec load.
- **`ISpecServiceFrontRunner`** — Marker interface; services that implement it get their `Load()` called *before* `SpecService.Load`. Registration only; no additional method needed beyond `ILoadableSingleton`.
- **`ISpecServiceTailRunner`** — Services with a `Run(SpecService)` called *after* `SpecService.Load`.
- **`ITemplateCollectionServiceTailRunner`** — Services with a `Run(TemplateCollectionService)` called *after* `TemplateCollectionService.Load`. Supports `Order`.
- **`ITemplateModifier`** — Fine-grained interface for modifying individual templates; receives blueprint name, template name, and the original `TemplateSpec`. Must implement `ShouldModify` (gate) and `Modify` (transform, returning null for no change).
- **`TemplateCollectionTailRunnerService`** — Coordinator (`ITemplateCollectionIdProvider`) that fans out to all registered `ITemplateCollectionServiceTailRunner`s in order.
- **`TemplateModifierTailRunner`** — `ITemplateCollectionServiceTailRunner` that applies all `ITemplateModifier`s to `TemplateCollectionService.AllTemplates`, updates `blueprintSourceService` for changed templates, and replaces the collection in-place.

## How it fits together

`ModdableDependencyInjectionConfig.Configure` is the single entry point that registers everything. In the **bootstrapper context** it only registers `AssetRefService`. In all other contexts it registers the two coordinator chains:

1. **Spec chain**: `SpecServiceRunner` (multi-bound as `IBlueprintModifierProvider`) collects `ISpecServiceFrontRunner`s (loaded before spec load) and `ISpecServiceTailRunner`s (run after). The built-in tail runner `SpecModifierService` (registered via `BindSpecTailRunner`) then dispatches to all multi-bound `ISpecModifier`s. Consumers register their own `ISpecModifier` implementations with `configurator.BindSpecModifier<T>()` or `ISpecServiceTailRunner` implementations with `configurator.BindSpecTailRunner<T>()`.

2. **Template chain**: `TemplateCollectionTailRunnerService` (multi-bound as `ITemplateCollectionIdProvider`) collects `ITemplateCollectionServiceTailRunner`s in order. `TemplateModifierTailRunner` (registered via `BindTemplateTailRunner`) is the built-in runner that fans out to all `ITemplateModifier`s. Consumers register their modifiers with `configurator.BindTemplateModifier<T>()` or full runners with `configurator.BindTemplateTailRunner<T>()`.

The Harmony patches in `Patches/` trigger both chains: `SpecServicePatches` postfixes `SpecService.Load` to invoke `SpecServiceRunner.OnSpecLoaded`, and `TemplateCollectionServicePatches` postfixes `TemplateCollectionService.Load` to invoke `TemplateCollectionTailRunnerService.Run`. Both patches are gated by `ModdableDependencyInjectionConfig.PatchCategoryName` so they only activate when this config is registered.

Mod authors opt in either by implementing `IWithDIConfig` on their configurator class (preferred) or by calling `registry.UseDependencyInjection()` (deprecated).

## Dependencies & patterns

- **Bindito (DI)**: `[MultiBind]` / `configurator.MultiBindSingleton` for collecting open-ended lists of `ISpecModifier`, `ISpecServiceTailRunner`, `ITemplateModifier`, and `ITemplateCollectionServiceTailRunner`. Singletons throughout.
- **Harmony**: Two postfix patches (`SpecServicePatches`, `TemplateCollectionServicePatches`). Both use `ContainerRetriever` to pull services out of the DI container at patch time. Both are in category `ModdableTimberborn.DependencyInjection`.
- **Timberborn game types**: `SpecService`, `TemplateCollectionService`, `Blueprint`, `ComponentSpec`, `TemplateSpec`, `BlueprintAsset`, `IAssetLoader`, `BlueprintSourceService`, `ITemplateCollectionIdProvider`, `IBlueprintModifierProvider`.
- `FrozenDictionary` and `ImmutableArray` used in `SpecModifierService` and the runner services for read-only, allocation-minimal dispatch after load.

## Notes / gotchas

- `SpecServiceRunner.Load()` is intentionally empty; the `ILoadableSingleton` implementation exists only to force construction (and therefore front-runner loading) at the right time. The `#pragma warning disable CS9113` suppressing "parameter is unread" on `frontRunners` is a deliberate DI side-effect trick.
- `UseDependencyInjection()` on `ModdableTimberbornRegistry` is `[Obsolete]`; the idiomatic replacement is adding `IWithDIConfig` to the mod's own config class.
