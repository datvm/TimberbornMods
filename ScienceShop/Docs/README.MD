# Science Shop

**Science Shop** is a 1-worker workshop in the **Science** group. Each recipe cycle spends Science Points. This mod ships no recipes. Your mod adds them.

Players queue work from the shop panel. The vanilla recipe dropdown is hidden. How that panel works is on the mod page. This page is how to add a recipe.

Require both mods:

```json
"RequiredMods": [
  { "Id": "GlobalInventory" },
  { "Id": "ScienceShop" }
]
```

The shop itself already depends on [Global Inventory](../../GlobalInventory/Docs/README.MD).

---

## Add a recipe

Create a normal `RecipeSpec`, then stack `ConsumeScienceRecipeSpec` on it. `ScienceCost` must be greater than `0`. `Order` sorts the shop dropdown. Lower comes first. Equal orders use the recipe name.

```json
{
  "RecipeSpec": {
    "Id": "MyRecipe",
    "DisplayLocKey": "LV.MyMod.MyRecipe",
    "CycleDurationInHours": 15,
    "Ingredients": [
      { "Id": "Log", "Amount": 10 }
    ],
    "Products": [],
    "Icon": "Goods/MyRecipeIcon"
  },
  "ConsumeScienceRecipeSpec": {
    "ScienceCost": 150,
    "Order": 10
  }
}
```

Append that recipe id onto both shop buildings:

`Buildings/Science/ScienceShop/ScienceShop.Folktails.blueprint`

`Buildings/Science/ScienceShop/ScienceShop.IronTeeth.blueprint`

```json
{
  "ManufactorySpec": {
    "ProductionRecipeIds#append": [
      "MyRecipe"
    ]
  }
}
```

Science Points are spent when the cycle takes its ingredients, not when it finishes. If the colony does not have enough science, the cycle does not start. A finished, staffed shop with a queue waiting on science shows the science status icon.

Vanilla ingredients, products, and fuel still work. Science cost is drawn on the recipe description next to them.

---

## Global goods

Optional. A recipe can produce a normal good from `RecipeSpec.Products` and never touch global inventory.

Stack `GlobalInventoryRecipeSpec` on the same recipe only when the cycle should also spend or produce a [global good](../../GlobalInventory/Docs/README.MD). The shop already has an empty `GlobalInventoryManufactorySpec`.

```json
"GlobalInventoryRecipeSpec": {
  "GlobalProducts": [
    { "Id": "MyGlobalGood", "Amount": 1 }
  ]
}
```

Declare that good with `GlobalGoodSpec` in your own mod. Global ingredients are taken with the science, at the start of the cycle. Global products are given when the cycle finishes.

---

## The building

Both factions have the same shop.

| | |
|---|---|
| Group | Science, last in the list |
| Unlock | 150 Science |
| Bots | +1500 Science |
| Cost | 25 Planks, 25 Gears |
| Workers | 1 |
| Size | 2×2, 3 high |

`ConsumeScienceManufactory` is the queue. The panel is the normal way to use it. From code, `Add`, `AddIndefinite`, `RemoveOne`, and `ClearQueue` change that same queue. A change does not cancel a cycle that has already spent its science.
