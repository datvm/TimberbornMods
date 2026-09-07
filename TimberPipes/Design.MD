# Overview

Pipes get fluid from **pumps** and **tanks**. Fluid leaves through a **dump** (or any consumer with a one-way intake). Mixed goods contaminate that **pipe graph**; simulation on it stops until the player **Flush**es (wipes pipe volumes only; inventories stay).

This is a **sparse pipe network**, not a map-wide fluid sim. Only finished pipe tiles and attached building ports participate. It runs on the game **Tick** (`ITickService.TickIntervalInSeconds`, vanilla 0.6 s).

There is **no throughput cap**. Flow is head difference along **real port edges**, one-way filters, available volume, and free space. Same conductance \(k\) on every orthogonal edge. **No friction.**

**Grid:** Timberborn X/Y horizontal, Z up. 1 tile height = 1 m.

## Units

| Concept | Unit | Notes |
|--------|------|--------|
| Length / elevation | m | 1 m = 1 tile height |
| Volume | m³ | Per pipe cell **always 1 m³** |
| Water good | inventory | **5 goods = 1 m³**, **1 good = 0.2 m³** (integer goods) |
| Tick | s | `TickIntervalInSeconds` |

## Head vs Headlift

Do **not** call tank pressure “headlift”.

| Term | What it is | Who |
|------|------------|-----|
| **Head \(h\)** | Hydrostatic level in meters. Drives **passive** flow. | Pipes, tanks, and the pump **inventory column** |
| **Headlift** | Rated rise a **working** pump may use when **injecting a 0.2 m³ packet** into the **adjacent** pipe. | Pumps with `MaxHeadLift` > 0 |

### Pipe head

\[
h = z + V \qquad V \in [0, 1]
\]

A **full** pipe at \(z\) has \(h = z + 1\). An **empty** pipe at \(z + 1\) also has \(h = z + 1\). They are in equilibrium: **water does not climb**.

A pipe does **not** carry extra pressure from a full tank or pump behind it. Once the adjacent cell is full, that cell’s head is only \(z + V\). That is why a ground tank cannot push over a U-bend, and why packing the whole graph by height was wrong.

### Tank head

\[
h_{\text{tank}} = z_{\text{base}} + \mathrm{clamp}(V / C,\, 0,\, 1) \times H
\]

\(H\) is `FluidBufferBuildingSpec.Height` (tiles). Small Tank: \(H = 2\), ports on both occupied cells, all horizontal directions, **`Open`**.

### Pump column head (passive, one-way)

Inventory is a **1 m column** at the outlet elevation (not the whole tank height):

\[
h_{\text{pump}} = z_{\text{outlet}} + \mathrm{clamp}(V / 1,\, 0,\, 1)
\]

1 good = 0.2 m; **5 goods = 1 m cap**. Extra stock does not raise head. Ports are **`OpenOut`**: fluid can leave into pipes, not return.

Vanilla Folktails Water Pump: `BuildingToPipeSpec` has no `MaxHeadLift`, so **inject is off**. It only equalizes this column with **neighbor** pipes.

### Headlift (discrete inject)

Only if `MaxHeadLift` > 0 and the building can work (`WorkFactor` > 0):

```text
target = the connected neighbor pipe (not any tile in the graph)
if inventory has ≥ 1 good
   AND neighbor has ≥ 0.2 m³ free
   AND (neighbor.z - outlet.z) ≤ ratedMaxHeadLift × workFactor
then
   take 1 good, neighbor.V += 0.2
```

Mechanical: `workFactor` = power efficiency. Workshop/workplace: 1 while working, else 0.

Tanks **do not** propagate headlift. The far side of a tank only sees \(h_{\text{tank}}\).

# Network

| Kind | Graph member? | Role |
|------|---------------|------|
| Transport pipe / valve | Yes | Volume 0–1, contamination, flush |
| Pump / dump / tank | No | Ports only; flood-fill **stops** here |

`PipeRegistry` flood-fills **transport pipes** through connected ports. One tank can touch two graphs; contaminating one does not freeze the other.

Port flow:

```text
A → B iff A allows out and B allows in
```

`Open` both ways. Pump `OpenOut` / dump `OpenIn` are check valves. Paused buildings, and unpowered mechanical buildings, close all ports (`BuildingPipePortState`).

# Buildings

| Building | Ports | Continuous solver volume? |
|----------|-------|---------------------------|
| **Pipe** | `Open` on used faces | Yes, 0–1 m³ |
| **Valve** | `OpenOut` / `OpenIn` | Yes, 0–1 m³ |
| **Tank** | `Open` | Yes, inventory as \(V\) with presented head |
| **Pump** | `OpenOut` | Yes, 1 m column + optional packet inject |
| **Dump** | `OpenIn` | No. Discrete slurp 0.2 m³ from the **neighbor** pipe |

Dump:

```text
if dump has space for 1 good AND neighbor.V ≥ 0.2
then neighbor.V -= 0.2, inventory += 1
```

Wrong good on a tank/pump vs the pipe graph **contaminates immediately** (no phantom fill of the wrong good).

# Simulation

## Rejected model (do not bring back)

**Pack-from-the-bottom / one pool per graph.** All pipe volumes were summed and poured into the lowest \(z\) tiles, ignoring the path. A U-bend did not matter: water on the right appeared on the left. Valves were ignored. UI “headlift” used the tank surface as a graph-global remaining lift (empty far-side risers showed meters of lift they did not have).

That was gravity for a **single open tank**, not a pipe network.

## Current model: edge equalization

Each tick (`PipeFluidSimulator`):

1. Refresh port open/closed (pause / mechanical power).
2. Contamination check; skip flow on contaminated graphs.
3. Quantize tank/pump pending volume to integer goods (±0.2 m³).
4. Discrete **inject** (rated pumps) and **slurp** (dumps) on **adjacent** pipes only.
5. For each clean graph: build nodes (pipes + attached tanks/pumps) and **one edge per connected port pair**.
6. Substeps: compute heads, then `PipeFlowSolver.Equalize`.
7. Write volumes back; stamp fluid id onto newly filled empty pipes.

Flow on an edge:

\[
Q = k\,\Delta t \cdot (h_A - h_B)
\]

Clamped so a node cannot send more than it holds or receive more than free space. One-way edges zero the forbidden direction. Tunables: `PipeSimulationSpec.EqualizeK` (0.8), `Substeps` (2).

**Conservation** on pipes except flush. Horizontal open pipes share. Vertical: water runs **down**; it does not climb unless a **neighbor’s head** is actually higher (tank/pump port at that height, or a pump packet injected into that neighbor).

## Consequences (intentional)

1. **U-bend / inverted arch blocks** when the source only touches ground pipes. Full ground pipe \(h = z+1\) cannot fill the empty riser (\(h = z+1\)). Far side stays dry.
2. **A high tank can gravity-feed downward** from *its own ports*. Connect at the crest (or at \(z=1\) on a 2 m tank) and water will fall down both sides.
3. **Small Tank (\(H=2\))** can fill pipes it actually touches at \(z\) and \(z+1\). It still cannot pressurize a \(z+2\) riser through a full lower pipe.
4. **Worker pump** fills the **outlet layer** (Folktails ports at \(z=1\)) and will not climb the next riser. Extra water stays in inventory once the column is 1 m.
5. **Rated in-pipe pumps** inject only into the **adjacent** tile, and only if that tile’s \(z\) is within headlift. They do not teleport packets onto the highest legal pipe in the graph.
6. **Valves work.** Packing cannot move water through a closed or reverse face.

## Worked examples

**Horizontal 5 pipes, 1 m³ at one end** → ~0.2 m³ each.

**Column 0.7 / 0.6 / 0.5 at \(z=0,1,2\)** → bottom full, middle ~0.8, top empty.

**Ground tank, 10 m³, height 3, only connected to \(z=0\), riser on that pipe** → ground pipe fills; riser stays empty.

**Tank sitting on the crest of an arch** → water drains down both legs; crest empties.

**Worker pump with 2 m³, one-way into a U-bend at \(z=0\)** → adjacent ground fills to 1 m³; crest and far side stay empty; leftover stays in the pump.

**Dump** pulls 0.2 m³ packets from the neighbor pipe only.

**Flush** clears every transport pipe in that graph; tank/pump inventories unchanged.

# Persistence

| Save | Rebuild |
|------|---------|
| Per-pipe volume + fluid id | Ports, graphs (flood-fill on register) |
| Tank/pump pending (sub-packet) | Inventories (vanilla) |
| Contamination (or derive from mixed ids) | Graph ids |

# Code map

| Piece | Role |
|-------|------|
| `PipeRegistry` / `PipeGraph` | Register buildings, connect ports, transport flood-fill |
| `PipeFluidSimulator` | Tick pipeline |
| `PipeFlowSolver` | Heads, equalize, goods ↔ m³, contamination predicate |
| `PipePump` / `PipeTank` / `PipeDump` | Inventory, inject/slurp, apply solver volume |
| `BuildingPipePortState` | Close ports when paused / unpowered |
| `PipeSimulationSpec` | \(k\), substeps, default inject/slurp rates |

# Decisions

| Topic | Decision |
|-------|----------|
| Pipe flow | **Edge equalization only.** Never pack a graph by \(z\). |
| Pipe head | \(h = z + V\). No graph-global extra lift on pipes. |
| Pressure through full pipes | **Does not transmit.** Climbing needs a higher-head **neighbor** (or inject into that neighbor). |
| Pump | 1 m one-way hydrostatic column **and**, if rated, packet inject into the **adjacent** pipe. |
| Vanilla Water Pump | No rated headlift; column only. |
| Dump | Discrete slurp from neighbor. |
| Tank | Bidirectional; head from fill × height; ports `Open`; does not merge graphs. |
| Headlift across tanks | Reset. Far graph sees \(h_{\text{tank}}\) only. |
| Goods | 5 = 1 m³. Quantize pending at ±0.2 m³. |
| Flush | Wipe **pipe** volumes in that graph. |
| Mix | Wrong good vs stored / accepted good contaminates that graph. |
| UI “Headlift” | Remaining **pump** column/rated lift at this \(z\), not tank surface. Volume gauge is the truth for gravity networks. |
