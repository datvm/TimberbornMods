# Overview

Pipes carry **goods as fluid** between **pipe tiles**, **tanks**, and **buildings you opt a One-way Valve into**. Mixed goods **contaminate** that pipe graph; simulation on it stops until the player **Flush**es (pipe volumes only; inventories stay).

This is a **sparse network**, not the map-wide water sim. Only finished pipe tiles and the building ports attached to them participate. It runs on the game **Tick** (`ITickService.TickIntervalInSeconds`, vanilla 0.6 s).

**Grid:** Timberborn X/Y horizontal, Z up. 1 tile height = 1 m.

## What this sim is

Vanilla water is the right *family* for **local** motion: each cell talks to **port neighbors**, flow follows **surface height**. Pipes do that on a graph of port edges (pass 1).

Vanilla water is the wrong *detail* to copy: a full column stores extra volume as `Overflow`, which still raises head. That is why world water **piles up** with no rated lift. A pipe tile is a closed **1 m³** cell. Volume must not grow past capacity.

Neighbor gravity alone cannot do communicating vessels: extra water in riser 1 cannot squeeze through a **full** main into riser 2 (no Pascal on partial/empty tiles). That is pass 2: move surface water across a **full core**, along **directed** outflow, onto the **tops**.

Climbing a stack of 1 m buckets needs a rated pump. That is pass 3: a working pipe pump pushes a **filled slug** along outflow and pours into a frontier top that remaining lift can still reach.

Pipes are **not** closed-pipe Pascal through air. A U-bend / inverted arch stays dry on the far side until water has actually filled the **crest**. A **full** bottom main (or a **full** inverted U) *does* equalize its surfaces — that is pass 2, not a bug.

Headlift is a **pump rating in meters** (how high the free surface may sit above the outlet). It is not “how many m³ sit on top.” A thin 2 m standpipe stalls a pump the same as a wide 2 m tank; a huge 1 m tank does not.

# Units

| Concept | Unit | Notes |
|--------|------|--------|
| Length / elevation / head / headlift | m | 1 m = 1 tile height |
| Volume | m³ | Pipe cell **always 1 m³** |
| Water good | inventory | **5 goods = 1 m³**, **1 good = 0.2 m³** |
| Flow | m³ / tick | Pump throughput cap |
| Tick | s | `TickIntervalInSeconds` |

Do **not** switch pumps to kg. Mass is \(\rho V\); it adds nothing.

# Vocabulary

| Term | Meaning |
|------|---------|
| **Head \(h\)** | Surface height in meters. \(h = z + V\) on a pipe. |
| **Headlift \(L\)** | Rated extra meters a **working** pipe pump may add at its **outlet**. Pipe Pump: \(L = 2\). Water Pump has no \(L\). |
| **Remaining lift \(P\)** | How much of \(L\) is still available **here**, along a **filled outflow path**. Never assigned by elevation alone. Used by pass 3 and UI. |
| **Flow cap \(Q_{\max}\)** | How much a working pipe pump may push per tick. Default **0.2 m³** (1 good). |
| **Full** | Pipe volume at capacity. Treat \(V \ge 0.99\) as full. |
| **Full core** | Connected full pipes walked **only** along outflow ports. |
| **Top** | Free surface on a body: first **non-full** tile reached from the core (partial or empty), **or a full tile with no higher full outflow** (\(h = z + 1\)). Empty is a surface, not a corridor. A packed lid is still a top. |
| **Tank surface** | Not headlift. Never call it that. |

# Network

| Kind | In the pipe graph? | Volume in the solver? |
|------|--------------------|------------------------|
| Pipe / elbow / cross / vertical cross / riser | Yes | 0–1 m³ |
| One-way Valve | Yes | 0–1 m³ |
| Pipe Pump / Riser Pipe Pump | Yes | 0–1 m³ |
| Tank | No (ports only) | \(H\) slices, \(C/H\) each, packed bottom-first |
| Other buildings (Water Pump, Discharge, workshops, …) | No | Not in the solver. Packet IO only if a One-way Valve opts in |

`PipeRegistry` flood-fills **transport pipes** through connected ports. Flood-fill **stops** at tanks. One tank may touch two graphs; contaminating one does not freeze the other. Inventories are not tiles: a valve facing a Water Pump does **not** merge that building into the graph.

```text
A → B  iff  A allows out  and  B allows in
```

| Port | Meaning |
|------|---------|
| `Open` | Both ways (ordinary pipe, tank) |
| `OpenOut` | Fluid may leave, not return (valve / pipe pump outflow) |
| `OpenIn` | Fluid may enter, not leave (valve / pipe pump inflow) |
| `OpenIn` + `OpenOut` | Valve / pipe pump (oriented pair) |
| Pause | Does **not** close ports |
| Valve pause | Both ports **Closed** (no in, no out, **no IO**) |

Horizontal valve/pump: **rotate 180** to reverse (not flip). Riser Pipe Pump always lifts (Bottom in, Top out); rotation does not reverse it.

Graphs may run in parallel. No extra graph type is required; walk existing port adjacency.

# Buildings

| Building | Ports | Solver |
|----------|-------|--------|
| **Pipe** / **Elbow** / **Cross** / **Vertical Cross** / **Riser** | `Open` on used faces. Elbow: flip for the other 90°. Cross: four horizontal. Vertical Cross: two opposite sides plus up/down; rotate the horizontal axis. Riser: Bottom/Top. None of these use flip for horizontal vs vertical. | 0–1 m³ |
| **One-way Valve** | Oriented `OpenIn`/`OpenOut`; pause → **Closed** both ways | 0–1 m³, one-way. Optional packet IO (below). |
| **Small Tank** | `Open` | Inventory as \(H = 2\) slices |
| **Water Pump** | None (ordinary inventory) | Not in the graph. Fill pipes with a valve **Extract** (OpenIn toward the pump). |
| **Pipe Pump** | Oriented `OpenIn`/`OpenOut`, 40 hp. Rotate 180 to reverse. Pause does not close ports. | 0–1 m³, \(L = 2\), \(Q_{\max} = 0.2\) m³/tick. Unpowered / paused: no pass 3. |
| **Riser Pipe Pump** | Bottom `OpenIn`, Top `OpenOut`, 40 hp. Always lifts. Pause does not close ports. | Same as Pipe Pump. |
| **Dump** / Discharge | None (ordinary inventory) | Not in the graph. Empty pipes with a valve **Fill** (OpenOut toward the dump). |

Valve IO (after quantize, before the three passes). Default **off**. Point a directed end at a **finished** neighbor with an **enabled inventory** that actually IO’s liquid: **Extract** if it **Gives** a liquid (Water Pump, Badwater Pump, …); **Fill** if it **Takes** a liquid (Discharge, …). Warehouses with no liquid goods, and **tanks**, are not candidates. Transport pipes on that face stay graph connections. Building ports are ignored: occupancy of the facing cell is enough. Pumps are output-only, so they do not get Fill.

```text
Fill (OpenOut toward building, enabled):
  if valve.V ≥ 0.2 AND not contaminated AND building Takes the network good AND has space
  then valve.V -= 0.2, inventory += 1
  Refuse if Takes fails. Never contaminate the building.

Extract (OpenIn toward building, enabled):
  player-picked liquid from that building’s output / takeable stock
  if buffer is empty AND pipe is not full AND building has 1 good
  then inventory -= 1, buffer += 0.2
  if pipe is not full AND buffer > 0
  then pour min(buffer, free space) into the valve tile, AdoptFluid
  Leftover stays in the 0.2 m³ buffer until the pipe has room (so a 0.80 pipe can still take the last good).
  Wrong good vs the graph contaminates the **graph**, not the building.
```

Packet size is **0.2 m³** (1 good). Pause the **valve** to seal both ways (no IO). Pausing the Water Pump does not stop an enabled outlet from draining leftover stock. Destroy the building: IO UI goes away; the valve is a normal one-way pipe.

Wrong good on a **tank inventory** vs the pipe graph contaminates that graph immediately. Pipe pumps are transport tiles; mixing is the same as any other pipe.

# Tick

Each tick:

1. Refresh valve pause → Closed both ways. Other buildings keep spec ports.
2. Contamination check. Contaminated graphs: skip flow, \(P = 0\).
3. Quantize tank pending to goods (±0.2 m³).
4. Valve packet IO (inlet / outlet). Default off.
5. For each clean graph:
   1. **Pass 1** — neighbor gravity.
   2. **Pass 2** — communicating vessels (full core → tops).
   3. **Pass 3** — working pipe pumps.
6. Write volumes. Stamp fluid id onto newly filled empty pipes. Cache \(P\) for UI.

Pass 1 may use several substeps (`PipeSimulationSpec.Substeps`, default 4) if local sharing feels slow. Pass 2 and 3 run **once per tick** after pass 1.

**Conservation** on pipes except flush.

# Algorithm

## Pass 1 — neighbor gravity

Each transport pipe and tank port talks only to **port neighbors**. No search, no remaining lift, no compression through a full cell.

**Do not split this into “equalize” and “gravity” world-passes.** Share and fall are the same rule: water moves to a neighbor with lower **surface**. Vanilla water is also one \(\Delta h\) outflow, then scale.

**Heads:**

\[
h_{\text{pipe}} = z + V \qquad V \in [0, 1]
\]

A **full** pipe at \(z\) has \(h = z + 1\). An **empty** pipe at \(z + 1\) also has \(h = z + 1\). They are in equilibrium: **water does not climb**.

A tank is **\(H\) stacked 1 m slices**, not one node. Capacity \(C / H\) each. Inventory is the **sum**, packed **bottom-first**. Consecutive slices share an internal vertical `Open` edge. A port at world \(z\) attaches to slice \(\mathrm{clamp}(z - z_{\text{base}},\, 0,\, H-1)\). Graphs still **stop** at the tank (two graphs do not merge through it). Pass 2 and 3 **walk those slices** in this graph. Tanks never receive \(P\) and never emit \(P\).

\[
h_{\text{slice}} = z_{\text{slice}} + \mathrm{clamp}(V_{\text{slice}} / (C / H),\, 0,\, 1)
\]

\(H\) is `FluidBufferBuildingSpec.Height` if present, else `BlockObject` size \(z\). Vanilla tanks do not use that spec. Small Tank: \(H = 2\), \(C = 6\) m³, ports on both occupied cells, all horizontal directions, **`Open`**. Bottom-only gravity fills the bottom 1 m (full \(z\) vs empty \(z+1\) is equilibrium). A pipe on the upper tile, or a working pipe pump (\(L = 2\)), fills the top slice. Empty tank is a hole. A tall **full** tank is a packed column and can feed a riser through pass 2.

On each allowed edge \(A \to B\), desired \(Q = k\,\Delta t\,(h_A - h_B)\). Forbidden direction (valve) → 0. Then scale so a node never sends more than it holds or receives more than free space, and apply.

| Setup | \(\Delta h\) | Result |
|--------|----------------|--------|
| Same \(z\), 0.8 vs 0.2 | 0.6 | Share |
| 0.5 at \(z=1\) vs empty at \(z=0\) | 1.5 | Fall |
| 0.5 at \(z=0\) vs empty at \(z=1\) | −0.5 | No climb |
| Full at \(z\) vs empty at \(z+1\) | 0 | No climb |

- Unpowered / paused pipe pump: still one-way; no pass 3. Pause does not close ports.
- **Water Pump** is not in this pass. An enabled valve outlet injects 0.2 m³/tick onto the valve tile; climb still needs a Pipe Pump.
- Conductance \(k\) is the same on every orthogonal edge. **No friction.** Tunables on `PipeSimulationSpec`.
- Greedy-down before sideways is an optional *feel* tweak inside this pass, not a fourth world-pass. Default is one \(\Delta h\).

This pass will **not** move extra water from riser 1 through a **full** main into riser 2. That is pass 2.

## Pass 2 — communicating vessels

Not “any pipe with water, flood the graph.” Only a **full core** can carry a surface move. Empty air is not a corridor.

### Find bodies

For every **unvisited full** transport pipe:

1. Flood through **full** pipes only, along **outflow** ports (`A → B` as above).
2. **Tops** = first **non-full** tiles reached (partial or empty), **and** full core tiles with **no higher full outflow**. **Stop** at non-full. Do not walk through empty. Tiles **above** a non-full top are not in the body. A packed lid is a top (\(h = z + 1\)); do not skip it because it is full.
3. Mark the **full core** visited. Do **not** mark tops as finished bodies (a top is a surface, not a corridor). Each unique top donates **once** per tick.
4. Stop at graph boundaries. Walk tank slices as 1 m tiles. Do not flood into another graph through the tank.
5. If there are **no tops**, the body is sealed → do nothing.

Height of a top:

\[
h = z + V
\]

Empty at \(z = 1\) is \(h = 1.0\). Empty at \(z = 2\) above a full \(z = 1\) is a top at \(2.0\). You will not pour *into* it from a \(1.3\) surface → no climb without a pump.

### Move — one per top, not one per body

Do **not** pick a single global “highest → lowest” pair on the body. With a valve, that pair is often illegal (R2 → R1), and a legal pair (R2 → R3, or R1 → R2) would never run.

Keep the tops. Reachability is **directed**: top \(A\) may send to top \(B\) only if \(A\) can reach \(B\) along outflow through **full** pipes (valves included). From a partial top, enter the core through its outflow into full neighbors. From a **full** top, that tile is already in the core. Stop at the next non-full tiles.

**For each top that has water**, highest surface first:

1. Directed walk as above. Collect other tops (first non-full tiles).
2. Legal destinations = reachable **non-full** tops with **strictly lower** \(h\). A **full** top can donate; it never receives (it is still core, no room).
3. If none, this top does nothing (R2 higher than R1, reverse blocked).
4. Else pick the **lowest** legal destination (deepest hole).
5. Move

   \[
   \Delta = \min(\text{water on source},\; \text{remaining room on dest},\; (h_{\text{src}} - h_{\text{dest}}) / 2)
   \]

   Source \(V\) decreases, dest \(V\) increases. Conservation.

If two tops both pick the same hole, apply sequentially (highest source first) so remaining room is respected.

Pass 2 does **not** replace pass 1 for local neighbor equalize or falling into adjacent holes.

## Pass 3 — pipe pump

Only **working** pipe pumps (`HeadliftPipe`, powered, not paused). \(L = 2\). \(Q_{\max}\) default **0.2 m³/tick**.

**Water Pump** is not a pipe-pump and has no pass 3. **Unpowered pipe pump:** no pass 3.

From **that pump**, same full-or-stop outflow walk as pass 2:

- The pump **transmits** even if it is not full (it is the source). Every other tile transmits only if **full**.
- Stop at the first non-full tiles. Those are the **frontier**. Do not walk through empty. Do not look behind an empty crest.
- Remaining lift starts at \(P = L\) on the pump. Each step:

  \[
  P_{\text{next}} = P - \max(0,\, z_{\text{next}} - z)
  \]

  Sideways and down cost 0. Descent does **not** restore lift. Discard a frontier tile if \(P_{\text{next}} < 0\) (cannot enter it).
- Never write \(P\) onto tanks (slices are not in the UI walk). Pass 3 **may pour into** a non-full tank slice.
- Several pumps: each walks from itself. UI may show \(\max P\) on a tile.

Pour \(\min(Q_{\max},\; \text{volume on the pump tile},\; \text{room on dest})\) into the **lowest** frontier top that remaining lift can still reach. Emptiest is only a **same-\(z\)** tie-break.

Take the volume from the **pump tile**.

Teleport of \(0.2\) from the pump onto a far frontier tile is OK **only** if the path between them is full (hose slug). Not OK through empty.

After the pump shuts off, pass 1 will **not** split 0.2 in riser 1 into 0.1 / 0.1 in a second riser. Pass 2 will, if the main is full and the valve allows that direction.

## UI Headlift

UI “Headlift” on a tile is that tile’s \(P\) from the filled outflow walk (0 if unpowered, contaminated, or the path is not filled). Two ground pipes at the same \(z\) on opposite sides of an empty arch must **not** show the same \(P\).

Do not add \(P\) onto gravity heads in pass 1. Remaining lift is not extra pressure on every pipe at that \(z\).

# Forbidden (do not implement)

1. **Pack the graph by \(z\).** Ignores path and valves. Water appears on the far side of a U.
2. **Remaining lift by elevation** \(P = L - (z - z_{\text{pump}})\) on every pipe at that \(z\). Far side of an arch shows the same headlift and equalizes through air.
3. **Teleport / inject** into the lowest outflow-reachable tile **through an empty crest**.
4. **Vanilla overflow volume** on pipes (\(V > 1\) that still raises head). Piles without a lift cap.
5. **Siphon through air.** Empty far leg stays dry until the crest has water. (A **full** path equalizing surfaces is pass 2, not this.)
6. **Flow-only pumps** (X m³/tick, no \(L\)). That cannot climb; it is a 0-lift pump with a throttle.
7. **“Pump stalls on tank volume.”** Stall on **surface height**, not m³.
8. **One move per body** in pass 2. Use **one move per top** (lowest legal reachable dest).
9. **Undirected** “highest → lowest on the same body.” Valve R1 → R2 must not send R2 → R1.
10. **Extra \(P\) on pass 1 heads.** Climbing is pass 3 only.

# Worked examples

**Horizontal 5 pipes, 1 m³ at one end** → pass 1 shares; about 0.2 m³ each.

**Column 0.7 / 0.6 / 0.5 at \(z = 0,1,2\)** → pass 1: bottom full, middle ~0.8, top empty.

**Ground tank, 10 m³, \(H = 3\), only connected at \(z = 0\), riser on that pipe** → packed slices are a 3 m column; pass 2 fills the riser.

**Empty Small Tank, bottom pipe only, valve outlet from a Water Pump, no pipe pump** → bottom slice fills; top stays empty (half inventory).

**Same layout, pipe pump \(L = 2\)** → pass 3 fills the top slice; tank goes full. A pipe on the upper tile does the same (pass 1 into the top slice).

**Tank on the crest of an arch** → water drains down both legs; crest empties.

**Water Pump, valve Extract into a U-bend at \(z = 0\)** → the valve tile fills at 0.2 m³/tick; adjacent ground equalizes; crest and far side stay empty. Without Extract enabled, pipes stay dry.

**Pipe pump \(L = 2\), \(Q_{\max} = 0.2\), same U-bend** → pass 3 fills the near ground, then the near riser, then the crest (full slug). Far ground stays \(P = 0\) and empty until the crest has water; then pass 1 spills over. Never skip the crest. Unpowered: same as a valve.

**Two risers on a full main, pump off** → pass 1 cannot squeeze through the full main. Pass 2 equalizes the two tops.

**Valve R1 → R2** → R1 may fill R2. If R2 is higher, no reverse. If R2 can still reach a lower R3, R2 pours into R3 (per-top, not global highest→lowest).

**Gappy bottom row** → pass 1 equalizes locally. When the row is **full**, pass 2 sees the first non-full faces (next riser cell, etc.) as tops.

**Dump** is a valve Fill: 0.2 m³ packets from the valve tile into the building.

**Flush** clears every transport pipe in that graph; tank inventories unchanged.

# Persistence

| Save | Rebuild |
|------|---------|
| Per-pipe volume + fluid id | Ports, graphs (flood-fill on register) |
| Tank pending (sub-packet) | Inventories (vanilla) |
| Valve inlet/outlet enable + outlet good | Candidates from neighbors |
| Contamination (or derive from mixed ids) | Graph ids |

Do not save \(P\). Recompute from pumps, power, and volumes.

# Intended code

| Piece | Role |
|-------|------|
| `PipeRegistry` / `PipeGraph` | Transport pipes own the graph. Cache last \(P\) for UI only. |
| Tick pipeline | Pause, contaminate, quantize, valve IO, then passes 1–3 |
| Flow solver | Pass 1 neighbor flux; pass 2 full-core tops; pass 3 pump walk |
| `HeadliftPipe` | Pipe-pump tile: \(L\), \(Q_{\max}\), work factor |
| `PipeTank` / `TankPipe` | Tank inventory + generated `Open` ports |
| `ValvePipe` | `IFinishedPausable`; pause seals the gate; opt-in Fill/Extract packets |
| `BuildingPipePortState` | Valve pause closes both ways; other buildings keep spec ports |
| `PipeSimulationSpec` | \(k\), substeps, default slurp / \(Q_{\max}\) |

# Decisions

| Topic | Decision |
|-------|----------|
| Family | Three passes on the **port graph**. Sparse. Graphs in parallel. |
| Pipe head | \(h = z + V\). Capacity 1 m³. No overflow volume. |
| Pass 1 | Neighbor \(\Delta h\) only. Share / fall. No climb. No \(P\). Do not split into two world-passes. |
| Pass 2 | Full core only. Tops = first non-full **and** full lids (no higher full outflow). **One move per top** to the lowest **legal** (directed, lower \(h\)) dest. |
| Pass 3 | Working pipe pump only. Full-or-stop walk. Pour into lowest frontier within remaining lift. |
| Water Pump | Inventory only. Valve **Outlet** at 0.2 m³/tick. No well column, no pass 1/3. |
| Pipe Pump | Valve + \(L = 2\) + \(Q_{\max}\). Unpowered / paused: one-way, no pass 3. Pause does not close ports. |
| Valve pause | Both ports Closed. **No IO.** Ordinary / tank pause does not close ports. Building pause does not stop valve IO. |
| Climb | Pass 1 never. Pass 2 never into a higher top. Pass 3 only with remaining lift. |
| Headlift | Meters of **outlet surface**, not m³, not kg. |
| Remaining lift | Along **full outflow** from that pump. Drop on rise. Never restore on descent. Never by \(z\) globally. |
| Teleport / inject through empty | **None.** Frontier pour is allowed only on a filled path. |
| Siphon through air | **None.** Full-path surface equalize is pass 2. |
| Tank | Bidirectional 1 m slices; packed bottom-first; pass 1–3 in this graph; does not merge graphs; no \(P\). Not an IO candidate. |
| Valve IO | Opt-in 0.2 m³ packets. Extract keeps a 0.2 m³ buffer so a partial tile can still take the last good. Inlet never contaminates the building. Outlet may contaminate the graph. |
| Goods | 5 = 1 m³. Quantize at ±0.2 m³. |
| Flush | Wipe **pipe** volumes in that graph. |
| Mix | Wrong good vs stored / accepted good contaminates that graph. |
| UI Headlift | Cached \(P\) on that tile, 0 if the filled path does not reach it. |
