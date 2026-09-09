# TimberPipes in-game layouts

Build these on a **levee / flat deck** next to river water. Side views: **Z up, X right**. All pipes on the same Y unless noted.

Dev Mode: pipe panel **TransportPipe** debug → **Fill** to seed a tile. Watch **Headlift**, tank %, and that far-side tiles stay dry until the crest is actually wet.

Power every **Pipe Pump** (40 hp). **Riser Pipe Pump** always lifts (rotation does not reverse it). Water Pump needs workers and water under the intake. Point a **One-way Valve** at it and enable **Extract** (pick Water) to fill pipes. Point OpenOut at a Discharge and enable **Fill** to empty pipes. Pause a One-way Valve to seal both ways (no IO). Pause a Pipe Pump: still one-way, **no headlift**.

```
Legend
  ~~  world water          WP  Water Pump (inventory; valve Extract)
  PP> Pipe Pump outflow →  <PP outflow ←     (rotate 180 to reverse)
  V>  One-way Valve outflow →      <V  outflow ←
  ==  pipe                 ||  riser                         xx  floor cross
  ++  vertical cross
  T2  Small Tank (H = 2)   D   Discharge (inventory; valve Fill)
  *   tile to Fill / watch
```

Pipe Pump **L = 2**: it may **enter** a tile 2 m above the pump (`P_next >= 0`). A 3 m rise stays dry.

---

## 1. Water Pump fills the deck, not the riser

Pass 1 only after valve packets. Full deck vs empty `z+1` is equilibrium. OpenIn of `<V` faces the Water Pump; enable **Extract**, pick Water.

```
  2 |
  1 |                 ||*
  0 | ~~ WP <V == == ==
```

**Expect:** deck pipes fill to 1 m³ (0.2 m³/tick from the valve). Riser stays empty. Leftover stays in the Water Pump. Headlift on pipes is **0**.

**Off:** leave Extract disabled. Pipes stay empty — Water Pump is not a well column.

---

## 2. Pipe Pump climbs 2 m, stalls at 3 m

Same as (1) plus a powered Pipe Pump on the deck. Outflow toward the riser. Keep the Water Pump valve Extract enabled.

```
  3 |                   ||*     stays empty
  2 |                ||
  1 |             ||
  0 | ~~ WP <V == PP> ==
```

**Expect:** `z = 0,1,2` fill (slug, 0.2 m³/tick). `z = 3` stays empty. Headlift on the pump tile **2.00**, drops **1** per rise, **0** at `z = 2`. Unpower the pump: no more climb; water already up there **falls** back down.

---

## 3. U-bend — do not skip the crest

Water Pump **cannot** wet the far side. Pipe Pump **must** fill the crest before the far ground.

```
  2 |          ==*
  1 |       ||    ||
  0 | ~~ WP <V PP> ==    ==* ==
```

**A. Unpower PP (or delete it):** near ground fills; crest and far ground stay empty.

**B. Power PP:** near ground → near riser → **crest** → then far riser → far ground. Far ground Headlift stays **0** until the crest has water. Never a wet far side with a dry crest.

---

## 4. Two risers on a full main (communicating vessels)

Pass 1 cannot squeeze through a full main. Pass 2 should equalize the two tops.

```
  1 |  ||*              ||*
  0 |  ++ == == == == ++
         \___________/
            full main
```

**Setup:** run a Water Pump through an enabled valve Extract into the main until the **whole** bottom row is full. Dev **Fill** the left riser (or let a Pipe Pump fill it, then pause/remove the pump).

**Expect:** water appears in the right riser. Surfaces match. If the right riser is a hole at `z = 0` instead (empty branch off a packed column), the high full lid still drains into that hole.

---

## 5. One-way Valve

Horizontal valve: **rotate 180** to reverse (do not flip).

```
  1 |  ||*              ||*
  0 |  ++ == V> == == ++
```

**A. V> toward the right:** Fill left riser → right riser fills. Fill right riser → left stays dry.

**B. Reverse the valve:** opposite.

Pause the valve: both sides isolated (no in, no out, no IO).

---

## 6. One-way Valve does not trap a high top that can still drain

R2 is higher than R1 but can still reach a **lower** R3 through the valve outflow.

```
  1 |           || R2
  0 |  || R1 == V> ++ == ||* R3   (R3 is a hole / lower branch)
```

**Expect:** R1 may fill R2. R2 does **not** flow back to R1. R2 **does** pour into R3.

---

## 7. Small Tank, bottom pipe only, Water Pump

Tank is two 1 m slices. Bottom-only gravity fills the **lower meter** (half of 30 water). Enable the Water Pump valve **Extract**.

```
  1 |                [T2]
  0 | ~~ WP <V == == == [T2]
```

**Expect:** tank inventory stops at **~15 / 30**. Upper slice empty. A pipe sitting on the **upper** tank tile is not required for this fail case — that is (8)/(9).

---

## 8. Same tank, Pipe Pump fills the top slice

```
  1 |                [T2]     ← fills after PP runs
  0 | ~~ WP <V == PP> == [T2]
```

**Expect:** tank goes **30 / 30**. Takes a while (0.2 m³/tick into the top 3 m³). Headlift on pipes **2.00**; tank panel is not Headlift.

Unpower PP after it is full: tank **holds**. Drain a hole at deck level and the tank should feed it (packed column / pass 2).

---

## 9. Upper-tile pipe fills the tank without extra lift into the slice

Connect a pipe to the **top** occupied cell of the Small Tank. Pipe Pump still needed to **climb** that pipe.

```
  1 |             == [T2]
  0 | ~~ WP <V == PP> == [T2]
              ||
```

**Expect:** full tank, same as (8). Removing the upper pipe and keeping PP is (8). Removing PP and keeping only the upper pipe: Water Pump **cannot** climb to `z = 1`, so the upper connection stays dry and the tank stays at half from the bottom port only.

---

## 10. Full tank feeds a riser (pass 2 through slices)

Seed the tank with **Fill** on a connected pipe until the tank is full (or dump 30 water in by inventory debug). Then add a riser on the **bottom** pipe. No Pipe Pump.

```
  1 |  ||*            [T2]
  0 |  == =========== [T2]
```

**Expect:** riser fills. A 2 m packed tank is a 2 m column; communicating vessels may wet `z = 1`.

---

## 11. Tank breaks graphs

Two separate pipe networks that only touch the **same** Small Tank.

```
  0 | ~~ WP <V == [T2] == V> D
                 left   right
```

**Expect:** left and right are **different** graphs. Contaminating the left (see 13) does **not** freeze the right. Flush left does **not** empty the tank or the right pipes. Tank inventory is shared.

---

## 12. Discharge slurps the neighbor only

OpenOut of `V>` faces the Discharge; enable **Fill**. Left valve Extract from the Water Pump restocks the line.

```
  0 | ~~ WP <V == == V> D
```

**Expect:** each tick the **valve next to D** loses 0.2 m³ (1 good) if `V ≥ 0.2` and Discharge has space. Pipes further away are not teleported into D. Water Pump restocks through the Extract valve.

---

## 13. Mix contaminates that graph only

```
  0 | ~~ WP(water) == == xx == ==  (other good / second WP with Badwater)
```

Practical: Fill one pipe with **Water**, another on the **same** connected network with a different liquid (Badwater Pump, or a tank already holding Badwater).

**Expect:** graph stamps contaminated, flow **stops**, Headlift **0**. Tank inventories **unchanged**. **Flush** clears pipe volumes only; then it can run again.

---

## 14. Pause and unpowered Pipe Pump

```
  0 | ~~ WP <V == PP> == ==
```

**A. Pause PP:** still one-way; **no** climb / no pass 3. Gravity can still cross the pump in the open direction.

**B. Unpower PP (not paused):** same as A: one-way, **no** climb. Deck may still fill from the valve outlet if outflow allows.

---

## Quick pass map

| Layout | Pass that matters |
|--------|-------------------|
| 1, 7, 12 | valve IO + pass 1 |
| 4, 5, 6, 10 | 2 (full core → tops) |
| 2, 3B, 8, 9 | 3 (pipe pump slug) |
| 3A, 14 | negative: no climb / no skip |
| 11, 13 | graphs / contamination |
