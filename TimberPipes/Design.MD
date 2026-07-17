# Overview

## Scope

Pipes get fluid from certain buildings: Pumps or Tanks. The only way to exit is a Water Dump or a building that consumes such fluid.

Mixed fluid is not allowed. If mixed fluid enters a **pipe** network, that network is contaminated and **simulation on that network stops** until the player flushes. Flush is a player button that **destroys all pipe fluid in that network** and marks it clean again (time for the player to fix the setup). **Internal building cells are not part of the pipe network** for contamination: a Tank can sit between two pipe networks; if one side contaminates, the other side keeps running.

This is a **sparse pipe network**, not a full-map fluid sim. Only built pipe tiles and attached building cells participate. Simulation runs on the game **Tick** (fixed `TickIntervalInSeconds`, affected by pause/speed via `Time.timeScale` → `Ticker`), not on raw Unity `Update`.

There is **no throughput limit** for now; flow is driven only by head difference, one-way ports, available volume, free space, and pump headlift. **No friction**: orthogonal and 45° edges use the same conductance (ideal pipes).

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
| **Head \(h\)** | Free-surface height of fluid (hydrostatic “pressure” expressed as meters of water column). Drives **passive** equalization. | Every cell. Pipes: \(h = z + V\). Tanks: presented from inventory fill × building height. |
| **Headlift** | **Active budget** a working pump may add: max rise it can force beyond pure gravity equalization. Rated × work factor. | **Pumps only** |

**Head ≈ pressure (in height units).** In free-surface form we never need a separate pressure field for v1: higher free surface ⇒ higher drive into lower/emptier neighbors.

### Head (passive)

- Per normal pipe cell: \(h = z + V\) with \(V \in [0, 1]\) (capacity always 1 m³).
- Fluid equalizes toward the same head across open connections (same \(k\) on ortho and diagonal).
- Tanks present a free-surface head from how full their inventory is (see Tanks). That can push **out** or accept **in** — purely passive, no powered lift.

### Headlift (pumps only)

- **Pumps** are the only devices that *provide* headlift (force fluid up against gravity up to effective MaxHeadLift).
- Valves do **not** reset or consume headlift; they only filter direction via ports.
- **Effective MaxHeadLift** is dynamic:
  - Always: one-way valve behaviour of the pump port is active (`OneWayOut`).
  - Headlift only while the building can work: mechanical power efficiency and/or beaver work.
  - Example: rated 2 m headlift at 75% power → **1.5 m** effective headlift.
  - Vanilla Water Pump: headlift only while a worker is working it.
- When the pump cannot push further into the external network, it can still fill its **internal 1 m³** tile from goods (see Pumps).

```text
Passive edge:   Q ~ k * (h_i - h_j)     // head difference = pressure drive
Pump edge:      same idea, but pump may still force internal → pipe while
                (required_rise) <= effectiveMaxHeadLift
                and internal V > 0 and downstream has free space
```

**Important clarification (Headlift across Tanks):**  
Tanks **reset / do not propagate headlift**. Headlift is local to the pump that generates it and is fully consumed when water enters the tank. A network on the opposite side of a tank receives **only** the tank’s current passive free-surface head (`h_tank`). It receives zero active headlift or “extra pressure” from the input side. Tanks are level equalizers and volume buffers, **not** pressure or headlift transmitters.

# Network Map

Each coordinate (`Vector3Int`) in the map stores:

| Field | Purpose |
|-------|---------|
| **Cell kind** | `Pipe` (normal pipe / valve segment) or `InternalBuilding` (pump, dump, tank footprint, etc.) |
| **Shape** | Per-face ports (connectivity + one-way) |
| **Volume** \(V\) | Fluid on this cell (m³); pipe cells in **\[0, 1\]** |
| **Elevation** \(z\) | Cell Z (m) |
| **Head** \(h\) | Derived (see Head vs Headlift / Tanks) |
| **Fluid type** | Clean / contaminated / empty |
| **Graph id** | Connected component among **Pipe** cells only (internals are barriers / hubs, not graph members for contamination) |

## Cell kind (Pipe vs InternalBuilding)

| Kind | Role |
|------|------|
| **Pipe** | Normal pipes, valves. Belong to a **pipe network** graph. Contamination and flush apply here. |
| **InternalBuilding** | Pump / dump / tank / manufactory cells. **Not** flooded into a pipe graph for contamination. Still exchange volume with adjacent pipe cells through ports. |

**Graph rebuild (pipe networks only):** flood-fill through `Pipe` cells that share non-`Closed` faces. **Stop at `InternalBuilding`.**  
So one tank can touch two pipe networks; they stay **two graphs**. Contaminating one does not contaminate the other. The tank keeps its own inventory/volume rules.

## Shape (ports)

Fixed array of face states (45° ring; plus vertical up/down for risers as needed).

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
| Pump internal → pipe | toward pipe `OneWayOut` |
| Dump internal ← pipe | toward pipe `OneWayIn` |
| Tank footprint cell | faces between tank cells + ports to pipes **`Open`** (neutral) |

**Rule:** each cell describes **its own** faces. Normal pipe faces are `Open` when connected; only valves / directed building ports use one-way.

### Also maintain

- Edges from non-`Closed` faces with a neighbor.
- **Pipe graphs** rebuilt on place/delete/finish (dirty flag).
- UI snapshot after each sim step.

# Buildings

**5 Water good = 1 m³** (1 good = 0.2 m³). Goods are **integers**; conversion only in **0.2 m³** steps.

## Unified building pattern

Every fluid building registers **`InternalBuilding` cell(s)** that:

- hold a small continuous buffer (usually 1 m³ per cell; tanks also present column head from inventory);
- expose **ports** to external pipes;
- **do not join** Pipe contamination graphs;
- convert goods ↔ volume **locally** where applicable.

**Structurally like a pump** means: internal cell(s) + ports + local rules. It does **not** mean every building has headlift.

| Building | Internal cell(s) | Port to external pipes | Active energy | Mass / volume source of truth |
|----------|------------------|------------------------|---------------|-------------------------------|
| **Pump** | 1× `InternalBuilding` (1 m³) | **`OneWayOut`** (built-in check valve) | **Headlift** while working | Goods → \(V\); \(V\) pushes to pipes |
| **Dump / consumer** | 1× `InternalBuilding` (1 m³) | **`OneWayIn`** (check valve in) | none | \(V\) ← pipes; \(V\) → goods |
| **Tank** | **N×** footprint cells, open among themselves | **`Open`** (neutral / bidirectional) | **none** (hydrostatic head only) | **Inventory goods** own mass; cells present head + port exchange buffer |
| **Pipe** | n/a (`Pipe` kind) | `Open` on used faces | none | \(V\) on cell |
| **Valve segment** | `Pipe` kind | `OneWayIn` / `OneWayOut` | none | \(V\) on cell |

### Neutrality vs valve

- **Valve-ness** = face is `OneWayOut` or `OneWayIn`.
- **Neutral** = face is `Open` → both directions allowed when the neighbor also permits.

```text
Pump internal  --OneWayOut-->  pipe     // never siphons back into pump
Dump internal  <--OneWayIn---  pipe     // never acts as a source into the network
Tank cell      <--Open------>  pipe     // flow either way from head difference only
```

**Neutrality is not “headlift = 0 with a valve.”** A valve with zero headlift still blocks reverse flow. A tank must use **`Open`** so fluid can refill the tank when network head is higher.

Player-facing language (optional UI later):

- Pump: *check valve out + powered lift*
- Dump: *check valve in*
- Tank: *open flange / no check valve*

## Internal building pipe

Building tiles are `InternalBuilding` network cells (usually 1 m³ buffer) with a one-way **or open** port toward external pipes. Goods conversion is local inventory ↔ that cell’s volume. They **do not** join pipe contamination graphs.

## Pumps

Vanilla Water Pumps and mod In-pipe Pumps (some mechanical, some worker-driven).

```text
[External pipe]  <-[OneWayOut from internal]-  [Internal 1 m³]
```

- **Valve (always):** one-way out of the internal tile.
- **Headlift (when working):** effective lift = `ratedMaxHeadLift * workFactor`  
  - Mechanical: `workFactor` = power efficiency (e.g. 0.75 → 75% lift).  
  - Water Pump: `workFactor` = 1 while beaver works, else 0 lift (valve still holds).
- **Goods → water (0.2 steps):** while inventory has ≥1 Water good and internal \(V \le 0.8\), convert **1 good → +0.2 m³** on the internal tile (and optionally rate-limit by production speed).  
  - Can fill the internal buffer even when **effective headlift is 0** or the network cannot accept more (up to \(V = 1\)).  
  - After \(V > 0.8\), no further good conversion until outflow frees enough space for another 0.2 m³.
- **Outflow to network:** separate from conversion. Continuous sim volume can leave whenever ports + **headlift push** allow (see Simulation). Discrete goods do **not** block outflow.

## Water Dump and other consumers

```text
[Internal 1 m³]  -[OneWayIn]-  [External pipe]
```

- **Water → goods:** while \(V \ge 0.2\) and inventory can take 1 good, convert **−0.2 m³ → +1 good**.
- If conversion is blocked, internal holds water up to **1 m³**.

## Pipes

| Variant | Segments |
|---------|----------|
| Straight | 0° and 180° |
| Elbow (2 / 3 outlets) | 45° and/or 135° |
| Junctions (3 outlets) | 0°, 90°, 180° |
| Junctions (4 outlets) | 0°, 90°, 180°, **270°** |

`Pipe` kind, capacity 1 m³, usually all used faces `Open`.

## Valve

- **Straight only** (horizontal and vertical).
- Shape: `OneWayOut` / `OneWayIn` on the two ends.
- Does not reset headlift (headlift is not a network “token”; valves only filter direction).

## Storage (Tanks) — multi-tile

**Same InternalBuilding pattern as pump/dump**, but:

- ports to pipes are **`Open`** (neutral — no check valve);
- **no headlift** (no powered push above free surface);
- **inventory owns mass**; cells present **hydrostatic head** from fill × height.

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

### Volume / head sync (predictable)

Do **not** cap tank storage at 9 m³. Use inventory for mass; use cells for topology + presented head:

```text
fillFraction = storedGoods / capacityGoods          // e.g. 600/1200 = 0.5
columnHeight = fillFraction * buildingHeightTiles   // e.g. 0.5 * 3 = 1.5 m
```

For each tank footprint cell at base elevation \(z_{base}\):

```text
// Present a uniform free surface to adjacent pipes:
h_tank = z_base + columnHeight
// Optional per-cell V used only for local display / port exchange buffer:
V_cell = saturate(columnHeight - (z_cell - z_base))  // classic column in multi-height tanks
// For single-layer footprint with height in the building spec only:
V_cell = fillFraction   // 0..1 visual fill; head uses columnHeight above
```

**Port exchange:** pipes equalize against tank head / buffer using **passive** \(Q \sim k (h_i - h_j)\) only (no headlift). Goods count updates from net volume in/out at ports using 0.2 m³ ↔ 1 good when enough delta has accumulated. Prefer: tank inventory is source of truth; port flow changes goods in 0.2 steps when enough accumulated. Cell \(V\) may act as a small continuous exchange/display buffer — do not invent a second full storage system on the cells.

**Why multi-cell open footprint (not one super-node only):** pipe attachments on different sides stay natural; contamination still **stops at the tank**; two networks do not merge through the tank graph id.

**Why not “9×1 m³ = all storage”:** large good capacities (hundreds of m³) would not fit; inventory must own storage mass.

### Critical Clarifications (from design discussion)

#### 1. Two networks connected to the same tank remain two separate graphs

Even though water can flow between Network A and Network B through the tank, the graphs **do not merge**.  

- Flood-fill for Pipe graphs stops at `InternalBuilding` cells.  
- The tank acts as a **volume bridge** but **not a graph bridge**.  
- Contamination, simulation stopping, and flush on one side have **no effect** on the other side.  
- This is structural (always true), not conditional on mixed fluid.

Mental model: the tank is a shared swimming pool. Different plumbing systems can pour into / take from the same pool. The water level is shared, but the left and right plumbing systems remain independent networks.

#### 2. Headlift does not propagate through tanks

Tanks **reset headlift completely**.

- Headlift is local to the pump and is consumed when water is pushed into the tank.  
- Once water enters the tank it becomes part of the passive free-surface inventory.  
- The opposite network only ever sees the current passive `h_tank`. It receives **zero** active headlift.

**Example**:  
Tank base = 5 m, height = 3 m (max `h_tank = 8 m`).  
Pump A can push to effective head 8 m and fills the tank to 100%.  
Network B still cannot climb above the current `h_tank` without its **own** pumps + headlift. Any “down then up” section on Network B requires local headlift on B.

Tanks are excellent level equalizers and volume buffers, but they are deliberately **not** pressure or headlift transmitters.

#### 3. Filling limits — a pump can only fill a tank up to the head it can achieve

A pump/network can only raise the tank’s free surface to the maximum head it can push.

**Example**:  
Tank base = 5 m, height = 3 m (max `h_tank = 8 m`).  
Pump A can push to effective head = 6 m.  
Maximum reachable fillFraction ≈ `(6 − 5) / 3 ≈ 33%`.

Once `h_tank` approaches what the pump can achieve, inflow slows and eventually stops. Remaining capacity above that level is unreachable for this pump alone.

#### 4. Gravity-fed downstream pattern

If Pump A keeps the tank near its maximum height, and Network B has:

- no significant uphill sections, and  
- all destinations at or below the current `h_tank`,

then Network B can run **pump-free** as a pure gravity-fed system while the tank stays high.  

**Limitation**: As soon as consumption on Network B (or any side) lowers the tank level, the passive driving head decreases. Sustained high-level gravity feed requires continuous refilling from the input side(s).

#### 5. Implementation notes for tanks

- Building component owns inventory and computes `h_tank` every tick (or on change).  
- Port flow is handled in the normal network flow step (passive only).  
- Accumulate **net** delta across **all** tank ports; quantize to ±1 good (0.2 m³) only when the accumulated threshold is crossed. This prevents floating-point drift.  
- Cell `V` values are secondary (visual / small exchange buffer). Inventory is the single source of truth.  
- Large capacity relative to visual height produces a slow visible fill rate (intentional and realistic).

# Simulation

Simple, predictable: conserved volume on pipe cells, head equalization, pump forced lift. Ideal conductance (no diagonal penalty). No \(Q_{max}\).

## Driving metrics

| Metric | Role |
|--------|------|
| **Volume \(V\)** | Pipe cell fill 0..1 |
| **Head \(h = z + V\)** (pipes) | Passive equalization target (pressure height) |
| **Tank presented head** | From inventory fill × height |
| **Conductance \(k\)** | Same for all allowed edges (ortho + 45°) |
| **Effective MaxHeadLift** | Pump-only push against gravity |
| **\(\Delta t\)** | Tick interval (1–2 substeps optional) |

## Per-tick pipeline

```text
1. Topology (if dirty)
   - Rebuild Pipe graphs: flood-fill Pipe cells only; stop at InternalBuilding.
   - Link edges Pipe↔Pipe and Pipe↔Internal for flow, but graph id only on Pipe.

2. Contamination
   - Detect mix on a Pipe graph → mark that graph contaminated; skip its flow.
   - InternalBuilding cells are not in the graph; other graphs keep running.
   - Buildings keep inventory logic (as if that pipe side were unplugged if ports only touch a stopped graph).

3. Building conversion (InternalBuilding)
   - Pump: if can work and V <= 0.8 and has goods → +0.2 m³ / good (rate limits apply).
   - Dump/consumer: if V >= 0.2 and can accept good → −0.2 m³ / good.
   - Independent of whether external graph is full; only needs free/occupied internal volume.
   - Tank: inventory is authoritative; optionally quantize accumulated port exchange into goods.

4. Network flow
   - Skip contaminated Pipe graphs.
   - Passive edges: Q = k * (hi - hj), port filters, clamp by V / free space.
   - Pump discharge edges: if effectiveHeadLift > 0, allow push from internal toward pipe
     up to lift budget (see below), not only passive head equalization.
   - Tank ports: passive exchange only, using tank presented head / inventory rules.

5. Publish UI snapshot.
```

## Pump push vs goods conversion (vertical / remaining headlift)

These are **two different steps**. **Conversion space is only on the pump’s internal 1 m³ tile.** The vertical network never needs a reserved 0.2 m³ “goods slot.”

| Step | Unit | When |
|------|------|------|
| **A.** Goods → internal \(V\) | **0.2 m³ per integer good** | \(V \le 0.8\), working, has goods |
| **B.** Internal → pipes | **Continuous \(V\)** (float) | Ports allow + **effective headlift** covers the rise + downstream free space |

Why the 0.8 / 0.2 thresholds exist: **quantization guards** so we never hold a fractional good.

- Convert **in** only when there is room for a full +0.2 m³.
- Convert **out** (dump) only when there is a full −0.2 m³ of volume.
- Continuous pipe flow can still move 0.01 m³, 0.17 m³, etc. That is intentional.

Passive \(h = z + V\) alone **cannot** climb: empty pipe at \(z=2\) has \(h=2\), full internal at \(z=0\) has \(h=1\). The pump must **add lift**:

```text
// Conceptual: treat pump outlet as able to force flow upward while
// (targetSurface - sourceSurface) <= effectiveMaxHeadLift
// and internal V > 0 and downstream has free space.
```

So: remaining headlift lets the pump **empty continuous volume** into a riser; the 0.2 m³ rule only gates **taking the next good from inventory**, not pushing already-converted water. After water leaves, \(V\) drops; when \(V \le 0.8\) again, another good can convert.

| Situation | Outflow | Conversion |
|-----------|---------|------------|
| Headlift available, downstream free space | Continuous \(V\) climbs | Space on internal frees → more goods convert |
| Headlift 0 (or exhausted vs rise) | Stops | Continues until internal \(V > 0.8\) (buffer full) |
| Network completely full (no free space) | Stops even with headlift | Stops once \(V > 0.8\). Headlift does not create capacity; it only overcomes **elevation**. |

## Equilibrium rules

1. Conservation on pipes (except flush wipe).
2. Equal head across open pipe connections (ideal \(k\)).
3. Horizontal equal pipes → share volume equally.
4. Vertical: gravity via head; pumps force lift up to effective MaxHeadLift.
5. Internal buffers hold up to 1 m³ independent of lift.
6. Contamination is **per Pipe graph**, stops at InternalBuilding.
7. Tanks exchange only via passive head (no headlift); ports `Open`.

## Worked examples

### Horizontal 5 pipes

Inject 1 m³ at one end → settles to **0.2** each.

### Pump + vertical riser (conversion space)

Setup: pump internal at \(z=0\), empty riser pipe at \(z=1\), rated headlift 2 m, goods available.

| Phase | Conversion (A) | Outflow (B) | Why |
|-------|----------------|-------------|-----|
| Start, \(V=0\) | \(V: 0 → 0.2\) (1 good) | may push upward under headlift | Goods only care about internal free space |
| Working with lift | keep converting while \(V \le 0.8\) | riser fills gradually | Outflow frees internal space; conversion refills in 0.2 chunks |
| No lift (worker/power off) | stops once \(V > 0.8\) (e.g. \(V=1\)) | **0** | Passive \(h_\text{int} \le 1\); empty pipe at \(z=1\) has \(h=1\); cannot climb. Valve still blocks reverse. |
| With lift again | after some \(V\) leaves, \(V \le 0.8\) → convert | continuous climb | **Headlift empties continuous \(V\)** — that is what creates conversion space |

Discrete goods **do not** need to “fit” into the riser as integer packets; only continuous volume moves on edges.

### Contaminated side of a tank

- Left pipe graph mixes → contaminated, frozen, flush wipes **left pipe volumes only**.
- Tank inventory unchanged; right pipe graph still simulates.
- Tank stays neutral (`Open`); isolation is from **graph membership** (InternalBuilding barrier), not from a valve.

### Flush

- Player presses Flush on a contaminated network after fixing inputs.
- **Instant:** all `Pipe` cell volumes in that graph → 0, fluid type clean, resume sim.
- No beaver work cost (button is a deliberate “go” after fixes).
- `InternalBuilding` volumes/inventories **not** destroyed by pipe flush (unless a future explicit “empty building” action).

### Tank filling limit example (new)

Tank base = 5 m, height = 3 m (max `h_tank = 8 m`).  
Pump on Network A can push to effective head = 6 m.  

→ Network A can only fill the tank up to approximately 33% (`columnHeight ≈ 1 m`).  
Higher fill requires a stronger pump, higher pump elevation, or another input that can achieve higher head.

### Gravity-fed Network B example (new)

Tank kept near `h_tank = 8 m` by strong Pump A.  
Network B has no uphill sections and all destinations ≤ 8 m.  

→ Network B can operate **pump-free** while the tank stays high. Water moves by passive head equalization.  
If consumption lowers the tank, driving head decreases and some destinations may stop receiving water until the tank is refilled.

# Persistence

**Save only what cannot be rebuilt:**

| Save | Rebuild on load |
|------|-----------------|
| Per-cell **volume** (and fluid type if non-empty/contaminated) for cells that need it | Shape, kind, ports from buildings/pipes placed in the world |
| Contaminated flag can be derived from fluid type on any cell in the graph | **Graph ids** — always rebuild flood-fill |
| Building inventories | Already saved by vanilla entity persistence |

Topology/shape/kind come from finished buildings → **do not save graph structure**. On load: register all cells from buildings → rebuild graphs → apply saved volumes onto coordinates.

# Implementation shape

| Piece | Role |
|-------|------|
| `PipeNetworkMap` | Coord → cell (kind, shape, V, fluid type) |
| `PipeGraphRegistry` | Flood-fill Pipe-only graphs; dirty flag |
| `PipeSimulator` | `ITickableSingleton` |
| Building components | Register InternalBuilding cells + ports; conversion; dynamic headlift (pumps); presented head (tanks) |
| Specs | rated MaxHeadLift (pumps), conversion rate, tank height/capacity |

Main-thread singleton first; parallelize only if needed.

## Tunables

| Tunable | Intent |
|---------|--------|
| Equalize \(k\) | Settle short nets in ~1–3 ticks |
| Substeps | 1–2 per tick |
| Rated MaxHeadLift | Per **pump** building only |
| Conversion rate | Goods/tick while working |

# Decisions (resolved)

| Topic | Decision |
|-------|----------|
| Multi-tile tank | Footprint = N `InternalBuilding` cells, all-open among themselves; **inventory owns mass**; head from fill × height; contamination **stops** at tank (does not merge pipe graphs). |
| Tank neutrality | **`Open` ports** both ways + hydrostatic presented head. **Not** zero headlift with a valve. Tanks have **no MaxHeadLift**. |
| Head vs headlift | Head = passive pressure height everywhere. Headlift = pump-only active rise budget. |
| Headlift across tanks | Tanks **reset headlift**. No active lift is transmitted to the opposite network. Only passive `h_tank` is visible. |
| Conversion space | Only internal \(V \le 0.8\) gates goods→volume. Headlift creates space by continuous outflow into the network; no reserved goods slots on pipes. |
| Flush | Instant player button: wipe **pipe** volumes in that graph + clear contamination. Internals/tanks not wiped. Gives time to fix before resuming. |
| Pump power | One-way always; headlift × power efficiency or worker active. |
| Diagonal flow | Same \(k\) as ortho; no friction. |
| Save | Volumes (+ fluid type); rebuild topology/graphs on load. |
| Goods conversion | Integer goods only; ±0.2 m³ when \(V\le0.8\) (in) or \(V\ge0.2\) (out). Outflow/lift is continuous and separate. |
| Tank filling limit | A pump can only raise a tank’s free surface to the head level its headlift can achieve. |
| Gravity-fed networks | A high tank can gravity-feed a downstream network that stays at or below current `h_tank` and has no uphill sections. |
