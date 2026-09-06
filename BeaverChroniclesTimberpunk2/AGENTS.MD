# Agent Instructions

Follow the shared Beaver Chronicles agent instructions in `../BeaverChronicles/AGENTS.MD`.

Before working in this pack, read that file and its referenced `../BeaverChronicles/UsefulAgentContext.MD`.

## What This Pack Is

This pack is the TIMBERPUNK 2 campaign overlay for Beaver Chronicles. Implement story events from `timberpunk2story.txt` **one event at a time**, only when asked, and wait for review before starting the next one.

Campaign events use `BlockedNoFlags: [ "MAP.TIMBERPUNK 2" ]` so they are eliminated on any other map (weight `-1`). The warning event `TBP.NotTimberpunk2Map` (`NewHour`) is the opposite: `BlockedFlags: [ "MAP.TIMBERPUNK 2" ]`, so it only fires off-map.

The map name is `TIMBERPUNK 2`. The engine map flag is `MAP.TIMBERPUNK 2` (`MAP.` + map name, space included).

Everything after `TBP.0` also requires a difficulty flag (`TBP.Diff.Easy` / `Normal` / `Hard`, `RequiredFlags` default `Any`). That keeps `TBP.M1.0` and other max-weight `NewHour` events from winning the first hour over the difficulty dialog. `TBP.Welcome` already used those flags. `TBP.M1.5` still requires `EVENT.TBP.M1.2` (difficulty is implied by then).

Campaign event ids are prefixed with `TBP.` (`m1.0` → `TBP.M1.0`). Do not split the same story beat into Easy/Normal/Hard events.

The author's first half (`timberpunk2story.txt` through `m7.4`) is implemented. The latter half (Tradition / Democracy / Autocracy follow-ups and the six endings) is **not** written. `TBP.M7.4` only sets `TBP.Gov.Tradition`, `TBP.Gov.Democracy`, or `TBP.Gov.Autocracy` and does not `RequestNextEvent` missing ids.

Wonder follow-up `TBP.M1.5` requires `EVENT.TBP.M1.2` and `BuildingFinished` of `EarthRecultivator.Folktails` or `EarthRepopulator.IronTeeth` (built, not launched). `TBP.M4.0` uses `NewDay` `MinDay: 5` (end of day 4); the optional "24h after the drill" trigger was not added. `TBP.M5.0` fires on the first `NewWeather` drought.

Need overlays live in `version-1.1/Blueprints/NeedSpecs` and `NeedCollectionSpec/BeaverChroniclesCommon.blueprint.json` (keeps `Remembrance` plus `TBP.UnheardConcerns` / `TBP.BureaucraticNightmare`).

Difficulty flags:

- `TBP.Diff.Easy`
- `TBP.Diff.Normal`
- `TBP.Diff.Hard`

Choice flags from `TBP.0` are `EVENT.TBP.0.C1.1` through `C1.3`. `RequestNextEvent` throws if the id is missing, so do not queue an event that does not exist yet.

## Files

- Story source: `timberpunk2story.txt`
- Event blueprints: `version-1.1/Blueprints/ChronicleEvents` (campaign events in `Timberpunk2/`)
- English text: `version-1.1/Localizations/enUS.csv` with `LV.BCT.` keys
- Shared JSON authoring patterns: `../BeaverChronicles/Docs/README.MD`, `EventGuides.MD`, `References.MD`

When a later event replaces a vanilla one, follow the Waterfall `StartBonus.blueprint.json` patch style: same `Id`, only the fields that change.
