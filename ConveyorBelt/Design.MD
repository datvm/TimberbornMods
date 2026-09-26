# Conveyor Belts

Temporary design. Delete this file when the mod is done.

`D:\Temp\ConveyorBelt` is the previous mod. It is only a reference if an algorithm there is useful (moving discrete goods, or a merger/splitter picking which side to serve). Nothing in that project is a requirement for this one.

Project facts already chosen: id `ConveyorBelt`, version `11.3.0`, new Workshop item (`ItemId` stays empty until the first upload), `BackwardCompatibleTemplateNames` stays `[]`. Folktails buildings and models first.

## Toolbar

Conveyor Belts is its own bottom-bar group, a sibling of Storage. Storage is a root group at order 20, and District Management is 30, so this group uses order 25 and has no parent.

Four material groups sit inside it. Late and End are internal ids. Players see:

- **Log**
- **Plank**
- **Treated Planks** (`ConveyorLateFT`) for Folktails, and **Metal** (`ConveyorLateIT`) for Iron Teeth. An empty group stays hidden, so Iron Teeth does not show Treated Planks.
- **Composite** for both factions (internal id End)

Those groups hold the pieces that actually carry goods along a path: straight belt, corner, and the impermeable belt on Late only.

Merger, splitter, smart splitter, lift up, and lift down are teleporters. They have one version each and sit directly in the Conveyor Belts group, outside the material groups.

Risers are straight vertical belts, so they keep all four speeds. They stay off the toolbar. The player builds one from a lift, including a lift that is still a construction site, and picks the tier then.

## Buildings

Every shape is 1×1×1, has no entrance, and is a mechanical consumer. Timberborn X/Y is the ground plane and Z is up. Local port coordinates go through `BlockObject.TransformCoordinates`.

| Shape | Where it lives | What it does |
| --- | --- | --- |
| Belt | Log, Plank, Late, End | Straight, one-way, line tool. The only piece that takes goods from a building or puts goods into one. |
| Corner | Log, Plank, Late, End | Turns 90° on the ground. Can be flipped. Carries goods at that tier's rate. |
| Impermeable belt | Late | Straight one-way belt. One line of goods. Occupies the whole cell and can be built on. |
| Merger | Conveyor Belts | Teleporter. Several inputs, one output. A good leaves an input piece and appears on an output piece. |
| Splitter | Conveyor Belts | Teleporter. One input, three outputs: left, center, right. |
| Smart splitter | Conveyor Belts | Same shape as the splitter. Each output is a chosen good, Any, or Overflow only. |
| Lift up | Conveyor Belts | Teleporter between a horizontal belt and the cell above. |
| Lift down | Conveyor Belts | Teleporter. Placed on the cell directly above a riser or a lift up. |
| Riser | Built from a lift | Straight vertical belt at the chosen tier. Goods travel along it. |

The straight belt, including the impermeable belt, is the only shape that extracts from or fills a building. Corners, risers, and teleporters connect to other conveyor pieces.

Description and flavor are one text per shape. The material name and items per hour are on that building's entity description.

Lift down's placement requires the cell below to already be a riser or a lift up.

A riser's tier is chosen when it is built. The lift it attaches to has no tier.

## Speed

One belt-speed spec defines the four levels. Every building that carries goods selects one level on its own blueprint: straight belt, corner, impermeable belt, and riser. Teleporters do not select a level.

Log is available immediately and moves 1 item per hour, so it is a starter belt. Late stays at **8** power. The faction material is what the player pays for 50 items per hour. End is the last level and adds the other faction's material. Its power is 16.

| Level | Items/hour | Power on the piece | Science (belt) | Unlock |
| --- | --- | --- | --- | --- |
| Log | 1 | 2 | 0 | Available from the start |
| Plank | 10 | 4 | ~250 | Mid game |
| Late | 50 | 8 | ~750 | Late game |
| End | 100 | 16 | ~1500 | After both faction materials |

Power is not looked up from a shared table while the game runs. Each blueprint writes its draw on `MechanicalNodeSpec`. A log belt's node is 2, a plank belt's node is 4, a late belt's node is 8, and an end belt's node is 16. A corner uses the same power as the belt of its level. The impermeable belt is Late, so its node is 8.

A riser uses the same items/hour as the level it selected. Its power depends on direction:

- Up riser draws double: 4, 8, 16, or 32.
- Down riser draws half: 1, 2, 4, or 8.

Those numbers are on that riser's `MechanicalNodeSpec` too.

Every carrying piece holds at most **5** goods. That cap is the same on every belt, corner, impermeable belt, and riser. Spacing along the piece is 1/5 of its length, so the fifth good fits as the first one leaves. Items/hour is how fast that queue advances. A slower piece still limits a line: a log belt feeding an end belt moves at 1 item/hour.

Teleporters have no items/hour and no good queue. They hand one good across in one step.

## Build cost

Costs below are for one piece. A corner costs the same as the belt of its level. The impermeable belt costs the same as a Late belt. A riser costs the belt of its level plus 4 Gears.

| Piece | Folktails | Iron Teeth |
| --- | --- | --- |
| Log belt | 3 Logs | 3 Logs |
| Plank belt | 1 Plank, 2 Gears | 1 Plank, 2 Gears |
| Late belt | 1 Treated Plank, 2 Gears | 1 Metal Part, 2 Gears |
| End belt | Late belt, plus 1 Metal Block | Late belt, plus 1 Treated Plank |
| Merger, splitter, lift up, lift down | Plank belt, plus 3 Gears (1 Plank, 5 Gears) | Same |
| Smart splitter | Splitter, plus 3 Gears and 3 Metal Blocks (1 Plank, 8 Gears, 3 Metal Blocks) | Same |
| Riser | Belt of that level, plus 4 Gears | Belt of that level, plus 4 Gears |

## Power

Every conveyor building shuts off completely when its power efficiency is below 1. It does not keep running at a reduced rate. At full power a carrying piece runs at the items/hour of the level it selected. Teleporters follow the same rule: below full power they hand nothing across.

## Ports

Same idea as TimberPipes, simpler because every conveyor port is one-way.

- A belt, corner, or riser has an In port and an Out port. A corner's ports are the two legs of the turn. Flip swaps which leg is the output. A riser's ports are down and up, in the riser's direction.
- A belt's Out can give to a building face that accepts goods. A belt's In can take from a building face that provides goods. That building link exists on the straight belt only.
- A lift's ports are one horizontal and one vertical. The good changes cell without traveling through the lift.
- A merger has several In ports and one Out. A splitter and a smart splitter each have one In and three Out ports: left, center (straight ahead), and right, relative to the input.
- A normal building grows ports on its open faces unless a port spec lists them.

## Smart splitter

The smart splitter is a teleporter with the same three outputs as the plain splitter. Each output is saved on the building as one of:

- one good
- **Any**
- **Overflow only**

All three start as Any. In that state it behaves as a plain splitter: one good per tick, round-robin across the outputs that can take it.

When the player sets a port, a good leaves through the first matching output that can accept it:

1. An output set to that good.
2. An output set to Any.
3. An output set to Overflow only, which takes the good only when no specific or Any output took it. That includes a matching output whose downstream piece is full.

Several outputs in the same step share the good round-robin, the same way the plain splitter does.

## Occupancy

Belts and corners occupy **Bottom** and **Path**.

A path occupies Path, so a path and a belt cannot share a tile. Beavers walk around the belt. A TimberPipes pipe occupies Middle, so a pipe can share a tile with a belt.

The impermeable belt occupies the full cell: Bottom, Top, Corners, Path, and Middle. It blocks water, a path, and a pipe on that same cell. It stays stackable, so the player can build on top of it. On that cell it is still one straight one-way belt.

## Models

Belt meshes stay at or below **0.35** in Unity height (Y is up). That is the underside of the Folktails horizontal pipe tube (`BaseMetal` on `HorizontalPipe.Folktails.Model`, bottom at 0.35, top at 0.65). The pipe's leg (`Dam`) is a center post from 0 to about 0.43, about 0.06 wide. A belt may intersect that leg.

Models are animated. The belt surface moves, and the goods in its queue of 5 are visible. Animation rate follows that piece's items/hour.

Animation plays only when the piece is at full power and the player has left animation enabled. Two cases freeze it:

- Power efficiency below 1.
- Mod setting **Animation**, default on, turned off for performance. Main Menu -> Mods -> Settings button next to the mod name.

A straight belt swaps mesh when an end faces a building, the way the TimberPipes one-way valve swaps `#Straight` and `#Building`. The swap is visible in placement preview. The swapped mesh stays under the same 0.35 height.

## Stuck warning

Each carrying piece has its own stuck-warning toggle on that building, saved with the building. It starts off. The player turns it on for the segments they care about.

While the toggle is on, that piece shows a warning when it has a good and cannot hand that good onward. The warning clears when the good moves again. A piece that is shut off for lack of power is a separate case from this warning. Teleporters have no queue, so they have no stuck-warning toggle.

## When we implement

Layout matches the empty project: `Specs`, `Models`, `Components`, `Services`, `UI`, `Root`, `Localizations`. Pure models are what `ConveyorBelt.Tests` covers. `Namespaces.cs` goes away once each namespace has a real type.

`ModDesc.txt` and `ChangeLog.txt` wait until the buildings exist. The update notifier, when added, is `11.3.0` / `113000`.

## Open

These are the gaps. Working answers are in parentheses where a default would unblock the next pass.

- **Science, and power for teleporters.** Materials are set above. Science 0 / ~250 / ~750 / ~1500 is for the belt. (Corner and riser use the same science as the belt of that level. Merger, splitter, smart splitter, and each lift have one science cost. Their power draw is still open.)
- **Whether a belt line passes power through.** (Yes. A transput on the tile, so the player can chain power along the belts. Each piece still has its own `MechanicalNodeSpec`. Efficiency still has to reach 1 or that piece stays off.)
- **Merger sides.** The splitter's three outputs are left, center, and right. (Merger is horizontal only: three inputs and one output. Round-robin, one good per tick. The old junction walker is a reference.)
- **Growing the column.** Lift down can already sit on a riser or a lift up. (From a lift up or a riser, the player can place the next riser. An up riser continues upward.)
- **Goods.** (Solid goods only. Liquids stay on TimberPipes. One good at a time.)
- **Construction sites.** (A belt does not take goods out of a construction-site inventory.)
- **Deleting a belt.** (Goods on it drop as loose stacks.)
- **Building faces with no spec.** (Every open face of a finished building can give and receive, and only a straight belt uses that. A port spec replaces that set when a building should expose only some faces.)
- **Eject and a no-power option.** Left out. The log tier is the early, cheap belt. Say if either should come back.
