# Global Inventory

**Global Inventory** keeps colony-wide amounts that are not stored in a building and not carried by beavers. Money, reputation, and similar values belong here when several mods need to share them.

This mod adds no buildings and no goods. Another mod declares the goods, then reads and changes them through `GlobalInventoryService`.

Require it from your mod manifest:

```json
"RequiredMods": [
  { "Id": "GlobalInventory" }
]
```

---

## Declare a good

Add a blueprint whose only component is `GlobalGoodSpec`. Ids must be unique across every loaded mod.

```json
{
  "GlobalGoodSpec": {
    "Id": "Reputation",
    "DisplayNameLocKey": "LV.MyMod.Reputation",
    "PluralDisplayNameLocKey": "LV.MyMod.Reputation.Plural",
    "Icon": "Goods/ReputationIcon",
    "Order": 10,
    "MinCapacity": 0,
    "MaxCapacity": 100,
    "InitialAmount": 0,
    "HideOnZero": true
  }
}
```

| Field | Meaning |
|---|---|
| `Id` | Lookup key. Required. |
| `DisplayNameLocKey` / `PluralDisplayNameLocKey` | Tooltip name. Plural is used when it is set. |
| `Icon` | Required, unless `CopiedGoodId` supplies one. |
| `Order` | Top-bar order. Ties use the display name. |
| `MinCapacity` | Lowest allowed amount. Default `0`. Can be negative. |
| `MaxCapacity` | Highest allowed amount. Omit it for no maximum. |
| `InitialAmount` | Starting amount. Must sit inside the capacity. A non-zero start shows the row immediately. |
| `HideOnZero` | Hide the top-bar row again when the amount returns to `0`. |
| `CopiedGoodId` | Copy id, names, and icons from a vanilla `GoodSpec`. Capacity and visibility fields stay yours. |

`InitialAmount` outside the capacity, `MaxCapacity` below `MinCapacity`, a missing name, a missing icon, or a duplicate `Id` fails at load.

---

## Change an amount

Inject `GlobalInventoryService` and take a `GlobalGoodHandle`:

```cs
var reputation = inventory.Get("Reputation");
reputation.Add(5);
var removed = reputation.RemoveSafe(3);
```

| Method | What it does |
|---|---|
| `Get` / `TryGet` | Look up a declared good. `Get` throws when the id is missing. |
| `Add` / `Remove` / `Set` | Change the amount. Throws when the result is outside the capacity. |
| `AddSafe` / `RemoveSafe` / `SetSafe` | Same change, clamped to the capacity. Returns how much was actually applied. |
| `SetCapacity` | Replace min and max. Throws when the current amount would fall outside, or when max is below min. |
| `SetCapacitySafe` | Same replacement, then clamps the current amount. |
| `Hide` | Hides the top-bar row. Does nothing unless the amount is `0`. |

`All` is every declared good, in top-bar order. `Visible` is the rows the player can currently see.

Any real amount change reveals the good. `HideOnZero` hides it again at `0`.

---

## Events

Subscribe on `EventBus`.

| Event | When |
|---|---|
| `GlobalGoodAmountChangedEvent` | Amount changed. Carries the handle, previous amount, current amount, and the applied delta. |
| `GlobalGoodCapacityChangedEvent` | Min or max changed. |
| `GlobalGoodVisibilityChangedEvent` | The top-bar row appeared or disappeared. |
| `GlobalGoodClickedEvent` | The player clicked that row. |

---

## Top bar

Once any good is visible, a **Global** group is added after the vanilla good groups. Each row shows that good's own amount. The tooltip shows the name, the amount, the max when one is set and is not `0`, and the min when it is not `0`. A max also draws the fill bar.

The group hides itself when every row is hidden.

---

## Workshops

Put an empty `GlobalInventoryManufactorySpec` on the building and a `GlobalInventoryRecipeSpec` on the recipe.

```json
{
  "RecipeSpec": {
    "Id": "BuyFavor",
    "DisplayLocKey": "LV.MyMod.BuyFavor",
    "CycleDurationInHours": 1,
    "Ingredients": [],
    "Products": []
  },
  "GlobalInventoryRecipeSpec": {
    "GlobalIngredients": [
      { "Id": "Reputation", "Amount": 10 }
    ],
    "GlobalProducts": [
      { "Id": "Favor", "Amount": 1 }
    ],
    "AllowOverflow": false
  }
}
```

Each entry needs a declared global good id and an amount greater than `0`.

Global ingredients are taken when the cycle consumes its vanilla ingredients. Global products are given when the cycle finishes. Products use `AddSafe`, so anything past the max is dropped.

The workshop does not progress while:

- global ingredients are still unpaid and spending them would pass `MinCapacity`, or
- `AllowOverflow` is false and a product would pass `MaxCapacity`.

`AllowOverflow` only skips that second stall. The product is still clamped.

Global ingredients and products are drawn on the vanilla recipe description, in the building preview and on the production panel.

The [Science Shop](../../ScienceShop/Docs/README.MD) is one building that already has `GlobalInventoryManufactorySpec`.

---

## Dev tools

Dev mode includes **Global Inventory: Modify**. It lists every global good. Add and remove use the safe methods. Clear sets the amount to `0`, or to the min when the min is above `0`. Fill appears when a max is set. Set to Min appears when the min is below `0`.
