# DependencyInjection/TemplateCollection

## Purpose

Defines the interfaces and services for modifying Timberborn's prefab templates (the `TemplateCollectionService`) after they are loaded. Mirrors the Specs pipeline but targets template blueprints identified by `TemplateSpec`.

## Key types

- **`ITemplateCollectionServiceTailRunner`** — Interface with `Run(TemplateCollectionService)` and an `Order` priority. Services implementing this are called after `TemplateCollectionService.Load`.
- **`ITemplateModifier`** — Fine-grained interface for per-template modifications. Provides `Order`, a `ShouldModify(blueprintName, templateName, originalTemplateSpec)` gate, and `Modify(EditableBlueprint, TemplateSpec, Blueprint) -> EditableBlueprint?` (return null for no change).
- **`TemplateCollectionTailRunnerService`** — `ITemplateCollectionIdProvider` coordinator that collects all `ITemplateCollectionServiceTailRunner`s (sorted by `Order`) and dispatches to them. Also implements the `GetPrefabGroups` and `GetTemplateCollectionIds` stubs (both return empty).
- **`TemplateModifierTailRunner`** — `ITemplateCollectionServiceTailRunner` that iterates `TemplateCollectionService.AllTemplates`, applies all registered `ITemplateModifier`s, records blueprint-source metadata for changed templates via `BlueprintSourceService`, and replaces `AllTemplates` in-place if any changes were made.

## How it fits together

Mod authors implement `ITemplateModifier` and register with `configurator.BindTemplateModifier<T>()`. `TemplateModifierTailRunner` (registered via `BindTemplateTailRunner`) collects all modifiers and applies them in `Order` sequence. `TemplateCollectionTailRunnerService` coordinates all tail runners (including `TemplateModifierTailRunner`) and is triggered by the Harmony postfix in `Patches/TemplateCollectionServicePatches.cs`.

For broader control over the full template collection (not per-template), a mod can implement `ITemplateCollectionServiceTailRunner` directly and register with `BindTemplateTailRunner`.

## Dependencies & patterns

- **Bindito multi-bind**: Both `ITemplateCollectionServiceTailRunner` and `ITemplateModifier` are collected as `IEnumerable<T>`.
- **`BlueprintSourceService`**: `TemplateModifierTailRunner` calls `blueprintSourceService.Get(original).AddJson("{}", TypeName)` to register a source record for each modified template, maintaining Timberborn's blueprint provenance tracking.
- **`ImmutableArray`**: Both runner services freeze their injected collections at construction time.
- **`TemplateSpec`**: The gate for `ITemplateModifier`; only blueprints that have a `TemplateSpec` are considered.

## Notes / gotchas

- `TemplateModifierTailRunner` uses the type's own `FullName` as a source tag string (`TypeName = typeof(TemplateModifierTailRunner).FullName`); this is a static field so it is computed once at class load.
- Passing `"{}"` as the JSON content to `AddJson` registers a source entry so Timberborn knows the blueprint was modified, but does not record actual diff data.
