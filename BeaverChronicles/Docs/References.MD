# Beaver Chronicles Reference

Raw reference data for blueprint authors. Keep usage guides and examples in the guide pages; this file should stay compact and table-first.

# Trigger Sources

Trigger sources are game moments that can cause Beaver Chronicles to check whether an event should appear.

| Name | Fired when | Parameter Type |
|---|---|---|
| `None` | No automatic trigger source. | None |
| `GameLoad` | Special event for when loading a save when an event is in progress. | None |
| `NewHour` | A new in-game hour starts. | [DayTimeParameters](#daytimeparameters) |
| `NewDay` | A new in-game day starts. | [DayTimeParameters](#daytimeparameters) |
| `NewWeather` | The current weather changes. | [DayTimeParameters](#daytimeparameters) |
| `NewCycle` | A new cycle starts. | [DayTimeParameters](#daytimeparameters) |
| `CharacterCreated` | A character (Beaver or Bot) is created. | [CharacterParameters](#characterparameters) |
| `BeaverGrownUp` | A child grows into an adult. | [BeaverGrownUpParameters](#beavergrownupparameters) |
| `CharacterDeath` | A character dies. | [CharacterParameters](#characterparameters) |
| `ToolUnlocked` | A tool is unlocked. | [ToolParameters](#toolparameters) |
| `BuildingUnlocked` | A building is unlocked. | [BuildingParameters](#buildingparameters) |
| `BuildingPlaced` | A building is placed. | [BuildingInstanceParameters](#buildinginstanceparameters) |
| `BuildingFinished` | A building finishes construction. | [BuildingInstanceParameters](#buildinginstanceparameters) |
| `NewWellbeingHighscore` | The colony reaches a new wellbeing high score. | `int` |
| `WeatherWarning` | A hazardous-weather warning appears. | [WeatherWarningParameters](#weatherwarningparameters) |
| `NeedActivated` | A character need becomes active. | [NeedChangedParameters](#needchangedparameters) |
| `NeedDeactivated` | A character need becomes inactive. | [NeedChangedParameters](#needchangedparameters) |
| `Injured` | A character becomes injured. | [CharacterParameters](#characterparameters) |
| `InjuryCured` | A character's injury is cured. | [CharacterParameters](#characterparameters) |
| `Contaminated` | A character becomes contaminated. | [CharacterParameters](#characterparameters) |
| `Decontaminated` | A character is decontaminated. | [CharacterParameters](#characterparameters) |
| `Custom` | Fired by a custom Chronicle event message. | [CustomEventParameters](#customeventparameters) |
| `CharacterEnteredArea` | A character enters a tracked area. | [CharacterInAreaParameters](#characterinareaparameters) |
| `WonderActivated` | A wonder is activated. | [BuildingInstanceParameters](#buildinginstanceparameters) |

# Node Types

Node types are the executable steps inside a Chronicle event graph.

| Name | Parameters | Description |
|---|---|---|
| [Chance](#chancedata) | [ChanceData](#chancedata) | Randomly routes to a success or failure node. |
| [BuffWorkplaces](#buffworkplacesdata) | [BuffWorkplacesData](#buffworkplacesdata) | Applies a workplace buff to matching workplaces. |
| [BuffWaterSource](#buffwatersourcedata) | [BuffWaterSourceData](#buffwatersourcedata) | Applies a buff to water-source entities. |
| [BuffCharacters](#buffcharactersdata) | [BuffCharactersData](#buffcharactersdata) | Applies a buff to selected characters. |
| [BuffCharacterNeeds](#buffcharacterneedsdata) | [BuffCharacterNeedsData](#buffcharacterneedsdata) | Applies or removes need-related character effects. |
| [DeleteEntity](#deleteentitydata) | [DeleteEntityData](#deleteentitydata) | Destroys buildings, terrain, and/or kills characters. |
| [CharacterStatus](#characterstatusdata) | [CharacterStatusData](#characterstatusdata) | Adds or removes character status effects. |
| [FindEntities](#findentitiesdata) | [FindEntitiesData](#findentitiesdata) | Finds entities and stores their IDs as custom parameters. |
| [Condition](#conditiondata) | [ConditionData](#conditiondata) | Branches based on a condition evaluator. |
| [Choice](#choicedata) | [ChoiceData](#choicedata) | Shows event text and player choices. |
| [FindSpawningSpot](#findspawningspotdata) | [FindSpawningSpotData](#findspawningspotdata) | Finds a valid map position for spawning. |
| [Give](#givedata) | [GiveData](#givedata) | Gives goods or other rewards. |
| [RequestNextEvent](#requestnexteventdata) | [RequestNextEventData](#requestnexteventdata) | Requests another event after this one. |
| [SetFlag](#setflagdata) | [SetFlagData](#setflagdata) | Sets one or more Chronicle flags. |
| [SpawnObject](#spawnobjectdata) | [SpawnObjectData](#spawnobjectdata) | Spawns an object from a template. |
| [SetCustomParameter](#setcustomparameterdata) | [SetCustomParameterData](#setcustomparameterdata) | Writes or computes a custom event parameter. |
| [TimeLimit](#timelimitdata) | [TimeLimitData](#timelimitdata) | Creates a delayed or timed follow-up. |

# Conditions

Conditions are boolean checks used by event availability, node branching, and option enabling.

| Name | Parameters | Description |
|---|---|---|
| [Chance](#chanceconditiondata) | [ChanceConditionData](#chanceconditiondata) | Passes randomly based on a configured chance. |
| [HasBuilding](#hasbuildingdata) | [HasBuildingData](#hasbuildingdata) | Checks for buildings matching template and state filters. |
| [CustomParameter](#customparameterdata) | [CustomParameterData](#customparameterdata) | Checks whether a custom parameter exists or compares its value. |
| [WorkplaceWorkerCount](#workplaceworkercountdata) | [WorkplaceWorkerCountData](#workplaceworkercountdata) | Checks worker counts for matching workplaces. |
| [GameStat](#gamestatdata) | [GameStatData](#gamestatdata) | Compares a ModdableTimberborn game stat. |
| [Weather](#weatherdata) | [WeatherData](#weatherdata) | Checks current or upcoming weather IDs. |
| [Flag](#flagdata) | [FlagData](#flagdata) | Checks whether one or more Chronicle flags are set. |
| [Character](#characterconditiondata) | [CharacterConditionData](#characterconditiondata) | Checks explicit character IDs or character filters. |
| [TriggerCharacter](#characterconditiondata) | [CharacterConditionData](#characterconditiondata) | Checks the character supplied by the current trigger. |

# Trigger Source Details

## DayTimeParameters

Used by time and weather-transition trigger sources.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `Day` | `int` | Current game day. |
| `Cycle` | `int` | Current cycle number. |
| `CycleDay` | `int` | Current day within the cycle. |
| `PartialDays` | `float` | Current time as fractional days. |
| `Hours` | `float` | Current time as hours. |
| `WeatherId` | `string` | Current weather ID. |

## CharacterParameters

Used by character-created, character-death, injury, contamination, and related character trigger sources.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |
| `TriggerCharacterId` | Trigger character entity ID. |
| `TriggerCharacterName` | Trigger character display name. |
| `TriggerCharacterIsBeaver` | Whether the trigger character is a beaver. |
| `TriggerCharacterIsAdult` | Whether the trigger character is an adult. |
| `TriggerCharacterType` | Character type value used by character filters. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `Character` | `Character` | Trigger character entity. |
| `IsBeaver` | `bool` | Whether the trigger character is a beaver. |
| `IsAdult` | `bool` | Whether the trigger character is an adult. |
| `CharacterType` | `CharacterType` | Computed character type. |

## BeaverGrownUpParameters

Used when a child beaver grows into an adult. Also provides `CharacterParameters` for the adult beaver.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |
| `TriggerCharacterId` | Adult beaver entity ID. |
| `TriggerCharacterName` | Adult beaver display name. |
| `TriggerCharacterIsBeaver` | Always `True`. |
| `TriggerCharacterIsAdult` | Always `True`. |
| `TriggerCharacterType` | Always `AdultBeaver`. |
| `TriggerAdultId` | Adult beaver entity ID. |
| `TriggerAdultName` | Adult beaver display name. |
| `TriggerAdultIsBeaver` | Always `True`. |
| `TriggerAdultIsAdult` | Always `True`. |
| `TriggerAdultType` | Always `AdultBeaver`. |
| `TriggerChildId` | Child beaver entity ID. |
| `TriggerChildName` | Child beaver display name. |
| `TriggerChildIsBeaver` | Always `True`. |
| `TriggerChildIsAdult` | Always `False`. |
| `TriggerChildType` | Always `ChildBeaver`. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `Adult` | `Character` | Adult character after growth. |
| `Child` | `Character` | Child character before growth. |
| `Parameter` | `CharacterParameters` | Adult beaver exposed to character-based conditions and helpers. |

## ToolParameters

Used when a tool is unlocked.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `Tool` | `ITool` | Unlocked tool instance. |

## BuildingParameters

Used when a building template is unlocked.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `TemplateName` | `string` | Building template name. |
| `PlaceableBlockObjectSpec` | `PlaceableBlockObjectSpec` | Placeable building spec. |
| `Tool` | `BlockObjectTool?` | Related build tool, when available. |

## BuildingInstanceParameters

Used when a building is placed, finishes construction, or a wonder is activated.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `TemplateName` | `string` | Building template name. |
| `PlaceableBlockObjectSpec` | `PlaceableBlockObjectSpec` | Placeable building spec. |
| `Tool` | `BlockObjectTool?` | Related build tool, when available. |
| `BlockObject` | `BlockObject` | Trigger building entity. |

## WeatherWarningParameters

Used when a hazardous-weather warning appears.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `HazardousWeatherId` | `string` | Upcoming hazardous weather ID. |
| `DayTime` | `DayTimeParameters` | Current day/time context. |

## NeedChangedParameters

Used when a character need becomes active or inactive. Also provides `CharacterParameters` for the affected character.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |
| `TriggerCharacterId` | Affected character entity ID. |
| `TriggerCharacterName` | Affected character display name. |
| `TriggerCharacterIsBeaver` | Whether the affected character is a beaver. |
| `TriggerCharacterIsAdult` | Whether the affected character is an adult. |
| `TriggerCharacterType` | Character type value used by character filters. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `Need` | `NeedSpec` | Need that changed state. |
| `IsActive` | `bool` | Whether the need is active after the change. |
| `Character` | `CharacterParameters` | Affected character context. |

## CustomEventParameters

Used by custom Chronicle event messages.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | Custom event name. |
| `Data` | `object?` | Optional custom event payload. |

## CharacterInAreaParameters

Used when a character enters a tracked area. Also provides `CharacterParameters` for the entering character.

Custom parameters written:

| Name | Description |
|---|---|
| `TriggerSource` | Source name that triggered the event check. |
| `TriggerCharacterId` | Entering character entity ID. |
| `TriggerCharacterName` | Entering character display name. |
| `TriggerCharacterIsBeaver` | Whether the entering character is a beaver. |
| `TriggerCharacterIsAdult` | Whether the entering character is an adult. |
| `TriggerCharacterType` | Character type value used by character filters. |

Provided parameters:

| Name | Type | Description |
|---|---|---|
| `Character` | `CharacterParameters` | Entering character context. |

# Node Type Details

## ChanceData

Randomly routes to either the success node or the failure node.

Fields:

| Name | Type | Description |
|---|---|---|
| `Value` | `string` | Chance value after text formatting. |
| `SuccessNodeId` | `string?` | Node to run when the roll succeeds. |
| `FailNodeId` | `string?` | Node to run when the roll fails. |

## BuffWorkplacesData

Applies or removes a workplace buff for matching workplace templates.

Fields:

| Name | Type | Description |
|---|---|---|
| `TemplateNames` | `FrozenSet<string>` | Exact workplace template names to target. |
| `TemplateNamePrefixes` | `string[]` | Workplace template-name prefixes to target. |
| `BuffId` | `string` | Stable buff ID. |
| `TitleLoc` | `string` | Localization key for buff title. |
| `DescLoc` | `string` | Localization key for buff description. |
| `Bonuses` | `FormattableGoodItem[]` | Bonus IDs and amounts to apply. |
| `Days` | `string?` | Duration in days. Non-permanent buffs with zero or lower duration are removed. |
| `Category` | `EntityBuffCategory` | `LimitedTime` or `Permanent`. |

## BuffWaterSourceData

Applies or removes a water-source buff.

Fields:

| Name | Type | Description |
|---|---|---|
| `EntityIds` | `string[]?` | Water-source entity IDs to target. `null` targets the buff globally. |
| `BuffInfo` | `BuffInfo` | Shared buff metadata. |
| `Effects` | `BuffWaterSourceEffects` | Water-source behavior changes. |

`BuffInfo` fields:

| Name | Type | Description |
|---|---|---|
| `BuffId` | `string` | Stable buff ID. |
| `TitleLoc` | `string` | Localization key for buff title. |
| `DescLoc` | `string` | Localization key for buff description. |
| `Days` | `string?` | Duration in days. Non-permanent buffs with zero or lower duration are removed. |
| `Category` | `EntityBuffCategory` | `LimitedTime` or `Permanent`. |

`BuffWaterSourceEffects` fields:

| Name | Type | Description |
|---|---|---|
| `ImmuneToDrought` | `string?` | Optional boolean override for drought immunity. |
| `ImmuneToBadtide` | `string?` | Optional boolean override for badtide immunity. |
| `StrengthMultiplier` | `string?` | Optional water-source strength multiplier. |
| `ContaminationDelta` | `string?` | Optional contamination delta. |

## BuffCharactersData

Applies or removes a character buff for matching character types.

Fields:

| Name | Type | Description |
|---|---|---|
| `BuffId` | `string` | Stable buff ID. |
| `TitleLoc` | `string` | Localization key for buff title. |
| `DescLoc` | `string` | Localization key for buff description. |
| `CharacterTypes` | `CharacterType` | Character type flags to target. |
| `Bonuses` | `FormattableGoodItem[]` | Bonus IDs and amounts to apply. |
| `Days` | `string?` | Duration in days. Non-permanent buffs with zero or lower duration are removed. |
| `Category` | `EntityBuffCategory` | `LimitedTime` or `Permanent`. |

## BuffCharacterNeedsData

Boosts all beaver needs for selected character IDs.

Fields:

| Name | Type | Description |
|---|---|---|
| `Id` | `string?` | Single character entity ID. |
| `Ids` | `string[]` | Additional character entity IDs. |
| `Amount` | `string?` | Optional need boost amount. |
| `Permanent` | `bool` | Whether to also register permanent need boosts. |

## DeleteEntityData

Destroys buildings, kills characters, and/or destroys terrain in a bounds area.

Fields:

| Name | Type | Description |
|---|---|---|
| `EntityIds` | `string[]` | Entity IDs to delete. Buildings are destroyed; mortal characters die instantly. |
| `BeaverDeathMessageLoc` | `string?` | Optional localization key for character death messages. |
| `TerrainBounds` | `SerializableBoundsInts?` | Terrain voxel bounds to destroy. |

## CharacterStatusData

Removes or inflicts needs on selected characters.

Fields:

| Name | Type | Description |
|---|---|---|
| `EntityIds` | `string[]` | Character entity IDs. |
| `RemoveNeed` | `string?` | Need ID to remove from each character. |
| `InflictNeed` | `string?` | Need ID to inflict on each character. |

## ChoiceData

Shows event content and optional player choices. Choice selections are recorded in Chronicle history, and choice flags are written automatically by the choice handler.

Fields:

| Name | Type | Description |
|---|---|---|
| `TextLoc` | `string` | Localization key for the main event text. |
| `TopImage` | `bool` | Uses the default top image path for the event. |
| `TopImagePath` | `string?` | Explicit top image path. |
| `SideImage` | `bool` | Uses the default side image path for the event. |
| `SideImagePath` | `string?` | Explicit side image path. |
| `Options` | `ChoiceOptionData[]` | Player options. Empty options produce an OK-style dialog. |
| `DefaultOption` | `int` | 1-based default option index. |
| `ReuseRecordPage` | `bool` | Appends to the current record page instead of creating a new one. |

Option fields:

| Name | Type | Description |
|---|---|---|
| `TextLoc` | `string` | Localization key for the option text. |
| `NoteLoc` | `string?` | Optional note shown with the option. |
| `NextNodeId` | `string?` | Node to run if this option is selected. |
| `EnabledConditionNodeId` | `string?` | Condition node that controls whether this option is enabled. |
| `DisabledTextLoc` | `string?` | Optional disabled-state note. |

## FindEntitiesData

Finds characters or template-tracked entities and writes the found entity IDs to custom parameters.

Fields:

| Name | Type | Description |
|---|---|---|
| `TemplateNames` | `string[]` | Exact template names to match. |
| `TemplatePrefixes` | `string[]` | Template-name prefixes to match. |
| `CharacterType` | `CharacterType` | Character filter. `Unknown` disables character search. |
| `Areas` | `SerializableBoundsInts[]` | Optional map-area filters. |
| `AreaCondition` | `AreaCondition` | How entity bounds are compared with `Areas`. |
| `MaxCount` | `int` | Maximum number of entity IDs to store. |
| `ChooseRandom` | `bool` | Randomizes matches before applying `MaxCount`. |
| `ResultName` | `string` | Custom-parameter prefix for stored results. |
| `NoneFoundNodeId` | `string?` | Node to run if no entity is found. |

Custom parameters written:

| Name | Description |
|---|---|
| `<ResultName>_Count` | Number of found entities. |
| `<ResultName>_1`, `<ResultName>_2`, ... | Found entity IDs in order. |
| `<ResultName>` | First found entity ID, only when exactly one entity is found. |

## ConditionData

Branches to one node when its conditions pass and another node when they fail.

Fields:

| Name | Type | Description |
|---|---|---|
| `FulfilledNodeId` | `string?` | Node to run when conditions pass. |
| `FailedNodeId` | `string?` | Node to run when conditions fail. |
| `Conditions` | `ConditionItem[]` | Condition evaluator entries. |
| `ConditionType` | `ConditionType` | Combines condition entries. Valid values are `Never`, `Any`, and `All`. |

`ConditionItem` fields:

| Name | Type | Description |
|---|---|---|
| `Type` | `string` | Condition evaluator type. |
| `Parameters` | `object?` | Condition-specific parameter object. |

## FindSpawningSpotData

Finds a valid placement for a block-object template and writes the coordinates to custom parameters.

Fields:

| Name | Type | Description |
|---|---|---|
| `TemplateNames` | `string[]` | Candidate template names. |
| `ResultName` | `string` | Custom-parameter prefix for stored coordinates. |
| `LimitArea` | `SerializableInts?` | Optional area limit used by the placement helper. |
| `NearBuildingRadius` | `int` | Radius for preferring placement near buildings. |
| `FailedNodeId` | `string?` | Node to run if no valid placement is found. |

Custom parameters written:

| Name | Description |
|---|---|
| `<ResultName>_X` | Spawn X coordinate. |
| `<ResultName>_Y` | Spawn Y coordinate. |
| `<ResultName>_Z` | Spawn Z coordinate. |
| `<ResultName>_Orientation` | Spawn orientation. |

## GiveData

Gives goods, science, and/or character spawns.

Fields:

| Name | Type | Description |
|---|---|---|
| `Items` | `FormattableGoodItem[]` | Goods or special IDs to give. |
| `Science` | `string?` | Science amount to add. |
| `Spawns` | `FormattableGoodItem[]` | Character spawn requests. Valid IDs include `AdultBeaver`, `ChildBeaver`, and `Bot`. |
| `PreferedDistrictCenter` | `string?` | Optional district center entity ID for goods delivery. |

`FormattableGoodItem` fields:

| Name | Type | Description |
|---|---|---|
| `Id` | `string` | Good ID, special stat ID, or character type. |
| `Amount` | `string` | Amount after text formatting. |

## RequestNextEventData

Requests a specific event to be considered next.

Fields:

| Name | Type | Description |
|---|---|---|
| `Id` | `string?` | Event ID to request. |

## SetFlagData

Sets or removes a Chronicle flag.

Fields:

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | Flag name. Supports text placeholders. |
| `State` | `bool` | `true` sets the flag; `false` removes it. |

## SpawnObjectData

Spawns a block object from one of the supplied templates.

Fields:

| Name | Type | Description |
|---|---|---|
| `TemplateNames` | `string[]` | Candidate template names. |
| `X` | `string` | Spawn X coordinate. |
| `Y` | `string` | Spawn Y coordinate. |
| `Z` | `string` | Spawn Z coordinate. |
| `Orientation` | `string` | Spawn orientation, such as `Cw0`. |
| `Flipped` | `string` | Boolean string for flip mode. |
| `ConflictMode` | `string` | `Ignore` or `Destructive`. |
| `FailedNodeId` | `string?` | Node to run if spawning fails. |

## SetCustomParameterData

Writes a custom event parameter, optionally computing it from one or two values.

Fields:

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | Custom parameter name. Supports text placeholders. |
| `Type` | `CustomParameterValueType` | `String`, `Int`, or `Float`. |
| `Value1` | `string` | First value. Supports text placeholders. |
| `Value2` | `string?` | Optional second value. |
| `Operation` | `CustomParameterOperation` | `Add`, `Sub`, `Mul`, `Div`, `Round`, `Ceil`, `Floor`, or `Not`. |

## TimeLimitData

Registers an active timed event state, optional payment, and optional event subscriptions.

Fields:

| Name | Type | Description |
|---|---|---|
| `Days` | `string?` | Time limit in days. |
| `Hours` | `string?` | Time limit in hours when `Days` is not set. |
| `Payments` | `FormattableGoodItem[]` | Payment requirements for resolving the active event. |
| `Subscriptions` | `TimeLimitSubscriptionData[]` | Time-limit event subscriptions that can resolve the active event. |
| `PanelTextLoc` | `string?` | Localization key for active-event panel text. |
| `PaidNodeId` | `string?` | Node to run when the payment is completed. |

`TimeLimitSubscriptionData` fields:

| Name | Type | Description |
|---|---|---|
| `EventName` | `string` | Time-limit event name to subscribe to. |
| `NextNodeId` | `string?` | Node to run when this subscription fires. |

# Condition Details

## ChanceConditionData

Passes randomly based on the configured chance value.

Fields:

| Name | Type | Description |
|---|---|---|
| `Value` | `string` | Chance value after text formatting. |

## HasBuildingData

Checks whether matching buildings exist in the colony.

Fields:

| Name | Type | Description |
|---|---|---|
| `TemplateNames` | `FrozenSet<string>` | Exact building template names to count. |
| `TemplateNamePrefixes` | `string[]` | Building template-name prefixes to count. |
| `Amount` | `MinMaxInt` | Required count range. Defaults to at least 1. |
| `IncludeUnderConstruction` | `bool` | Whether unfinished buildings count. |
| `Areas` | `SerializableBoundsInts[]` | Optional map-area filters. |
| `AreaCondition` | `AreaCondition` | How building bounds are compared with `Areas`. |

## CustomParameterData

Checks whether a custom parameter exists or compares its value.

Fields:

| Name | Type | Description |
|---|---|---|
| `Key` | `string` | Custom parameter key. Supports text placeholders. |
| `Value` | `string?` | Optional value to compare. If omitted, only existence is checked. |
| `Comparison` | `NumericComparisonMode` | Numeric comparison mode. Non-numeric values only support equality. |

## WorkplaceWorkerCountData

Checks assigned worker counts for matching workplaces.

Fields:

| Name | Type | Description |
|---|---|---|
| `TemplateNames` | `FrozenSet<string>` | Exact workplace template names to count. |
| `TemplateNamePrefixes` | `string[]` | Workplace template-name prefixes to count. |
| `Amount` | `MinMaxInt` | Required worker count range. Defaults to at least 1. |

## FlagData

Checks global Chronicle flags or generated event-choice flags.

Fields:

| Name | Type | Description |
|---|---|---|
| `Flag` | `string?` | Single flag to check. Supports text placeholders. |
| `Flags` | `string[]` | Additional flags to check. Supports text placeholders. |
| `ConditionType` | `ConditionType` | Combines flags. Valid values are `Never`, `Any`, and `All`. |

## GameStatData

Checks a value from the ModdableTimberborn game stat registry.

Fields:

| Name | Type | Description |
|---|---|---|
| `Stat` | `string` | Stat ID. Supports text placeholders. |
| `Value` | `string` | Numeric value to compare against. Supports text placeholders. |
| `CheckExistsOnly` | `bool` | When true, checks whether the stat exists without comparing values. |
| `Comparison` | `NumericComparisonMode` | Comparison mode, such as `Equal`, `Greater`, or `GreaterOrEqual`. |

## WeatherData

Checks current weather state or weather warning state.

Fields:

| Name | Type | Description |
|---|---|---|
| `HazardousWeatherId` | `string?` | Expected upcoming hazardous weather ID. |
| `CurrentWeatherId` | `string?` | Expected current weather ID. |
| `WarningStage` | `string?` | Expected weather warning stage. |

## CharacterConditionData

Checks character entity IDs or filters. `TriggerCharacter` uses this same data shape against the trigger character.

Fields:

| Name | Type | Description |
|---|---|---|
| `EntityIds` | `string[]` | Character entity IDs. Supports text placeholders. |
| `ConditionType` | `ConditionType` | Combines matched characters. Defaults to `Any`. |
| `IsBeaver` | `bool?` | Optional beaver filter. |
| `IsAdult` | `bool?` | Optional adult-beaver filter. |
| `CharacterType` | `CharacterType?` | Optional character type flag filter. |
| `Name` | `string?` | Optional case-insensitive first-name filter. |

# Game Stats

The `GameStat` condition and `{GS_...}` text placeholders use stat IDs from ModdableTimberborn. See [ModdableTimberborn Game Stats](../../ModdableTimberborn/Docs/GameStats.MD#built-in-stat-quick-reference).
