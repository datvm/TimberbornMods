# Waterfall Event Guides

This guide assumes you have already read [EventGuides.MD](EventGuides.MD). It only calls out patterns that are new or especially different in `BeaverChroniclesWaterfallEventPack`; common concepts such as `Choice`, `TimeLimit`, payments, normal buffs, and basic flags are covered in the base guide.

The event pack project is [BeaverChroniclesWaterfallEventPack](../../BeaverChroniclesWaterfallEventPack). Its event blueprints live in [Blueprints/ChronicleEvents](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents), and its English text lives in [Localizations/enUS.csv](../../BeaverChroniclesWaterfallEventPack/Localizations/enUS.csv).

## What This Pack Adds

The Waterfall pack demonstrates these extra patterns:

- Map-specific event gating with `MAP.WATERFALLS`.
- Overriding a base event with a tiny blueprint patch.
- Using automatically generated choice flags as follow-up state.
- Water-source buffs, including drought immunity and contamination effects.
- Shortening an active timer by using `TimeLimitRemainingDays`.
- Payment-only `TimeLimit` nodes.
- `FindSpawningSpot` followed by destructive `SpawnObject`.
- Follow-up event requests after a temporary buff ends.
- Building-triggered conditions with `TriggerBuildingId`.
- `FindEntities.AllBuildings`.
- Terrain deletion in stepped layers.
- Spawning a permanent drought-resistant water source.
- Camera focus and shake after a map-changing event.

## Reading Order

1. [Start Bonus Override](#start-bonus-override)
2. [A Clear Fall](#a-clear-fall)
3. [The Spring Beneath the Stone](#the-spring-beneath-the-stone)
4. [The Drain Beneath the Drain](#the-drain-beneath-the-drain)
5. [What Lies Below](#what-lies-below)
6. [The Folded Feast](#the-folded-feast)
7. [The Folded Feast: Unleashed](#the-folded-feast-unleashed)
8. [Sinkhole of the Waterfall](#sinkhole-of-the-waterfall)

## Pack-Wide Map Flag

Most pack events require:

```json
"RequiredFlags": [ "MAP.WATERFALLS" ]
```

Use this when an event should only exist on the Waterfalls map. Conversely, use:

```json
"BlockedFlags": [ "MAP.WATERFALLS" ]
```

when a base event should not appear on that map.

## Start Bonus Override

[StartBonus.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/StartBonus.blueprint.json) is a minimal override:

```json
{
  "ChronicleEventSpec": {
    "Conditions": {
      "BlockedFlags": [ "MAP.WATERFALLS" ]
    }
  }
}
```

New pattern: a pack can adjust an existing event without copying the whole event. Here it blocks the base `StartBonus` only on Waterfalls, so [StartBonusWaterfall.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/StartBonusWaterfall.blueprint.json) can replace it.

## A Clear Fall

[StartBonusWaterfall.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/StartBonusWaterfall.blueprint.json) is a map-specific start event.

New pattern: later events read automatically generated choice flags:

- `EVENT.StartBonusWaterfall.C1.1` means the player filled the barrels.
- `EVENT.StartBonusWaterfall.C1.2` means the player left the spring untouched.

No explicit `SetFlag` node is needed. The choice system records these flags automatically, and `FirstDroughtWaterfall` uses them to decide whether the spring can help during drought.

## The Spring Beneath The Stone

[FirstDroughtWaterfall.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/FirstDroughtWaterfall.blueprint.json) appears during drought if the player left the spring untouched.

New pattern: weather-duration water-source buff.

The event finds one random `WaterSource`, stores it as `WaterSource`, and applies:

```json
"Effects": {
  "ImmuneToDrought": "true"
}
```

with:

```json
"Days": "{GS_WeatherCurrentHazardousDuration}"
```

That means the water source remains active only for the current hazardous-weather duration.

## The Drain Beneath The Drain

[BadtideDrainStudy.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/BadtideDrainStudy.blueprint.json) starts a long Badtide investigation.

New pattern: pay after a timer starts to shorten remaining time.

The first `TimeLimit` records `TimeLimitRemainingDays` when its payment is completed. The event then computes:

```json
"Name": "ReducedStudyDays",
"Type": "Float",
"Value1": "{CP_TimeLimitRemainingDays}",
"Value2": "{F2}",
"Operation": "Div"
```

and starts a second `TimeLimit` using:

```json
"Days": "{CP_ReducedStudyDays}"
```

So the player can begin the study now and later pay science to reduce whatever time is left.

The event also uses `RequestNextEvent` to ask for `BadtideDrainExcavation` after the study completes.

## What Lies Below

[BadtideDrainExcavation.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/BadtideDrainExcavation.blueprint.json) is the requested follow-up.

New patterns:

- Payment-only `TimeLimit` nodes with `Payments` and `PaidNodeId`, but no `Days` or `Hours`.
- Two similar payment paths where the careful version costs more but reduces random injury victims.
- `FindSpawningSpot` writes custom parameters such as `UnstableCoreSpot_X` and `UnstableCoreSpot_Orientation`.
- `SpawnObject` reads those custom parameters and uses `ConflictMode: "Destructive"`.

The destructive spawn is the important new idea. It lets the event place the `UnstableCore` even if something occupies the target spot.

## The Folded Feast

[Taco.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/Taco.blueprint.json) is weather-gated to `TemperateWeather`.

New pattern: request a follow-up only after a temporary buff has ended.

After the successful taco buff message, the event starts a plain `TimeLimit` matching the buff duration. When that timer ends, it requests `Taco2`.

This makes the next chapter appear after the original temporary effect has had time to play out.

## The Folded Feast: Unleashed

[Taco2.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/Taco2.blueprint.json) is a follow-up chapter gated by the first taco event.

New patterns:

- The powerful buff and the aftermath timer are separate. The buff lasts `{F3}` days, while the cure sequence begins after `{F4}` days.
- `SetCustomParameter` computes `BeaverCount` from adult plus child population.
- The cure payment uses `{CP_BeaverCount}` as the amount for both water and berries.
- Failure finds a random `WaterSource` and applies temporary contamination:

```json
"Effects": {
  "ContaminationDelta": "1"
}
```

This shows that `BuffWaterSource` can alter contamination without touching drought or badtide immunity.

## Sinkhole Of The Waterfall

[WaterfallSinkhole.blueprint.json](../../BeaverChroniclesWaterfallEventPack/Blueprints/ChronicleEvents/WaterfallSinkhole.blueprint.json) is the advanced map-changing example.

New patterns:

- `Repeat: true` is used because early triggers may only ask the player for a preference.
- The event stores preference flags: disabled forever, wait until cycle 10, or trigger now.
- The event triggers from `BuildingFinished`.
- The trigger condition uses `Building` with:

```json
"EntityIds": [ "TriggerBuildingId" ]
```

This checks the specific building that caused the trigger, not any building in the colony.

### Delayed Destruction

The event inserts a tiny delay before destructive work:

```json
"Type": "TimeLimit",
"Data": {
  "Hours": "0.01"
}
```

This avoids deleting a building in the same tick it finished construction, which could cause a crash.

### Area Victims

`FindVictims` uses:

```json
"CharacterType": "AdultBeaver, ChildBeaver, Bot",
"AllBuildings": true,
"ResultName": "SinkholeVictims"
```

`AllBuildings: true` is the new part. It tells `FindEntities` to include buildings even when no templates or prefixes are listed.

### Stepped Terrain

The sinkhole uses several `DeleteEntity` nodes with different `TerrainBounds`. The outer layer starts at `Z = 4`, and each inset starts lower until the center starts at `Z = 0`.

This creates a bowl shape instead of one flat rectangular deletion.

### New Water Source

After terrain deletion, the event spawns `WaterSource`, stores it as `SinkholeWaterSource`, and buffs only that entity:

```json
"Category": "Permanent",
"Effects": {
  "ImmuneToDrought": "true"
}
```

It deliberately does not set `ContaminationDelta`, so the source is drought-immune but not contamination-immune.

### Camera

The event then focuses and shakes the camera:

```json
"FocusOnEntityIds": [ "SinkholeWaterSource" ],
"CameraShake": {
  "Duration": 3,
  "Strength": 1
}
```

This is useful for map-changing events where the player should immediately see what changed.

### Result Text

The localization uses:

```text
{CP_SinkholeVictims_Count}
```

because `FindVictims` wrote the result count. This lets the story text report how many characters and buildings were removed.

## Next Steps

For full node and condition fields, use [References.MD](References.MD). For the foundational walkthroughs, use [EventGuides.MD](EventGuides.MD).
