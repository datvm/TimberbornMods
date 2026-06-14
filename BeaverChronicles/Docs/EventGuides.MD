# Event Guides

This page continues the JSON event guide with more examples from the base Beaver Chronicles mod. The goal is still practical: read one event, understand what each part is doing, then reuse the same pattern in your own event.

## Suggested Reading Order

The examples are ordered so each one introduces only a few new ideas:

1. [Start Bonus](README.MD#example-start-bonus) covers the basics: event declaration, conditions, parameters, choices, rewards, and simple condition nodes.
2. [First Death](#example-first-death) introduces active events, time limits, payments, chance branches, character-trigger checks, character need boosts, and temporary character buffs.
3. [First Child Growth](#example-first-child-growth) is a good next step for custom parameters, game stat placeholders, and calculated payment costs.
4. [Drought Prep](#example-drought-prep) introduces repeatable weather-triggered events.
5. [Unhoused](#example-unhoused) shows promise-style events that check game stats after a delay.
6. [Contamination Conspiracy](#example-contamination-conspiracy) shows time limit subscriptions that can react to another event before the timer expires.
7. [Food Wood Dispute](#example-food-wood-dispute-1) shows multi-chapter events, flags, workplace buffs, cleanup nodes, and follow-up events.

## Example: First Death

The next example is [FirstDeath.blueprint.json](../Root/Blueprints/ChronicleEvents/FirstDeath.blueprint.json). This event appears when a beaver dies for the first time. It asks the player how the colony should respond, with different consequences: build a small raft for a small one-time bonus, work toward a cemetery by paying more for a permanent bonus, or keep working and hope morale holds, which can either have a negative impact or a chance to get away with it.

This is a strong second example because it can stay active over time, show a task in the active event panel, wait for payments, and then resolve differently depending on what the player does.

### Event Declaration

`Id` is `FirstDeath`, the unique name of this event.

`TitleLoc` points to the localized event title.

This event does not set `IsMini`, so it follows the normal event flow and can remain active while the player works toward a goal.

### Conditions

`Sources` contains `CharacterDeath`, so this event is checked when a character dies.

`Weight` uses the maximum integer value, which means this event should be chosen whenever it is eligible unless another equally weighted event competes with it.

`ConditionNodeId` points to `IsBeaverDeath`. This gates the whole event behind a condition node. The event can only trigger if that node passes.

> [!INFO]  
> Event-level `Conditions` also support timing fields such as `MinDay`, `MaxDay`, `MinCycle`, and `MaxCycle`, plus flag gates like `RequiredFlags`, `RequiredNoFlags`, and `BlockedFlags`. `FirstDeath` does not need them because the trigger character check is enough.

### Parameters

`Floats` holds decimal values like payment deadlines, chances, stat modifiers, and buff durations.

`Strings` holds IDs like goods, need IDs, and stat IDs.

`Ints` holds whole-number values like payment amounts.

These parameters make the event easier to tune. If you want the cemetery to cost fewer planks, you can change one number in `Ints` instead of searching through the nodes.

### Trigger Character Condition

The `IsBeaverDeath` node is a `Condition` node. It contains a `Character` condition with:

- `EntityIds`: `[ "TriggerCharacterId" ]`
- `IsBeaver`: `true`

When an event starts from a character-related trigger, Beaver Chronicles records useful custom parameters such as `TriggerCharacterId`, `TriggerCharacterName`, `TriggerCharacterIsBeaver`, `TriggerCharacterIsAdult`, and `TriggerCharacterType`. This condition uses `TriggerCharacterId` to inspect the character who died.

> [!INFO]  
> A custom parameter is data tied to the current event record. Beaver Chronicles creates some automatically, like `TriggerCharacterId`, and JSON nodes can also set their own custom parameters. Later guides will cover setting custom parameters and using conditions to check them.

The result is simple: if the dead character was a beaver, the event is allowed to appear. If it was a bot or something else, it is not.

The `Character` condition also supports `IsAdult`, `CharacterType`, `Name`, `ConditionType`, and `Expected`. For example, `IsAdult: true` would require an adult beaver, and `Expected: false` would invert the result.

### The Choice Node

The first visible node is `Choice`. It shows the event text and three options:

- Send the deceased downriver.
- Build a cemetery.
- Keep working.

Each option points to a different next node with `NextNodeId`.

The first two options start `TimeLimit` nodes. The third option goes to a `Chance` node.

`Choice` data also supports the same optional fields covered in the first guide, such as `TopImagePath`, `SideImage`, `SideImagePath`, option notes, option condition checks, disabled text, and `ReuseRecordPage`. `FirstDeath` uses `TopImage: true` for the first page, then specific `TopImagePath` values for outcome pages.

### Time Limit Payments

`SetTimer1` and `SetTimer2` are `TimeLimit` nodes.

A `TimeLimit` node can keep a normal event active while the player completes a task. In this event, both timers show a panel message and register payments.

`SetTimer1` uses:

- `Days`: `{F1}`, so the raft option has a shorter deadline.
- `PanelTextLoc`: the text shown in the active event panel.
- `Payments`: one payment, `{I1}` logs of `{S1}`.
- `PaidNodeId`: `RaftNeedBuff`, where the event goes if the payment is completed.
- `NextNodeId`: `FailureBuff`, where the event goes if the timer expires.

`SetTimer2` works the same way, but with a longer deadline and two payments: science and planks.

`TimeLimit` also supports `Hours` instead of `Days`, and `Subscriptions` for listening to other time-limit events. If neither `Days` nor `Hours` is set, the node can still wait for a payment or subscription, but it will not have a countdown timer. Mini events cannot use `TimeLimit`.

> [!INFO]  
> In a `TimeLimit` node, `PaidNodeId` is used when all registered payments are completed. The node's normal `NextNodeId` is used when the timer runs out.

### Character Need Buffs

If the player completes the raft payment, the event goes to `RaftNeedBuff`, a `BuffCharacterNeeds` node.

It uses `Id: "{S4}"`, so it boosts the `Remembrance` need for all beavers. Because it does not specify an amount, the code boosts that need to its maximum.

If the player completes the cemetery payment, the event goes to `CemeteryNeedBuff`. It also boosts `Remembrance`, but it sets `Permanent: true`.

`BuffCharacterNeeds` supports:

- `Id` for one need ID.
- `Ids` for multiple need IDs.
- `Amount` if you want a specific boost amount instead of filling the need to maximum.
- `Permanent` to save the need boost and reapply it every new day.

### Chance Branch

If the player chooses to keep working, the event goes to `IgnoreChance`, a `Chance` node.

`Value` is `{F3}`, so the event rolls the 0.30 chance from the float parameters.

If the roll succeeds, the event goes to `IgnoredNoConsequence`. If it fails, it goes to `FailureBuff`.

`Chance` supports `SuccessNodeId` and `FailNodeId`. Either can be omitted if you want that branch to end the event.

### Failure Buff

`FailureBuff` is a `BuffCharacters` node. It applies a temporary penalty to adult beavers:

- `BuffId` gives this buff a stable ID, so the same buff can be updated or removed later.
- `TitleLoc` and `DescLoc` define the localized status text.
- `CharacterTypes` chooses who receives it. Here it is `AdultBeaver`.
- `Days` is `{F5}`, the penalty duration.
- `Bonuses` lists the stat changes. Here it applies `{F4}` to `{S5}`, so adult beavers get a negative `WorkingSpeed` modifier.

`BuffCharacters` also supports a `Category`. The default is `LimitedTime`, but it can be `Permanent`. If a limited-time character buff is given with `Days` less than or equal to zero, the handler removes that buff instead of applying it.

`CharacterTypes` can use more than one flag, such as `AdultBeaver, ChildBeaver`, if the effect should apply to multiple character types.

### Outcome Messages

The final nodes, such as `RaftMessage`, `CemeteryMessage`, `IgnoredNoConsequence`, and `FailureMessage`, are all `Choice` nodes with no options.

When a `Choice` node has no `Options`, it becomes a simple message with a generic confirmation button. After the player closes it, the event follows the node's `NextNodeId`. In this event, those message nodes do not have `NextNodeId`, so the event concludes.

This pattern is useful for endings: do the mechanical work first, then show a final story message.

## Example: First Child Growth

The next example is [FirstChildGrowth.blueprint.json](../Root/Blueprints/ChronicleEvents/FirstChildGrowth.blueprint.json). This event appears when a child grows up. It asks whether the colony should celebrate with a party, paying berries and water for a temporary working-speed bonus, or skip the party and move on.

The new concept here is calculation. The event does not hard-code the party cost. It computes the cost from the current population, stores it as a custom parameter, then uses that custom parameter in the payment text and `TimeLimit` payment.

### Custom Parameter Math

The event starts at `ComputeChildPlusOne`, not at a visible `Choice` node. It runs three `SetCustomParameter` nodes before showing anything to the player:

- `ComputeChildPlusOne` creates `ChildPlusOne` from `{GS_PopulationNumberOfChild} + 1`.
- `ComputeHalfChildren` creates `HalfChildren` from `{CP_ChildPlusOne} / 2`.
- `ComputePartyCost` creates `PartyCost` from `{GS_PopulationNumberOfAdult} + {CP_HalfChildren}`.

The final result is available as `{CP_PartyCost}`. The choice note uses it to show the required goods, and the `TimeLimit` payment uses it as the amount for both berries and water.

`SetCustomParameter` supports `String`, `Int`, and `Float` values. String parameters can only use `Add`, which joins `Value1` and `Value2`. Number parameters support `Add`, `Sub`, `Mul`, `Div`, `Round`, `Ceil`, and `Floor`. `Int` values are saved as whole numbers, while `Float` values are saved with two decimal places.

> [!INFO]  
> `{GS_...}` reads a game stat. `{CP_...}` reads a custom parameter already stored on the current event record. This is why the calculation nodes must run in order: `HalfChildren` cannot use `{CP_ChildPlusOne}` until the previous node creates it. For the list of available game stats, see [ModdableTimberborn Game Stats](../../Docs/ModdableTimberborn/GameStats.MD#built-in-stat-quick-reference).

### Party Payment

The visible `Choice` node has two options: hold the party or skip it.

The party option starts a `TimeLimit` node with two payments. Both payment amounts use `{CP_PartyCost}`, so the cost scales with the colony instead of being fixed.

If the player pays in time, `PartyBuff` applies a `BuffCharacters` node to both `AdultBeaver` and `ChildBeaver`. If the timer expires, the event falls through to `NoPartyMessage`.

This event also uses `SideImage: true`, which follows the same pattern as `TopImage: true` but places the image at the side.

## Example: Drought Prep

[DroughtPrep.blueprint.json](../Root/Blueprints/ChronicleEvents/DroughtPrep.blueprint.json) appears during a drought warning. The player sends scouts toward a spring, woods, or meadow. After a short delay, each route has a chance to succeed and grant a different reward.

This event is useful for learning repeatable weather events, combined trigger conditions, and delayed chance outcomes.

### Repeatable Event

`Repeat: true` allows this event to appear more than once. Without it, an event is normally treated as something that should only happen once.

The event uses `WeatherWarning` as its source, so it is checked when the game warns about upcoming hazardous weather.

### Weather And Chance Conditions

`ConditionNodeId` points to `IsDroughtWarning`. That condition node has two conditions:

- `Chance` with `{F1}`, so only some drought warnings create this event.
- `Weather` with `HazardousWeatherId: "{S1}"`, so it only matches drought warnings.

Because a `Condition` node defaults to `ConditionType: All`, both checks must pass.

The `Weather` condition also supports `CurrentWeatherId` and `WarningStage`. Use `HazardousWeatherId` for the incoming warned weather, `CurrentWeatherId` for the weather happening right now, and `WarningStage` if you need to check the warning state.

### Delayed Search Outcomes

Each choice goes to a `TimeLimit` node with no payments. When the timer ends, it goes to a `Chance` node.

If the chance succeeds, the event gives the matching reward and shows a success message. If it fails, it shows a failure message.

This is a simple pattern for "send someone to do a thing, wait, then roll for the result."

## Example: Unhoused

[Unhoused.blueprint.json](../Root/Blueprints/ChronicleEvents/Unhoused.blueprint.json) appears during the early game if beavers may be living without shelter. It asks whether the player will promise to shelter most beavers, shelter everyone, or refuse to make promises.

This event teaches game-stat conditions, starting an event from a condition node, reusing condition nodes, and checking a promise after a delay.

### Time Window

The event uses `NewDay`, `MinDay: 5`, and `MaxDay: 60`. That means it can only appear during a particular early-game window.

It also uses a lower `Weight` than the guaranteed story beats, so it is less forceful than events such as `FirstDeath`.

### Starting With A Condition

`StartNodeId` is `InitialCheck`, a `Condition` node. If the colony already has no homeless beavers, the event immediately applies a positive buff and shows a different message. If not, it shows the normal choice.

This is a useful pattern when an event has a special "you already solved it" path.

### GameStat Conditions

`InitialCheck`, `NotMostSheltered`, and `CheckPromise2` use the `GameStat` condition type.

`GameStat` compares a named game stat to a value. `Unhoused` checks stats such as `BedHomelessPercent` and `BedHousedPercent`. For the full stat list, see [ModdableTimberborn Game Stats](../../Docs/ModdableTimberborn/GameStats.MD#built-in-stat-quick-reference).

`Comparison` defaults to `Equal`, but the event uses comparisons such as `Less` and `LessOrEqual`. The value can be a literal number or a placeholder such as `{F1}`.

### Reusing A Condition Node

`NotMostSheltered` is used twice:

- As `EnabledConditionNodeId` on the "shelter most beavers" option, so the option is only enabled when that promise still makes sense.
- As the node reached after the timer, so the event can check whether the promise was fulfilled.

In that node, `FulfilledNodeId` goes to failure and `FailedNodeId` goes to success. That may look backwards at first, but the condition is named "NotMostSheltered"; fulfilling "not most sheltered" means the player failed the promise.

### BuffCharacters Variations

This event shows `BuffCharacters` with multiple bonuses at once. The best result boosts both working speed and movement speed, while the smaller success only boosts working speed.

It also shows a common cleanup trick indirectly: all variants use the same `BuffId`, `ChronicleEvent_Unhoused`, so later applications can update or replace the same status cleanly.

## Example: Contamination Conspiracy

[ContaminationConspiracy.blueprint.json](../Root/Blueprints/ChronicleEvents/ContaminationConspiracy.blueprint.json) appears when a beaver becomes contaminated. It asks the colony to cure that specific beaver before time runs out.

This event introduces `TimeLimit` subscriptions: the event can end early when something happens to the trigger character.

### Character Trigger Gate

Like `FirstDeath`, the event uses a `Character` condition on `TriggerCharacterId` to make sure the affected character is a beaver.

The localization uses `{CP_TriggerCharacterName}` so the active event panel can say exactly who needs help.

### TimeLimit Subscriptions

`WaitForCure` is a `TimeLimit` node with two subscriptions:

- `CharacterDecontaminated` goes to `SuccessBuff`.
- `CharacterDied` goes to `FailureBuff`.

If neither subscription fires before the deadline, the normal `NextNodeId` sends the event to failure.

Subscriptions are useful when the success condition is not a payment. In this case, the player succeeds by curing the same beaver that triggered the event.

> [!INFO]  
> The currently supported subscription event names in this project are `CharacterDecontaminated` and `CharacterDied`. Both are tied to the event's `TriggerCharacterId`.

### Success And Failure Effects

The success path applies a positive `BuffCharacters` status to adult and child beavers. The failure path applies a stronger negative status to the same character types.

This is the same buff pattern you already saw, but attached to a live event subscription instead of a payment or end-of-timer check.

## Example: Food Wood Dispute 1

[FoodWoodDispute1.blueprint.json](../Root/Blueprints/ChronicleEvents/FoodWoodDispute1.blueprint.json) starts a two-chapter workplace conflict between farmers and lumberjacks. The player can side with farmers, side with lumberjacks, or try to stay neutral.

This is the first event in the guide that uses workplace checks, workplace buffs, flags, cleanup nodes, and an explicit follow-up request.

### Worker Count Gate

`ConditionNodeId` points to `HasEnoughWorkers`, which uses two `WorkplaceWorkerCount` conditions:

- One checks for enough workers assigned to `LumberjackFlag.` workplaces.
- One checks for enough workers assigned to farmhouses.

`WorkplaceWorkerCount` targets buildings with `TemplateNames` or `TemplateNamePrefixes`, then compares assigned worker count with a `Min` and/or `Max` amount.

### Flags For Story State

The first two choices set the continuation flag `{S2}` with `SetFlag`. A failed neutral attempt sets both a failure flag and the continuation flag. A successful neutral attempt sets a success flag but does not request the next chapter.

`SetFlag` supports:

- `Name`, the flag to set or clear.
- `State`, which defaults to `true`. Set it to `false` to remove the flag.

Flags are saved outside the current event record, so later events can use them to decide whether they should appear.

### Workplace Buffs

`BuffWorkplaces` works like `BuffCharacters`, but targets workplaces by template names or prefixes.

In this event, siding with farmers gives farmhouses a working-speed bonus and lumberjacks a penalty. Siding with lumberjacks does the opposite. Neutral success helps both, while neutral failure hurts both.

`BuffWorkplaces` supports `BuffId`, `TitleLoc`, `DescLoc`, `TemplateNames`, `TemplateNamePrefixes`, `Bonuses`, `Days`, and `Category`. Like character buffs, a limited-time workplace buff with `Days` less than or equal to zero removes that buff.

### Cleanup And Follow-Up

After the chapter duration ends, cleanup nodes remove the workplace buffs by using the same `BuffId` with `Days: "0"`.

Some paths then go to `RequestEscalation`, a `RequestNextEvent` node that requests `FoodWoodDispute2`.

`RequestNextEvent` supports an optional `Id`. If you provide one, the active event asks for that specific event to be considered next.

## Example: Food Wood Dispute 2

[FoodWoodDispute2.blueprint.json](../Root/Blueprints/ChronicleEvents/FoodWoodDispute2.blueprint.json) is the follow-up chapter. It reads the choices and flags from chapter 1, then changes the consequences depending on whether the player stays loyal, switches sides, or tries to stay neutral.

This final base example is mostly about scale: it combines flags, conditions, chances, workplace buffs, timers, and cleanup into one larger branching graph.

### Flag-Gated Triggering

The event uses `RequiredFlags: [ "BCEv.FoodWoodDispute.Continue" ]`, so it only appears if chapter 1 asked for the conflict to continue.

It also uses `BlockedFlags: [ "EVENT.FoodWoodDispute1.C1.SY" ]`, which prevents the second chapter after a successful neutral resolution.

Event-level flag checks also support `RequiredFlagsCondition`, `RequiredNoFlagsCondition`, and `BlockedFlagsCondition`. These use `Any` or `All` to decide how multiple flags are combined.

### Remembering Previous Choices

Choice nodes automatically record flags like `EVENT.FoodWoodDispute1.C1.1` and `EVENT.FoodWoodDispute1.C1.2`. This event stores those flag names in string parameters, then uses `Flag` conditions to check what the player picked in chapter 1.

For example, `FarmersWasPreviousChoice` checks whether the player previously sided with farmers. If yes, the event treats siding with farmers again as loyalty. If not, it becomes a switch-side chance.

The `Flag` condition supports one `Flag`, multiple `Flags`, `ConditionType`, and `Expected`. Use `Expected: false` when you want a condition to pass only if the flag is missing.

### Larger Branching Graphs

Each major choice has several possible outcomes:

- Staying loyal gives one side a large bonus and the other a large penalty.
- Switching sides rolls a chance and may either partly work or create a crisis.
- Staying neutral rolls a separate chance and can help or hurt both sides.

Even though the graph is large, each node still does one small job. This is the main trick for keeping big events readable: use clear node IDs, keep each node focused, and let `NextNodeId` describe the flow.

### Final Cleanup

All paths eventually wait for the chapter duration, remove the workplace buffs, and clear the continuation flag with `SetFlag` using `State: false`.

That last cleanup matters. Without clearing the continuation flag, the follow-up event could remain eligible when it should be finished.

At this point, you have seen the core patterns used by the base JSON events: immediate rewards, active timers, payments, chance branches, custom parameters, game stat promises, subscriptions, flags, follow-up chapters, and cleanup.
