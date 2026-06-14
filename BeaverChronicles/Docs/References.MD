# Beaver Chronicles Reference

This file will list the available condition types and handler nodes for JSON events.

## Trigger Sources

Trigger sources are the game moments that can cause Beaver Chronicles to check whether an event is allowed to appear.

- `None`
- `GameLoad`
- `NewHour`
- `NewDay`
- `NewWeather`
- `NewCycle`
- `CharacterCreated`
- `BeaverGrownUp`
- `CharacterDeath`
- `ToolUnlocked`
- `BuildingUnlocked`
- `BuildingPlaced`
- `BuildingFinished`
- `NewWellbeingHighscore`
- `WeatherWarning`
- `NeedActivated`
- `NeedDeactivated`
- `Injured`
- `InjuryCured`
- `Contaminated`
- `Decontaminated`
- `Custom`
- `CharacterEnteredArea`

## Node Types

This section will list the available event node types.

- `SetCustomParameter`: stores a value on the current event record. Supports `String`, `Int`, and `Float` values. Operations include `Add`, `Sub`, `Mul`, `Div`, `Round`, `Ceil`, `Floor`, and `Not`. `Not` treats `Value1` as true when it is a positive number or a true-like string, then stores `0`; otherwise it stores `1`.
- `FindEntities`: finds characters or template-tracked entities and stores their IDs as custom parameters. Use `ChooseRandom: true` to randomize matches before `MaxCount` is applied.
- `CharacterStatus`: removes or inflicts a need on characters resolved from `EntityIds`.

## Conditions

This section will list the available condition types.

- `Flag`: checks whether one or more flags are set.
- `_CustomParameter`: checks whether a custom parameter exists, or compares its value.
- `GameStat`: compares a ModdableTimberborn game stat. Use `CheckExistsOnly: true` to check whether a stat exists without comparing its value.

### Game Stats

The `GameStat` condition and `{GS_...}` text placeholders use stat IDs from ModdableTimberborn. Beaver Chronicles does not duplicate that list here; refer to [ModdableTimberborn Game Stats](../../Docs/ModdableTimberborn/GameStats.MD#built-in-stat-quick-reference).
