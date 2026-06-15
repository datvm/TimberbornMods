# Beaver Chronicles

Beaver Chronicles lets Timberborn mods add story events: small pop-ups, choices, rewards, penalties, delayed outcomes, follow-up chapters, and temporary effects on beavers, workplaces, or water sources. The base mod already includes examples such as the starting bonus, drought preparation, first death, unhoused beavers, contamination rumors, and the food-versus-wood dispute.

This guide is written for people who want to make events without writing code. Most Beaver Chronicles events are plain JSON files, so you can create them with a text editor, test them in-game, and gradually build up more interesting stories from simple pieces.

If you are comfortable with C#, you can still go further. The JSON system is built on condition evaluators and handler nodes, and custom code can add new behavior when JSON alone is not enough. The main guide will focus on JSON first, because that is the easiest place to start and the best way to understand how events are assembled.

## What This Guide Covers

We will build up the concepts using examples from the current events included in the base mod:

- What an event file looks like.
- How an event decides when it can appear.
- How parameters let you reuse values like goods, amounts, chances, durations, and flags.
- How nodes create choices, give rewards, apply buffs, wait for time to pass, roll chances, and request follow-up events.
- How localization keys connect the JSON to text shown in-game.

For the full catalog of available condition types and handler nodes, see [References.MD](References.MD).

## Example: Start Bonus

The first example is [StartBonus.blueprint.json](../Root/Blueprints/ChronicleEvents/StartBonus.blueprint.json). This event appears near the beginning of a game and asks the player to choose one starting benefit: berries, water, logs, or an extra child beaver.

This is a good first event to study because it is small. It has one player-facing choice, a few possible rewards, and one condition that disables some options if the settlement does not have a District Center.

### Event Declaration

Every event file starts with a `ChronicleEventSpec`. That is the wrapper that tells Beaver Chronicles, "this JSON describes one chronicle event."

`Id` is the event's unique name. In this file it is `StartBonus`, which is how other events or code can refer to it.

`TitleLoc` is the localization key for the event title. The actual text lives in the localization file ([`Localizations/enUS.csv`](../Localizations/enUS.csv))

`IsMini` marks the event as a mini event.

> [!INFO]  
> A mini event must resolve immediately, so it cannot use a `TimeLimit` node or stay active across days. The tradeoff is that mini events are allowed to trigger even while a non-mini event is already active. Beaver Chronicles only allows one active event at a time.

### Conditions

`Conditions` describes when the event is allowed to appear.

`Sources` tells the event system which game moments can trigger the event check. `StartBonus` uses `NewHour`, so the game may consider this event when a new in-game hour begins. For all sources, refer to [Trigger Sources](References.MD#trigger-sources).

`Weight` controls how strongly this event is favored when multiple events are available. This event uses the maximum integer value, a special value that means "always pick this one if it is available." If there are multiple events with this weight, the game will choose randomly among them.

`MinDay` prevents the event from appearing too early. `1.25` means the event waits until a little after day 1 has started. There are also `MaxDay`, `MinCycle`, and `MaxCycle` fields that work the same way but with different time units.

> [!INFO]  
> In-game, a "day" is a decimal number (called float) that starts at 1.00 when the first day begins, and increases to under 2.00 as the second day approaches.

### Parameters

`Parameters` is a small value bank for the event. Instead of repeating the same goods and amounts inside every node, the event stores them once and refers to them later.
- `Strings` contains text values used by the event logic. In `StartBonus`, these are the IDs for berries, water, logs, and child beavers.
- `Ints` contains whole-number (integer) values. Here they are the reward amounts: 20 berries, 20 water, 20 logs, and 1 child beaver.
- There are also `Floats` for decimal numbers, used by other events for things like chances and durations.

Inside nodes, placeholders such as `{S1}` and `{I1}` pull from these parameter lists. `{S1}` means the first string, `{S2}` means the second string, `{I1}` means the first integer, and so on. Floats would use `{F1}`, `{F2}` for number but you can also use {%1} and {%2} to show them as percentages (0.15 becomes 15%).

> [!IMPORTANT]  
> Unlike most programming languages, I intentionally made most indexes 1-based instead of 0-based. This is because the placeholders are meant to be human-friendly, and 1-based numbering is more intuitive for non-programmers.

### Text Placeholders

Event text can use two kinds of placeholders.

Beaver Chronicles placeholders use curly braces. They are replaced before the text is shown:

- `{S1}`, `{S2}`, etc. use values from `Parameters.Strings`.
- `{I1}`, `{I2}`, etc. use values from `Parameters.Ints`.
- `{F1}`, `{F2}`, etc. use values from `Parameters.Floats` as decimal numbers.
- `{%1}`, `{%2}`, etc. use values from `Parameters.Floats` as percentages, so `0.15` becomes `15%`.
- `{CP_Name}` uses a custom parameter from the current event record.
- `{GS_StatName}` uses a game stat value. For available stats, see [ModdableTimberborn Game Stats](../../ModdableTimberborn/Docs/GameStats.MD#built-in-stat-quick-reference).

Timberborn mixed-text tags use double square brackets. They control formatting or insert game UI elements:

- `[[C]]` centers the text that follows.
- `[[+]]` marks a positive line.
- `[[-]]` marks a negative line.
- `[[good:{S1}:{I1}]]` shows a good icon and amount, after `{S1}` and `{I1}` are replaced.
- `[[time:{F1}d]]` shows a duration.
- `[[icon:UI/images/game/ico-child]]` shows an icon.

You can combine both systems. For example, `[[+]]Gain [[good:{S1}:{I1}]]` first replaces `{S1}` and `{I1}`, then Timberborn displays the formatted reward line.

### Nodes

`Nodes` is the event flow. Think of it as a small map: each node does one thing, and some nodes point to another node. Nodes are identified by their `Id`, which must be unique within the event. They don't have to be in any particular order in the JSON, because the flow is determined by the `NextNodeId` fields that point to other nodes by their `Id`.

- `StartNodeId` chooses where the event begins. `StartBonus` starts at the node named `Choice`.
- `Items` contains every node used by the event. Each node has an `Id`, a `Type`, and `Data`.
  - `Id` is the node's local name. Other nodes can jump to it with `NextNodeId`.
  - `Type` chooses what kind of node it is. For example, `Choice` shows text and options, `Give` grants goods or spawns, and `Condition` checks whether something is true. For the full list of node types, see [Node Types](References.MD#node-types).
  - `Data` contains the settings for that specific node type. Again, see [Node Types](References.MD#node-types) for the expected data fields for each type, while this guide will cover nodes used by the example events.

> [!INFO]  
> If for whatever reason the next node ID is undefined (called `null`), the event will conclude. For example, if you forget to set `NextNodeId` on a choice option, the event will end when the player picks that option. This could be intentional, for example, on an option that naturally should end the event.

### The Choice Node

The `Choice` node shows a block of text and a list of options for the player to choose from. The event waits until the player picks an option, then jumps to the next node specified by that option. By default, the content is also written to the Chronicle Record automatically. The `Choice` node uses these fields in the data:

- `TextLoc` is the localization key for the main event text.
- `TopImage` (true/false) or `TopImagePath` (string): specifies whether to show an image on the top. Use `TopImagePath` to specify a path, or set `TopImage` to true to use the default path: `Resources/Chronicles/<EventId>_Top`.
- `SideImage` and `SideImagePath`: similar to `TopImage` and `TopImagePath`.
- `DefaultOption` marks which option is selected by default.

> [!INFO]  
> The `DefaultOption` is also 1-based.

- `Options` lists the buttons the player can choose from. Each option has:
  - `TextLoc`: for the button text.
  - `NextNodeId`: for the node to jump to when the player chooses that option.
You can also specify the following optional properties:
  - `NoteLoc`: extra texts show below the buttons, often used for specific rewards or consequences.
  - `EnabledConditionNodeId`: if specified, a `Condition` node whose ID is referenced here will determine whether the option is enabled or disabled. See details on Condition nodes below.
  - `DisabledTextLoc`: if the option is disabled, this text will be appended to the note, typically used to explain why the option is unavailable.

> [!TIP]  
> You can also leave the Options empty for a simple "I see" option. It will jump to the `NextNodeId` instead.

In the example, the first three options have `NoteLoc`, `EnabledConditionNodeId`, and `DisabledTextLoc`. These options are only enabled if the `HasDistrictCenter` condition node passes. If it does not pass, the event shows the disabled text instead.

### The Condition Node

`HasDistrictCenter` is a `Condition` node. It is not shown directly to the player. Instead, the choice options use it to decide whether some buttons should be enabled.

> [!INFO]  
> A `Condition` node is quite special. While it can be used as a "branching" node, it is also reused by other nodes such as the above choice options. The event's trigger `Conditions` can even refer to a `Condition` node by its ID to gate the entire event behind that condition.

For the full list of condition types and their data, see [Condition Types](References.MD#conditions).

`HasDistrictCenter` uses the `HasBuilding` condition type, which checks whether the settlement has a building that matches certain criteria. `TemplateNamePrefixes` contains `DistrictCenter.`, which means the condition checks whether the settlement has a building whose template name starts with that prefix.

This keeps the player from choosing a stored-good reward when there is no District Center available to receive the goods.

### The Reward Nodes

The nodes named `C1`, `C2`, `C3`, and `C4` are all `Give` nodes.

`C1`, `C2`, and `C3` use `Items` to give goods to the settlement. They use the string parameters for the good IDs and the integer parameters for the amounts.

`C4` uses `Spawns` instead of `Items`, so it creates a child beaver rather than adding a stored good.

These reward nodes do not need `NextNodeId` because the event ends after the reward is given.

## Next Guide

Continue with [EventGuides.MD](./EventGuides.MD) for more event explanations and JSON authoring guides.

