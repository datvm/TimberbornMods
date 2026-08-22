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

The Folktails model is a hub with four timber arms and inward hanging hooks. Iron Teeth models are not in yet.

---

## Trebuchet

A 1×1 Crane Head that launches rubble away instead of storing it in the Crane. Gathered recovered goods are thrown in the Crane's facing direction and land as a new pile (or leave the map if that landing is off the edge).

Place it on top of a Crane Section. Nothing stacks on a Trebuchet. It uses the Firework Launcher model.

| | |
|---|---|
| Cost (Folktails) | 8 logs, 4 planks, 4 gears, 400 science |
| Cost (Iron Teeth) | 8 planks, 4 gears, 400 science |
| Size | 1×1×1 |
| Effect | Launches rubble 10 tiles away |

---

## Adding another head

Heads are buildings with `CraneHeadSpec`. `CraneHeadComponent` marks the building as a crane head: it names the attachment cell and tells `CraneTowerHead` when it is on top of a mast. It does not change how the Crane works.

`CraneTowerHead` is decorated onto every `CraneComponent`. `CraneHeadStructureService.HasHead` is the query for whether a tower currently has one.

Put the actual effect on a second component with its own spec — `CraneHeadJib` / `CraneHeadJibSpec` for range, `CraneHeadTrebuchet` / `CraneHeadTrebuchetSpec` for launching rubble. When that head is finished and attached, it registers with the Crane. See [Crane](../../Crane/Docs/README.MD#range-modifiers).

Heads are crane parts (`ICranePartComponent`), so they share the **Select Crane** panel with Crane Sections.
