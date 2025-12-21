With `ToolGroupChildrenSpec` and similar specs, you can create mods that rearrange the tool groups and tools for Block Objects without writing any code.

This mod provides [the following Specs](../../ConfigurableToolGroups/Specs/):

## Give Groups and Tools new parents

[`ParentToolGroupSpec`](../../ConfigurableToolGroups/Specs/ParentToolGroupSpec.cs) can be applied to `BlockObjectToolGroupSpec` or any block object blueprints (templates).

For example, the following JSON moves the "Badwaters" tool group under the "Water" tool group:

```jsonc
{
    "BlockObjectToolGroupSpec": {
        "Id": "Badwaters",
        "Order": 20,
        "NameLocKey": "LV.OG.GroupBadwaters",
        "Icon": "Sprites/OrganizedGroups/Badwaters",
        "FallbackGroup": false
    },
    "ParentToolGroupSpec": {
        "ParentIds": [ "Water" ]
    },
}
```

## Declare a Group's children and their orders

[`ToolGroupChildrenSpec`](../../ConfigurableToolGroups/Specs/ToolGroupChildrenSpec.cs) can only be applied to `BlockObjectToolGroupSpec` but it is very powerful that gives you many options. For example:

```jsonc
{
    "BlockObjectToolGroupSpec": {
        "Id": "Surfaces",
        "Order": 10,
        "NameLocKey": "LV.OG.GroupSurfaces",
        "Icon": "Sprites/OrganizedGroups/Surfaces",
        "FallbackGroup": false
    },
    "ParentToolGroupSpec": {
        "ParentIds": [ "Paths" ]
    },
    "ToolGroupChildrenSpec": {
        "ChildrenGroupsIds": [
            "Platforms",
            "Bridges",
            "Overhangs"
        ],
        "ChildrenToolsTemplateNames": [
            "MetalPlatform3x3.Folktails",
            "MetalPlatform3x3.IronTeeth",
            "MetalPlatform5x5.Folktails",
            "MetalPlatform5x5.IronTeeth"
        ],
        "ChildrenOrderedIds": [
            "Path",
            "Subgroup_MGMetalPlatform"
        ],
        "ChildrenExplicitOrderedIds": [
            {
                "Id": "Subgroup_MGZiplines",
                "Order": 15
            }
        ],
        "DoNotIncludePlaceableToolGroup": true
    }
}
```

- This example creates a new "Surfaces" tool group as a child of the "Paths" tool group using `ParentToolGroupSpec`.
- `ChildrenGroupsIds` declares that "Platforms", "Bridges", and "Overhangs" tool groups are children of the "Surfaces" tool group.
- `ChildrenToolsTemplateNames` declares that the specified tools (by their template names) are also children of the "Surfaces" tool group.
- `DoNotIncludePlaceableToolGroup` tells the mod to **NOT** also keep it in the `PlaceableBlockObjectSpec.ToolGroupId` group. If you do not set this to `true`, Metal Platforms will appear twice: once in "Surfaces" and once in "Paths".

The above properties do not affect the Order in which they appear.

## The Children's Order

From v10.3.0, this is how the order of children is determined:
1. `ChildrenExplicitOrderedIds` - If you specify explicit orders for certain children, they have that Order number.
2. `ChildrenOrderedIds` - A fast way to implicitly assign Order numbers: any that appear here that did not appear in `ChildrenExplicitOrderedIds` are assigned Order numbers based on their position in this list: 10, 20, 30, etc.
3. Any remaining children (not mentioned in either of the above) use their existing Order numbers (from `BlockObjectToolGroupSpec.Order` or `PlaceableBlockObjectSpec.Order`).

After that, all children are sorted by their Order numbers (ascending).