# DependencyInjection/Patches

## Purpose

Harmony postfix patches that hook into Timberborn's two main data-loading methods and forward control to ModdableTimberborn's runner services. These patches are the bridge between the game's load sequence and the mod's modification pipeline.

## Key types

- **`SpecServicePatches`** — Postfixes `SpecService.Load`; retrieves `SpecServiceRunner` from the DI container and calls `OnSpecLoaded`.
- **`TemplateCollectionServicePatches`** — Postfixes `TemplateCollectionService.Load` at `HarmonyPriority.First`; retrieves `TemplateCollectionTailRunnerService` and calls `Run`.

## How it fits together

Both patches are decorated with `[HarmonyPatchCategory(ModdableDependencyInjectionConfig.PatchCategoryName)]` so they are only activated when `ModdableDependencyInjectionConfig` is registered (i.e., when the DependencyInjection subsystem is in use). They use `ContainerRetriever` to pull the relevant service from the active DI container at runtime, avoiding any tight compile-time coupling.

## Dependencies & patterns

- **Harmony**: Standard postfix attribute-based patches. `TemplateCollectionServicePatches` additionally sets `HarmonyPriority.First` to ensure it runs before any other mods that might also patch `TemplateCollectionService.Load`.
- **`ContainerRetriever`**: Timberborn/Bindito utility for accessing the DI container from static Harmony patch methods.

## Notes / gotchas

- No cleanup / unpatch is performed explicitly; patch lifecycle is managed by the Harmony category registration in `ModdableDependencyInjectionConfig`.
