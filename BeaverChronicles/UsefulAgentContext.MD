# Useful Agent Context

This file is a living handoff note for AI agents working in this `BeaverChronicles` project. Keep it concise and update it when durable project context changes.

## Project Shape

- This is a Timberborn mod project under `BeaverChronicles`.
- JSON event blueprints live under `Root/Blueprints/ChronicleEvents`.
- English localization strings live in `Localizations/enUS.csv`.
- Spec node data records live under `Specs/NodeData`.
- Spec node handlers live under `Services/SpecNodes`.
- Event condition evaluators live under `Services/Conditions`.

## Docs

- Project docs live in `Docs`; the shared repo-root `../Docs/README.MD` links back to them.
- `Docs/README.MD` is a non-technical JSON event authoring guide; `Docs/References.MD` is intended to catalog conditions and handler nodes.
- `Docs/EventGuides.MD` is the continuation page for more event examples and JSON authoring guides.
- `Docs/EventGuides.MD` covers all base JSON events in teaching order: FirstDeath, FirstChildGrowth, DroughtPrep, Unhoused, ContaminationConspiracy, ContaminationCure/Pasteur's Gamble, FoodWoodDispute1, and FoodWoodDispute2.
- `Docs/README.MD` includes a "Text Placeholders" section covering Beaver Chronicles `{S1}`/`{CP_Name}`/`{%1}` placeholders and Timberborn mixed-text tags like `[[C]]`, `[[+]]`, `[[good:...]]`, `[[time:...]]`, and `[[icon:...]]`.
- Beaver Chronicles docs link to the shared ModdableTimberborn game stat list at `../Docs/ModdableTimberborn/GameStats.MD#built-in-stat-quick-reference` instead of duplicating stat IDs.
- Docs may use GitHub-style callout boxes such as `> [!INFO]` when a note is helpful.

## Verification

- `dotnet build` from this folder is the quickest verification command and currently works for normal code checks.
- `SpecChronicleEventProvider.ValidateSpec` checks supported node/condition types and some TimeLimit rules, but it does not currently validate graph references such as `StartNodeId`, `NextNodeId`, branch node IDs, localization keys, or image paths.

## Style Notes

- Use braces for all `if`, loop, and similar statement bodies.
- Prefer concise modern C# patterns already used here: collection expressions (`[]`), primary constructors, pattern matching, and init-only properties.
- Project code usually omits explicit access modifiers for lowest visibility.
- Avoid field names prefixed with `_`.
- Use `apply_patch` for manual edits.

## Event Data Notes

- `SpecChronicleEventController.GetEntities(IEnumerable<string>)` can resolve:
  - A custom-parameter array prefix using `<Name>_Count` and `<Name>_1`, `<Name>_2`, etc.
  - A custom-parameter key whose value is an entity GUID.
  - A raw entity GUID string.
- While the event is inactive during trigger-weight evaluation, `SpecChronicleEventController.GetEntities(...)` ignores input IDs and instead resolves the root trigger entity from parameters such as `CharacterParameters` or `BuildingInstanceParameters`.
- `FindEntities` node writes arrays in the same custom-parameter format:
  - `<ResultName>_Count`
  - `<ResultName>_1`, `<ResultName>_2`, ...
  - `<ResultName>` as the first ID only when exactly one entity is found.
- `FindEntities.ChooseRandom` randomizes matching entities before applying `MaxCount`.
- `CharacterStatus` node can remove or inflict a need on characters resolved from `EntityIds`; Pasteur's Gamble uses it for `BadwaterContamination`.
- `SetCustomParameter` has a `Not` operation that treats true-like values as true and stores `"0"`; other values store `"1"`.

## Helper Notes

- Shared entity lookup/query helpers live in `Services/Helpers/FindEntityHelper.cs`.
- `FindEntityHelper` now owns shared character/template/building area filtering used by `FindEntitiesHandler` and `HasBuilding`.
- For bounded building/entity checks, `AreaCondition.Evaluate(objectBounds, areaBounds)` is the intended argument order.

## Buff Service Notes

- Buff logic was split under `Services/Buffs`.
- `_EntityBuffService.cs` is the abstract base for typed buff services.
- Character, workplace, and water source buff services are separate concrete services.
- Status models are in `Models/StatusModels.cs` and use init-only record-style properties.
- Water source buffs support nullable effect fields and nullable `EntityIds` for global application.
