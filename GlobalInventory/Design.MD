Note: this file will be deleted later once we done implementing.

# Global Inventory

API mod. Timberborn goods (`GoodSpec`) are always *somewhere*: a building inventory, a warehouse, a beaver's hands. This mod adds a second, colony-wide inventory for values that are not in the world — money, reputation, faction points, and similar.

The two systems never mix. A coin that exists as a `GoodSpec` can also exist as a `GlobalGoodSpec`; producing the local coin does not change the global count until a recipe (or other API call) says so.

This mod ships **no player buildings**. Other mods declare specs and call the API.

---

## Goals / non-goals

**Goals**

- One save-wide inventory of named global goods, with min/max capacity.
- Spec-driven definition, copied from a `GoodSpec` when the author already has a local good.
- Tiny runtime API: get a handle, add/remove/clamp, change capacity, hide from UI.
- Top-bar group that appears only when something is actually showing.
- Optional manufactory hook so a workshop can spend/produce global goods on a recipe cycle **without Harmony**.

**Non-goals**

- Hauling, warehouses, districts, carrying weight, stockpile visualization.
- Auto-converting local stock into global stock (authors do that with a recipe or their own code).
- Per-district or per-building global inventories.
- A player-facing settings panel in v1.
- Registering global goods as `GoodSpec` (that would make them haulable).

---

## Specs: `GlobalGoodSpec`

Blueprint component, same pattern as `GoodSpec` / `RecipeSpec`. Other mods drop JSON under their own `Root/`.

```jsonc
{
  "GlobalGoodSpec": {
    "Id": "Coin",
    "CopyFromGoodSpec": true,          // optional; default false
    "CopiedGoodId": "Coin",            // optional; default = Id
    "DisplayNameLocKey": "LV.X.Coin",  // required unless CopyFromGoodSpec
    "PluralDisplayNameLocKey": "LV.X.Coins",
    "Icon": "Goods/Coin/CoinIcon",
    "Order": 10,                       // default 0; sort alphabetically on ties
    "MinCapacity": 0,                  // default 0; may be negative
    "MaxCapacity": null,               // omit / null = unlimited, 0 = negative-only good, like debts
    "InitialAmount": 0,                // default 0
    "HideOnZero": false                // default false
  }
}
```

### Fields

| Field | Default | Notes |
|---|---|---|
| `Id` | required | Unique across all mods. This is the API key. |
| `CopyFromGoodSpec` | `false` | If true, fill display/icon from a `GoodSpec`. |
| `CopiedGoodId` | `Id` | Which `GoodSpec` to copy. Use when the global id differs (`GlobalCoin` copying `Coin`). |
| `DisplayNameLocKey` / `PluralDisplayNameLocKey` | — | Required when not copying. Copied values win only for fields the author left unset? **No:** if `CopyFromGoodSpec`, copied display/icon always win; loc/icon on the global spec are ignored. Keeps "just set the flag" simple. |
| `Icon` | — | `AssetRef<Sprite>`. Required when not copying. Also expose flipped/small UI sprites if the copied good has them; otherwise one icon is enough. |
| `Order` | `0` | UI sort among global goods. Tie-break: localized display name. |
| `MinCapacity` | `0` | Inclusive. May be negative (debt). |
| `MaxCapacity` | `null` | Inclusive. `null` = no max. Must be `>= MinCapacity` when set. |
| `InitialAmount` | `0` | Applied only when the save has **no** stored entry for this id (new game or new good on an old save). Must lie in `[MinCapacity, MaxCapacity]`. |
| `HideOnZero` | `false` | After the good has been revealed, hide it again whenever `Amount` is `0` (not when it is merely at min, if min ≠ 0). |

**Copied from `GoodSpec`:** display name, plural name, icon / flipped icon / small icon.

**Not copied:** `Weight`, `GoodGroupId`, `GoodOrder`, `GoodType`, stockpile/container/carrying/consumption fields. Those describe world goods.

`GoodOrder` on the local good is ignored; global UI uses `GlobalGoodSpec.Order`.

### `GlobalGoodSpecService`

`[BindSingleton]`, `ILoadableSingleton`. Specs only — no amounts.

On `Load`:

1. Read all `GlobalGoodSpec`.
2. If `CopyFromGoodSpec`, resolve `IGoodService.GetGood(CopiedGoodId)` and bake display/icon onto a resolved record. Missing good → throw with blueprint name + id.
3. Validate unique `Id`, `MinCapacity <= MaxCapacity`, `InitialAmount` in range, icon/name present after copy.
4. Freeze lookup dictionaries.

API:

```cs
GlobalGoodSpec Get(string id);          // throw if missing
bool TryGet(string id, out GlobalGoodSpec spec);
IReadOnlyList<GlobalGoodSpec> All { get; }  // stable order: Order, then localized name
```

Use a resolved/baked spec at runtime so callers never branch on `CopyFromGoodSpec`.

---

## Inventory

One colony-wide inventory. Not a `Timberborn.InventorySystem.Inventory`.

### `GlobalInventoryService`

`[BindSingleton]`, `ILoadableSingleton` + `ISaveableSingleton`.

At load, after specs: create one **handle per spec**, apply save data or `InitialAmount`, then freeze the handle map. Handles are singletons for the game session.

```cs
GlobalGoodHandle Get(string id);        // throw if unknown spec
bool TryGet(string id, out GlobalGoodHandle handle);
IReadOnlyList<GlobalGoodHandle> All { get; }
IEnumerable<GlobalGoodHandle> Visible { get; }  // current UI set
```

### `GlobalGoodHandle`

Not a Unity component. Created by `GlobalInventory`.

```cs
GlobalGoodSpec Spec { get; }
float Amount { get; }
float MinCapacity { get; }
float? MaxCapacity { get; }             // null = unlimited
bool IsVisible { get; }                 // UI revealed and not hidden
float? FillRate { get; }                // Amount mapped into [Min, Max]; null if unlimited or MaxCapacity == 0

void Add(float delta);                  // throw if result outside [Min, Max]
void Remove(float delta);               // Add(-delta)
float AddSafe(float delta);             // clamp; return actual applied delta
float RemoveSafe(float delta);
void Set(float amount);                 // throw if outside range
float SetSafe(float amount);            // clamp; return resulting Amount

void SetCapacity(float min, float? max);     // throw if Amount outside new range
void SetCapacitySafe(float min, float? max); // clamp Amount into the new range

void Hide();                            // drop from UI until the next revealing change
```

Rules:

- `delta == 0` is a no-op: no event, no reveal.
- Throw versions use a clear exception (`InvalidOperationException`) with id, attempted amount, and current range.
- Safe versions clamp to `[MinCapacity, MaxCapacity]` (or only min if max is null).
- Runtime capacity overrides persist. Spec min/max are the defaults for a **new** save entry only.
- `SetCapacity` does not change `Amount` unless using the Safe variant.
- Hide() is no op if amount is not zero.

### Reveal / hide

A good is **revealed** after the first non-zero `Add`/`Remove`/`Set` (including going negative). `InitialAmount != 0` also reveals on a new entry, so a starting treasury of `100` is visible immediately.

Once revealed, it **stays visible across saves**, except:

- `HideOnZero` and `Amount == 0` → un-reveal (will show again on the next non-zero change).
- `Hide()` → un-reveal immediately, even if amount ≠ 0. Next non-zero change reveals again.

`IsVisible` is `revealed && !(HideOnZero && Amount == 0)` after applying `Hide()`.

The top-bar **group** is shown iff `Visible` is non-empty. Hide the group again when the last visible good disappears.

### Events (`EventBus`)

```cs
readonly record struct GlobalGoodAmountChangedEvent(
    GlobalGoodHandle Handle, float Previous, float Current, float AppliedDelta);

readonly record struct GlobalGoodCapacityChangedEvent(
    GlobalGoodHandle Handle, float Min, float? Max);

readonly record struct GlobalGoodVisibilityChangedEvent(
    GlobalGoodHandle Handle, bool Visible);

readonly record struct GlobalGoodClickedEvent(GlobalGoodHandle Handle);
```

Post after the handle has the new values. Authors subscribe with `[OnEvent]`. Click has no default UI beyond this event.

### Persistence

`ISaveableSingleton` key e.g. `GlobalInventory`.

Per known id, store: `Amount`, runtime `MinCapacity` / `MaxCapacity` (including “unlimited”), `Revealed`, `HiddenByApi`.

- Unknown ids in the save (uninstalled mod): ignore, do not throw, do not keep rewriting them unless it is cheap to round-trip. Prefer drop-on-load.
- New spec on an old save: `InitialAmount`, not revealed unless that amount ≠ 0.
- On load, if stored amount is outside stored/spec range (author tightened max): **clamp**, do not throw. Saves must load.

Amounts are `float`. UI may show integers; storage stays float.

---

## UI

Do **not** inject a `GoodGroupSpec` or fake `GoodSpec`. Vanilla counters go through `IGoodService` + `ContextualResourceCountingService` (district stock, fill from warehouses). Global goods are not stock.

### Placement

Clone the look of an extendable top-bar good group. Insert it **after the last vanilla group** on the same top-bar root.

`TopBarPanel` is internal but publicized. A `[BindSingleton]` service depends on `TopBarPanel` so Bindito constructs it afterward; `PostLoad` appends the borrowed extendable group onto `_root`. Do not Harmony-patch and do not `UILayout.AddTopBar` a second bar.

Configurable Top Bar reorders vanilla groups; appending to `_root` still puts global goods last. That is fine. Do not register the group with CTB.

District context: **ignore**. Counts are colony-wide; do not dim or split by selected district.

### When it appears

- Game start with nothing revealed: no extra group.
- First revealing change: show the group and that good.
- Last visible good hidden: hide the group.

Row order: `Order` ascending, then localized display name (`ILoc`). Goods that are not visible are omitted, not greyed.

### Row contents

Each visible good shows:

- **Icon**
- **Name** (localized display name)
- **Amount**
- **Max capacity**, only when `MaxCapacity` is not `null` **and not `0`**. Format like vanilla stock (`10 / 100`). `null` is unlimited — omit the max. `0` is still a real cap for clamping/stalling (debt-only goods); do not render `/ 0`.

Amount/max numbers: compact. Whole numbers as integers (`100`); otherwise a short decimal (`1.5`). Negative sign allowed.

Fill gauge: same rule as the max label — only when `MaxCapacity` is not `null` and not `0`. Map `Amount` from `MinCapacity..MaxCapacity`. If min < 0, 0 is not necessarily empty.

Click a row → `EventBus.Post(new GlobalGoodClickedEvent(handle))`. No built-in panel.

---

## Convenient component: global recipes

For authors who already have a `Manufactory` and just want “this recipe also pays/costs global goods”.

### `GlobalInventoryManufactorySpec`

Empty marker on the **building** blueprint. `[AddTemplateModule2(typeof(GlobalInventoryManufactorySpec))]` adds `GlobalInventoryManufactory`.

```jsonc
{
  "GlobalInventoryManufactorySpec": {}
}
```

Without this marker, `GlobalInventoryRecipeSpec` on a recipe is ignored because nothing handles it.

### `GlobalInventoryRecipeSpec`

Sibling spec on the **recipe** blueprint (same as `ModdableRecipeLockSpec` on `RecipeSpec`). Look up with `recipe.GetSpec<GlobalInventoryRecipeSpec>()`.

```jsonc
{
  "RecipeSpec": { "Id": "MintCoin", /* local ingredients/products as usual */ },
  "GlobalInventoryRecipeSpec": {
    "GlobalIngredients": [ { "Id": "Gold", "Amount": 1 } ],
    "GlobalProducts":    [ { "Id": "Coin", "Amount": 2 } ],
    "AllowOverflow": false
  }
}
```

```cs
readonly record struct GlobalGoodAmountSpec(string Id, float Amount);

record GlobalInventoryRecipeSpec : ComponentSpec
{
    ImmutableArray<GlobalGoodAmountSpec> GlobalIngredients { get; init; } // default []
    ImmutableArray<GlobalGoodAmountSpec> GlobalProducts { get; init; }
    bool AllowOverflow { get; init; } // default false
}
```

Amounts must be **> 0**. Spending is ingredients, not negative products.

`AllowOverflow`: if a product would pass `MaxCapacity`,

- `false` (default): stall, same as a full output inventory.
- `true`: finish the cycle and `AddSafe` (extra is discarded). Never partial-credit a cycle.

No `AllowUnderflow`. Missing ingredients stall; they are never partially consumed.

Validate on load: every id exists in `GlobalGoodSpecService`.

### `GlobalInventoryManufactory`

On the building, next to `Manufactory`. Inject a small service for inventory + recipe lookup (AGENTS.md: do not dump many deps on the component).

Implements **`IManufactoryLimiter`**. Vanilla `Manufactory.Awake` collects limiters with `GetComponents`. `IsReadyToProduce` requires `ProductionEfficiency() > 0`, so returning `0` stalls **without Harmony**.

Vanilla limiter API:

```cs
float ProductionEfficiency();
float MaxProductionProgressChange(float expectedProductionProgressChange);
```

Return `0` efficiency (and `0` max progress) when the current recipe has a `GlobalInventoryRecipeSpec` and:

- global ingredients for this cycle are not yet taken **and** any ingredient `Amount` is short, or
- `!AllowOverflow` and any product cannot fit in remaining capacity (`Amount + product > Max`).

Unlimited max → products never block.

### Cycle timing (mirrors vanilla)

Vanilla takes **local** ingredients on the first progress tick (`_ingredientsConsumed`), and gives **local** products in `FinishProduction`.

Global side:

| When | What |
|---|---|
| First progress of a cycle (same moment local ingredients are taken) | `Remove` each `GlobalIngredients` (throwing version: limiter already guaranteed they fit). Set a per-cycle flag so we do not charge twice. |
| `ProductionFinished` | `Add` / `AddSafe` each `GlobalProducts`. Clear the flag. |
| `ResetProductionProgress` / recipe change after ingredients were taken | **Do not refund.** Vanilla does not refund local ingredients either. |

How to detect “first progress”: on `ProductionProgressed`, if `Manufactory` has `_ingredientsConsumed` and our flag is false, take global ingredients.

**Why not only `ProductionFinished`:** two buildings can both be mid-cycle with only enough global gold for one mint. Charging at start (or reserving at start) prevents double-spend. Implement as an immediate `Remove` at cycle start, not a separate reservation object, unless we later need refunds.

Recipes with **only** global products (empty local `Products`) are valid. Vanilla `HasUnreservedCapacityForCurrentProducts` is vacuously true; the limiter enforces global capacity.

### Recipe UI

Players see global in/out on the building preview via `IEntityDescriber` on `GlobalInventoryManufactory` (`DescribedAmountFactory` / `ProductionItemFactory`). Vanilla `ManufactoryDescriber` recipe dropdown stays local-goods-only.

### Workplace “no resources” bubble

`ManufactoryInputChecker` only looks at district stock for **local** goods. A building stalled on global gold will look like a powered, stocked shop that is not progressing. Acceptable for v1. Follow-up: a status that is true when the limiter is blocking on ingredients.

---

## Author cookbook

**Global-only currency**

```jsonc
{ "GlobalGoodSpec": { "Id": "Money", "DisplayNameLocKey": "LV.X.Money", "Icon": "...", "MinCapacity": 0 } }
```

```cs
inventory.Get("Money").Add(10); // inject GlobalInventoryService
```

**Local coin that becomes global at a bank**

1. Normal `GoodSpec` `Coin` (haulable).
2. `GlobalGoodSpec` `{ "Id": "Coin", "CopyFromGoodSpec": true }` — same icon/name, separate counter.
3. Bank building: `Manufactory` recipe `Ingredients: [{ Coin, 1 }]`, empty local products, plus

```jsonc
"GlobalInventoryRecipeSpec": { "GlobalProducts": [ { "Id": "Coin", "Amount": 1 } ] }
```

and `GlobalInventoryManufactorySpec` on the building.

**Spend global to craft a local good:** `GlobalIngredients` on the recipe; stall when the treasury is empty.

**Click UI:** `[OnEvent] void OnClick(GlobalGoodClickedEvent e)` → open your own panel.

---

## Implementation map

| Piece | Harmony? |
|---|---|
| `GlobalGoodSpec` + spec service | No |
| `GlobalInventoryService` + handles + save | No |
| `GlobalInventoryManufactory` stall/produce via `IManufactoryLimiter` + `ProductionFinished` | No |
| Top-bar group appended to `TopBarPanel._root` by a service that depends on `TopBarPanel` | No |
| Recipe preview via `IEntityDescriber` | No |
| Lack-of-resources status for global ingredients | Later |

Project references TimberUi only (no Harmony).

Tests (focused, no Given/When/Then names): clamp add/remove, capacity set throw vs safe, reveal/hide/HideOnZero, overflow stall vs `AllowOverflow`.

`BackwardCompatibleTemplateNames` stays `[]`.

---

## Open for later (not v1)

- Multiple named inventories (per faction, per district).
- CTB integration so players can move the global group.
- Built-in click panel / ledger history.
- Refund global ingredients on recipe cancel.
- `AllowUnderflow` / partial cycles.
- Demo mod with a mint building.
