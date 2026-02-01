To group your decals (Tails or Banners), use `DecalGroupSpec` entries by creating `*.blueprint.json` files in your mod.

For example:
```jsonc
{
    "DecalGroupSpec": {
        "Id": "BannerGood",
        "TitleLoc": "LV.MDG.BannerGoods.Goods",
        "Category": "Banners",
        "Order": 900,
        "DecalIdExacts": [
            "CustomBanner.Antidote",
            "CustomBanner.Badwater",
            // More...
        ]
    }
}
```

- `Id`: Unique identifier for the decal group.
- `TitleLoc`: Localization key for the decal group title.
- `Category`: Which category the decal group belongs to (e.g., "Banners" or "Tails").
- `Order`: The order in which the decal group appears.
- `DecalIdExacts`: An array of exact decal Ids to include in this group. If any Id does not exist, it's simply ignored.

You can also use `DecalIdPatterns` to match multiple decals with a [Regular Expression](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions) pattern. For example:

```json
{
    "DecalGroupSpec": {
        "Id": "BannerAlphabet.Capital",
        "TitleLoc": "LV.MDG.BannerAlphabet.Capital",
        "Category": "Banners",
        "Order": 1010,
        "DecalIdPatterns": [
            "CustomBanner\\.01BasicCapital.+",
            "CustomBanner\\.05SupplementCapital.+"
        ]
    }
}
```

You can combine both `DecalIdExacts` and `DecalIdPatterns` in the same group, and decals will be included if they match either criterion. A single decal can also appear in multiple groups.