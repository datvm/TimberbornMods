# Overview

## Scope

Pipes get fluid from certain buildings: Pumps or Tanks. The only way to exit is a Water Dump or a building that consumes such fluid.

Mixed fluid is not allowed. If mixed fluid enters a **pipe** network, that network is contaminated and **simulation on that network stops** until the player flushes. Flush is a player button that **destroys all pipe fluid in that network** and marks it clean again (time for the player to fix the setup). **Internal building cells are not part of the pipe network** for contamination: a Tank can sit between two pipe networks; if one side contaminates, the other side keeps running.

This is a **sparse pipe network**, not a full-map fluid sim. Only built pipe tiles and attached building cells participate. Simulation runs on the game **Tick** (fixed `TickIntervalInSeconds`, affected by pause/speed via `Time.timeScale` → `Ticker`), not on raw Unity `Update`.

There is **no throughput limit** for now; flow is driven only by head difference, one-way ports, available volume, free space, and pump headlift. **No friction**: all orthogonal (90°) edges use the same conductance (ideal pipes).

## Units

| Concept | Unit | Notes |
|--------|------|--------|
| Length / elevation | m | 1 m = 1 tile height |
| Volume | m³ | Stored per network cell; **pipe cell capacity always 1 m³** |
| Water good | inventory item | **5 Water good = 1 m³**, so **1 Water good = 0.2 m³** (integer goods only) |
| Tick step | s | `ITickService.TickIntervalInSeconds` (vanilla 0.6 s) |

## Head vs Headlift

These are **two different quantities**. Do not use “headlift” for tanks or passive pressure.

| Term | What it is | Who has it |
|------|------------|------------|
| **Head \(h\)** | Free-surface height of fluid (hydrostatic “pressure” expressed as meters of water column). Drives **passive** equalization. | Pipe cells and Tanks. Pipes: \(h = z + V\). Tanks: presented from inventory fill × building height. |
| **Headlift** | **Active budget** a working pump may add: max rise it can force when injecting into the network. Rated × work factor. | **Pumps only** |

**Head ≈ pressure (in height units).** Higher free surface ⇒ higher drive into lower/emptier neighbors.

### Head (passive)

- Per normal pipe cell: \(h = z + V\) with \(V \in [0, 1]\) (capacity always 1 m³).
- Fluid equalizes toward the same head across open connections (same \(k\) on all orthogonal edges).
- Tanks present a free-surface head from how full their inventory is (see Tanks). That can push **out** or accept **in** — purely passive, no powered lift.
- **Pumps and Dumps do not present a passive free-surface head.** They are discrete injectors / sinks.

### Headlift (pumps only)

- **Pumps** are the only devices that *provide* headlift.
- Valves do **not** reset or consume headlift; they only filter direction via ports.
- **Effective MaxHeadLift** is dynamic:
  - Always: one-way valve behaviour of the pump port is active (`OneWayOut`).
  - Headlift only while the building can work: mechanical power efficiency and/or beaver work.
  - Example: rated 2 m headlift at 75% power → **1.5 m** effective headlift.
  - Vanilla Water Pump: headlift only while a worker is working it.
- Headlift is checked at the moment of discrete injection (see Pumps).

**Important clarification (Headlift across Tanks):**  
Tanks **reset / do not propagate headlift**. Headlift is local to the pump that generates it and is fully consumed when water enters the tank. A network on the opposite side of a tank receives **only** the tank’s current passive free-surface head (`h_tank`). It receives zero active headlift or “extra pressure” from the input side. Tanks are level equalizers and volume buffers, **not** pressure or headlift transmitters.

# Network Map

Each coordinate (`Vector3Int`) in the map stores:

| Field | Purpose |
|-------|---------|
| **Cell kind** | `Pipe` (normal pipe / valve segment) or `InternalBuilding` (pump, dump, tank footprint, etc.) |
| **Shape** | Per-face ports (connectivity + one-way) |
| **Volume** \(V\) | Fluid on this cell (m³); pipe cells in **\[0, 1\]**. Pump/Dump cells hold no continuous volume for flow. |
| **Elevation** \(z\) | Cell Z (m) |
| **Head** \(h\) | Derived (see Head vs Headlift / Tanks). Only meaningful for Pipe and Tank cells. |
| **Fluid type** | Clean / contaminated / empty |
| **Graph id** | Connected component among **Pipe** cells only (internals are barriers / hubs, not graph members for contamination) |

## Cell kind (Pipe vs InternalBuilding)

| Kind | Role |
|------|------|
| **Pipe** | Normal pipes, valves. Belong to a **pipe network** graph. Contamination and flush apply here. |
| **InternalBuilding** | Pump / dump / tank / manufactory cells. **Not** flooded into a pipe graph for contamination. Still exchange volume with adjacent pipe cells through ports (pumps/dumps via discrete actions; tanks via continuous flow). |

**Graph rebuild (pipe networks only):** flood-fill through `Pipe` cells that share non-`Closed` faces. **Stop at `InternalBuilding`.**  
So one tank can touch two pipe networks; they stay **two graphs**. Contaminating one does not contaminate the other. The tank keeps its own inventory/volume rules.

## Shape (ports)

Fixed array of face states (90° ring: 0°/90°/180°/270°; plus vertical up/down for risers as needed).

```text
enum PipePort {
  Closed,     // no connection on this face
  Open,       // bidirectional (neutral)
  OneWayOut,  // fluid may leave this cell through this face
  OneWayIn,   // fluid may enter this cell through this face
}
```

### Flow rule between neighbors A and B

```text
flow A → B allowed iff
  A.faceTowardB is Open or OneWayOut
  AND B.faceTowardA is Open or OneWayIn
```

| Setup | A → B | B → A |
|-------|-------|-------|
| Both `Open` | yes | yes |
| A `OneWayOut`, B `OneWayIn` or `Open` | yes | no* |
| A `OneWayIn`, B `OneWayOut` or `Open` | no* | yes |

\*Reverse blocked when the one-way side forbids it; plain `Open` neighbor does not cancel a valve’s `OneWayOut`/`OneWayIn`.

### Examples

| Tile | Shape (relevant faces) |
|------|-------------------------|
| Straight pipe E–W | 0° `Open`, 180° `Open` |
| Straight valve flow E | 0° `OneWayOut`, 180° `OneWayIn` |
| 4-way junction | 0°/90°/180°/270° `Open` |
| Pump → pipe | toward pipe `OneWayOut` |
| Dump ← pipe | toward pipe `OneWayIn` |
| Tank footprint cell | faces between tank cells + ports to pipes **`Open`** (neutral) |

**Rule:** each cell describes **its own** faces. Normal pipe faces are `Open` when connected; only valves / directed building ports use one-way.

### Also maintain

- Edges from non-`Closed` faces with a neighbor.
- **Pipe graphs** rebuilt on place/delete/finish (dirty flag).
- UI snapshot after each sim step.

# Buildings

**5 Water good = 1 m³** (1 good = 0.2 m³). Goods are **integers**; conversion only in **0.2 m³** steps.

## Building roles (simplified)

| Building | Role | Port to external pipes | Continuous volume? | Active energy |
|----------|------|------------------------|--------------------|---------------|
| **Pump** | Discrete one-way source | **`OneWayOut`** | No (inventory only) | **Headlift** while working |
| **Dump / consumer** | Discrete one-way sink | **`OneWayIn`** | No (inventory only) | none |
| **Tank** | Bidirectional buffer + level equalizer | **`Open`** | Yes (presented head from inventory) | **none** |
| **Pipe** | Transport | `Open` on used faces | Yes (0–1 m³) | none |
| **Valve segment** | Direction filter | `OneWayIn` / `OneWayOut` | Yes (0–1 m³) | none |

```text
Pump inventory  --(discrete inject, OneWayOut)-->  pipe
Dump inventory  <--(discrete slurp, OneWayIn)---  pipe
Tank cells      <--Open continuous exchange---->  pipe
```

**Neutrality is not “headlift = 0 with a valve.”** A valve with zero headlift still blocks reverse flow. A tank must use **`Open`** so fluid can refill the tank when network head is higher.

Player-facing language (optional UI later):

- Pump: *check valve out + powered lift*
- Dump: *check valve in*
- Tank: *open flange / no check valve*

## Pumps (discrete source)

Vanilla Water Pumps and mod In-pipe Pumps (some mechanical, some worker-driven).

- Registers 1× `InternalBuilding` cell for topology / ports only. **Does not hold continuous volume** for the flow solver.
- Port toward pipes: always `OneWayOut`.
- Inventory holds integer Water goods (capacity is a balance choice per building, e.g. 10–50).
- **Injection rule** (while working and effective headlift > 0):

```text
if inventory has ≥ 1 Water good
   AND target pipe cell has free space ≥ 0.2 m³
   AND (target_z - pump_z) ≤ effectiveMaxHeadLift
then
   inventory -= 1
   target.V += 0.2
```

- **Effective MaxHeadLift** = `ratedMaxHeadLift × workFactor`
  - Mechanical: `workFactor` = power efficiency.
  - Worker-driven (vanilla Water Pump): `workFactor` = 1 while beaver is working, else 0.
- Rate limit: optionally cap how many goods can be injected per tick (tunable).
- If the target pipe is full or the rise exceeds headlift, the good simply stays in inventory. No special internal buffer is needed.
- Once injected, the volume is continuous pipe fluid and never returns to the pump.

## Water Dump and other consumers (discrete sink)

```text
if inventory has space for ≥ 1 Water good
   AND connected pipe cell (via OneWayIn) has V ≥ 0.2
then
   pipe.V -= 0.2
   inventory += 1
```

- Registers 1× `InternalBuilding` cell for topology / ports only.
- Port toward pipes: always `OneWayIn`.
- No continuous internal volume. No free-surface head.
- Rate limit: optionally cap how many goods can be slurped per tick.

## Pipes

| Variant | Segments |
|---------|----------|
| Straight | 0° and 180° |
| Elbow (2 outlets) | 0° and 90° (any adjacent pair) |
| Junctions (3 outlets) | 0°, 90°, 180° (any three of the four faces) |
| Junctions (4 outlets) | 0°, 90°, 180°, **270°** |

`Pipe` kind, capacity 1 m³, usually all used faces `Open`.

## Valve

- **Straight only** (horizontal and vertical).
- Shape: `OneWayOut` / `OneWayIn` on the two ends.
- Does not reset headlift (headlift is not a network “token”; valves only filter direction).

## Storage (Tanks) — multi-tile

Tanks are the only buildings that keep a continuous free-surface model.

- Ports to pipes are **`Open`** (neutral — no check valve).
- **No headlift** (no powered push above free surface).
- **Inventory owns mass**; cells present **hydrostatic head** from fill × height.

```text
[Pipe net A] <--Open--> [Tank InternalBuilding cells...] <--Open--> [Pipe net B]
                              ^
                              inventory is mass source of truth
                              presented head = z_base + fillFraction * heightTiles
```

### Example: 3×3 footprint tank

- **9 cells**, each `InternalBuilding`, faces between them `Open` (a small open “room”).
- **Inventory is authoritative** for total water goods (can be far more than 9 m³ of goods).
- **Pipe-facing ports** are `Open` and connect adjacent `Pipe` networks; flood-fill for contamination **stops** at these cells, so left and right pipe systems stay separate graphs even though volume may exchange both ways with the tank.

### Volume / head presentation

```text
fillFraction = storedGoods / capacityGoods          // e.g. 600/1200 = 0.5
columnHeight = fillFraction * buildingHeightTiles   // e.g. 0.5 * 3 = 1.5 m
h_tank = z_base + columnHeight
```

- Port exchange uses normal passive flow: \( Q \sim k \cdot (h_i - h_j) \).
- Accumulate **net** volume delta across all tank ports.
- When accumulated delta crosses ±0.2 m³, add/remove 1 good from inventory. This prevents floating-point drift.
- Cell `V` values (if used) are only a small visual / exchange buffer. Inventory remains the single source of truth.

### Critical tank behaviours (unchanged)

1. **Two networks connected to the same tank remain two separate graphs.**  
   Flood-fill stops at `InternalBuilding`. Contamination / flush on one side has no effect on the other.

2. **Headlift does not propagate through tanks.**  
   The opposite network only ever sees the current passive `h_tank`.

3. **Filling limit.**  
   A pump can only raise a tank’s free surface up to the maximum head its headlift can achieve.  
   Example: tank base = 5 m, height = 3 m (max \( h = 8 \)). Pump effective head = 6 m → max reachable fill ≈ 33 %.

4. **Gravity-fed downstream.**  
   If a tank is kept high by an upstream pump, a downstream network that stays at or below current `h_tank` and has no uphill sections can run entirely pump-free.

# Simulation

Simple, predictable: conserved volume on pipe cells, head equalization, discrete pump inject / dump slurp. Ideal conductance on orthogonal edges only. No \( Q_{max} \).

## Driving metrics

| Metric | Role |
|--------|------|
| **Volume \(V\)** | Pipe cell fill 0..1 |
| **Head \(h = z + V\)** (pipes) | Passive equalization target |
| **Tank presented head** | From inventory fill × height |
| **Conductance \(k\)** | Same for all allowed orthogonal (90°) edges |
| **Effective MaxHeadLift** | Pump-only injection budget |
| **\(\Delta t\)** | Tick interval (1–2 substeps optional) |

## Per-tick pipeline

```text
1. Topology (if dirty)
   - Rebuild Pipe graphs: flood-fill Pipe cells only; stop at InternalBuilding.
   - Link edges Pipe↔Pipe and Pipe↔Internal for flow, but graph id only on Pipe.

2. Contamination
   - Detect mix on a Pipe graph → mark that graph contaminated; skip its flow.
   - InternalBuilding cells are not in the graph; other graphs keep running.

3. Building discrete actions (InternalBuilding)
   - Pump: while working and conditions met → inject 0.2 m³ packets (up to rate limit).
   - Dump/consumer: while space and V ≥ 0.2 → slurp 0.2 m³ packets (up to rate limit).
   - Tank: (optional) quantize any previously accumulated port delta into goods.

4. Network flow (continuous)
   - Skip contaminated Pipe graphs.
   - Passive edges between Pipe cells and between Pipe ↔ Tank: Q = k * (hi - hj), port filters, clamp by V / free space.
   - No continuous flow involving Pump or Dump cells (they already acted discretely).

5. Publish UI snapshot.
```

## Equilibrium rules

1. Conservation on pipes (except flush wipe).
2. Equal head across open pipe connections (ideal \(k\)).
3. Horizontal equal pipes → share volume equally.
4. Vertical: gravity via head; pumps force discrete injection up to effective MaxHeadLift.
5. Contamination is **per Pipe graph**, stops at InternalBuilding.
6. Tanks exchange only via passive head (no headlift); ports `Open`.
7. Pumps and dumps never participate in continuous equalization.

## Worked examples

### Horizontal 5 pipes

Inject 1 m³ at one end → settles to **0.2** each.

### Pump + vertical riser

Pump at z = 0, empty riser pipe at z = 1, rated headlift 2 m, goods available.

- While working: each tick the pump may inject +0.2 m³ into the riser (rise of 1 m ≤ 2 m headlift and space available).
- Inventory decreases by 1 good per successful injection.
- Once the riser (and connected network) is full, injection stops and goods stay in the pump.

### Contaminated side of a tank

- Left pipe graph mixes → contaminated, frozen, flush wipes **left pipe volumes only**.
- Tank inventory unchanged; right pipe graph still simulates.
- Isolation comes from **graph membership** (InternalBuilding barrier).

### Flush

- Player presses Flush on a contaminated network after fixing inputs.
- **Instant:** all `Pipe` cell volumes in that graph → 0, fluid type clean, resume sim.
- `InternalBuilding` inventories **not** destroyed by pipe flush.

### Tank filling limit

Tank base = 5 m, height = 3 m (max \( h_{tank} = 8 \)).  
Pump on Network A can push to effective head = 6 m.  
→ Network A can only fill the tank up to approximately 33 % (`columnHeight ≈ 1 m`).

### Gravity-fed Network B

Tank kept near \( h_{tank} = 8 \) m by strong Pump A.  
Network B has no uphill sections and all destinations ≤ 8 m.  
→ Network B can operate **pump-free** while the tank stays high.

# Persistence

**Save only what cannot be rebuilt:**

| Save | Rebuild on load |
|------|-----------------|
| Per-cell **volume** (and fluid type if non-empty/contaminated) for Pipe cells | Shape, kind, ports from buildings/pipes placed in the world |
| Contaminated flag can be derived from fluid type on any cell in the graph | **Graph ids** — always rebuild flood-fill |
| Building inventories | Already saved by vanilla entity persistence |

Topology/shape/kind come from finished buildings → **do not save graph structure**. On load: register all cells from buildings → rebuild graphs → apply saved volumes onto coordinates.

# Implementation shape

| Piece | Role |
|-------|------|
| `PipeNetworkMap` | Coord → cell (kind, shape, V, fluid type) |
| `PipeGraphRegistry` | Flood-fill Pipe-only graphs; dirty flag |
| `PipeSimulator` | `ITickableSingleton` |
| Building components | Register InternalBuilding cells + ports; discrete inject/slurp (pumps/dumps); presented head + quantization (tanks); dynamic headlift (pumps) |
| Specs | rated MaxHeadLift (pumps), injection/slurp rate, tank height/capacity |

Main-thread singleton first; parallelize only if needed.

## Tunables

| Tunable | Intent |
|---------|--------|
| Equalize \(k\) | Settle short nets in ~1–3 ticks |
| Substeps | 1–2 per tick |
| Rated MaxHeadLift | Per **pump** building only |
| Injection / slurp rate | Max goods (0.2 m³ packets) per tick while working |

# Decisions (resolved)

| Topic | Decision |
|-------|----------|
| Pump model | Discrete one-way injector. Inventory → 0.2 m³ packets into pipe when space + headlift allow. No continuous internal volume for flow. |
| Dump model | Discrete one-way sink. Pipe → 0.2 m³ packets → inventory when space allows. |
| Tank model | Continuous free-surface. Inventory owns mass; presented head = fill × height; ports `Open`; multi-cell footprint isolates graphs. |
| Head vs headlift | Head = passive pressure height (pipes + tanks). Headlift = pump-only active injection budget. |
| Headlift across tanks | Tanks **reset headlift**. Only passive `h_tank` is visible on the far side. |
| Conversion factor | Fixed: 5 goods = 1 m³ (1 good = 0.2 m³). |
| Flush | Instant player button: wipe **pipe** volumes in that graph + clear contamination. Inventories untouched. |
| Pump power | One-way always; headlift × power efficiency or worker active. |
| Connectivity | Orthogonal 90° only (no diagonals). Same \(k\) on all edges; no friction. |
| Save | Pipe volumes (+ fluid type); rebuild topology/graphs on load. |
| Tank filling limit | A pump can only raise a tank’s free surface to the head level its headlift can achieve. |
| Gravity-fed networks | A high tank can gravity-feed a downstream network that stays at or below current `h_tank` and has no uphill sections. |
| Graph isolation | Flood-fill stops at every `InternalBuilding`. Tanks (and pumps/dumps) never merge pipe graphs. |
