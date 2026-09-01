# Crane Heads

Companion to **[Crane](../../Crane/Docs/README.MD)**. Adds **heads** you place on top of a Crane mast. Each finished head changes how that Crane works.

Required dependency: [Crane](../../Crane/Docs/README.MD).

---

## Jib

Extends the Crane's working area from **3×3 to 5×5**. Height is still one tile above the mast.

The building is a 5×5 plus: two 5×1 bars crossing at the center. Only those bars occupy space.

Place the center tile on top of a Crane Section (from the **Crane Heads** group under District Management). The Crane can build the Jib itself once it is in range. Raise the mast first if you still need height: nothing stacks on a Jib.

Select the **Crane** (not the Jib) to see the working-area overlay. It updates when the Jib finishes. A Jib panel has a **Select Crane** button that jumps back to the base.

| | |
|---|---|
| Cost | 4 metal blocks, 4 gears, 500 science |
| Size | 5×5 plus (center row and center column) |
| Effect | +1 horizontal radius (3×3 → 5×5) |

The Folktails model is a hub with four timber arms and inward hanging hooks. Iron Teeth uses the same mesh with metal and dark-brown wood materials.

---

## Trebuchet

A 1×1 Crane Head that launches goods along a **curve**. Horizontal range equals the number of **Crane Sections** (the Crane base does not count). The top of the arc is the Trebuchet's height plus that same section count, so it can land higher than itself, but only if the shot is short enough.

The path must be unobstructed, like a zipline cable. Cells above the map height cap are treated as clear. The landing tile itself must be a valid Recovered Good Stack spot: ground or the top of a stackable building, not a wall face or empty air.

On the Trebuchet panel: pick one or more goods (weight limit 90), pick a landing tile, then set launch to **One time** or **Repeat**. You can set a payload over the limit; the weight turns red, the inventory stops accepting goods, and it will not launch until the weight is at or below the limit. The Crane fills the Trebuchet inventory (payload plus launch cost × capacity). When the stock is enough and the path is still clear, it fires. One time then returns to None; Repeat waits for the next load. If the path is blocked, you get an alert when the inventory becomes ready or when you select the Trebuchet.

The cannon turns to face the chosen tile. The targeting tool highlights the landing floor and draws the shot curve (red if the shot is invalid, with a reason tooltip).

Place it on top of a Crane Section. Nothing stacks on a Trebuchet. It uses the Firework Launcher model.

| | |
|---|---|
| Cost (Folktails) | 8 logs, 4 planks, 4 gears, 400 science |
| Cost (Iron Teeth) | 8 planks, 4 gears, 400 science |
| Size | 1×1×1 |
| Range | 1 tile per Crane Section |
| Weight limit | 90 |
| Cost per shot | 1 Fireworks + 2 planks (spec) |
| Cost stock | 3 shots (`LaunchCostCapacity`) |

---

## Cranetop Windmill

A 1×1×2 Crane Head that turns the **Crane** into a wind-powered generator. Shafts connect at the Crane base. The Crane's mechanical node is 50 hp; the mill scales that by wind and **+10% per Crane Section** (the Crane base does not count). Below 30% wind, or with no mill attached, output is 0.

Place it on top of a Crane Section. Nothing stacks on a Cranetop Windmill. It uses the Weathervane model (Iron Teeth gets the same mesh with IT wood and metal). The vane tracks wind. The mill panel lists current output, then the wind and height breakdown.

| | |
|---|---|
| Cost (Folktails) | 5 logs, 1 plank, 4 gears, 300 science |
| Cost (Iron Teeth) | 8 planks, 4 gears, 300 science |
| Size | 1×1×2 |
| Crane power | 50 hp (`MechanicalNodeSpec`) |
| Bonus | +10% per Crane Section |
| Min wind | 30% |

---

## Adding another head

Heads are buildings with `CraneHeadSpec`. `CraneHeadComponent` marks the building as a crane head: it names the attachment cell and tells `CraneTowerHead` when it is on top of a mast. It does not change how the Crane works.

`CraneTowerHead` is decorated onto every `CraneComponent`. `CraneHeadStructureService.HasHead` is the query for whether a tower currently has one.

Put the actual effect on a second component with its own spec — `CraneHeadJib` / `CraneHeadJibSpec` for range, `CraneHeadTrebuchet` / `CraneHeadTrebuchetSpec` for launching rubble, `CraneHeadWindmill` / `CraneHeadWindmillSpec` to toggle `CraneWindmillGenerator` on the Crane. When that head is finished and attached, it registers with the Crane. See [Crane](../../Crane/Docs/README.MD#range-modifiers).

Heads are crane parts (`ICranePartComponent`), so they share the **Select Crane** panel with Crane Sections.
