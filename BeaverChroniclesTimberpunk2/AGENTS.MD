# Agent Instructions

Follow the shared Beaver Chronicles agent instructions in `../BeaverChronicles/AGENTS.MD`.

Before working in this pack, read that file and its referenced `../BeaverChronicles/UsefulAgentContext.MD`.

## What This Pack Is

This pack is the TIMBERPUNK 2 campaign overlay for Beaver Chronicles. Implement story events from `timberpunk2story.txt` **one event at a time**, only when asked, and wait for review before starting the next one.

Unlike `BeaverChroniclesWaterfallEventPack`, do **not** gate story events with `BlockedNoFlags` / `RequiredFlags` on `MAP.TIMBERPUNK 2`. This pack also removes or overrides original Beaver Chronicles events, so it is meant to own the campaign, not sit unused on other maps.

The map name is `TIMBERPUNK 2`. The engine map flag is `MAP.TIMBERPUNK 2` (`MAP.` + map name, space included).

The only map check is the bootstrap warning event `TBP.NotTimberpunk2Map` (`NewHour`). It uses `BlockedFlags: [ "MAP.TIMBERPUNK 2" ]` so it is eliminated on the correct map and only warns when the player left this pack enabled on a different map. After that warning, proceeding is treated as user error; do not add further off-map protection.

Campaign event ids are prefixed with `TBP.`. Do not split the same story beat into Easy/Normal/Hard events. Put difficulty branching inside one event: read `TBP.Diff.*` flags, write custom parameters, then share Give/status nodes.

Current events:

- `TBP.0` — difficulty select (`NewHour`, `MinDay: 1.25`, max weight). Vanilla `StartBonus` is patched to `Sources: []`. All three choices set a flag then go to the same `RequestNextEvent` for `TBP.Welcome`. That request waits for the next matching trigger (next hour).
- `TBP.Welcome` — one welcome event. It sets `Diff`, `RewardId`, `RewardAmount`, and `Science` from the difficulty flags, then shows Easy/Normal/Hard copy. All paths `RequestNextEvent` `TBP.M1.0`.
- `TBP.M1.0` — trek memory (`NewHour`, not mini). Paid choices use a 12-hour `TimeLimit`; timeout is `M1.7` inside this event (life-expectancy debuff, not a literal game crash). Instant and paid success paths request `TBP.M1.1`–`TBP.M1.4`.
- `TBP.M1.1`–`TBP.M1.4` — end-of-day follow-ups (`NewDay`). Do not request `TBP.M1.5` until the wonder event exists.

Story ids match the author's codes with a `TBP.` prefix (`m1.0` → `TBP.M1.0`).

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
