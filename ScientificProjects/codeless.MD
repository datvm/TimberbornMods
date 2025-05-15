---
title: "Scientific Projects"
description: Modify Scientific Projects without any code.
date: 2025-05-14
tags: mods timberborn guide modding scientific projects
---

With this guide you can make an extension mod that **modifies** other Scientific Projects (including the ones added by other extension mods). Note that you _cannot_ add a new project without code. You can also rearrange the grouping of the projects, including creating a new group and moving the projects around into different groups.

You should have read at least the first step of the guide: [Getting Started](../ModdingGuide/getting-started).

# Changing a simple project

All projects are defined by `ScientificProjectSpec` JSON files. You can find them in the mods' `Blueprints` folders or in the mods' source code [here](#to-be-updated). For example, consider the _Resin-Infused Planks_ project:

```jsonc
// ScientificProjects\Factions\FtPlankUpgrade.ScientificProjectSpec.json
{
    "ScientificProjectSpec": {
        "Id": "FtPlankUpgrade",
        "GroupId": "Factions",
        "NameKey": "LV.SP.FtPlankUpgrade",
        "EffectKey": "LV.SP.FtPlankUpgradeEff",
        "LoreKey": "LV.SP.FtPlankUpgradeLore",
        "Icon": "Sprites/Projects/ftplank",
        "ScienceCost": 2500,
        "Parameters": [ 1, -0.9999 ],
        "Order": 10,
        "Factions": [ "Folktails", "GreedyBuilders" ]
    }
}
```

Most of the time, you will want to change the `ScienceCost` or the `Parameters` properties. If you are not sure which number the parameters are, check the `Localizations\enUS.csv` file, the description should have placeholders explaining which parameter corresponds to which effect.

```csv
LV.SP.FtPlankUpgradeEff,"Woodworkshop uses {1:#.###%} power and {0:+#%} the Treated Planks output.",
```

**Note**: the parameter index starts at 0. So you can see the first number in `Parameters` is for the output multiplier (but when showing as text, it's formatted as a percentage for easy reading) while the second number is the power reduction, again formatted as a percentage when showing on the UI.

For example, let's say you want to increase the science cost but also increase the output (1 stands for +100% output), in your mod, create a file `FtPlankUpgrade.ScientificProjectSpec.json` anywhere in the `Blueprints` folder:

```jsonc
// Blueprints\Vanilla\FtPlankUpgrade.ScientificProjectSpec.json
{
    "ScientificProjectSpec": {
        "ScienceCost": 5000,
        "Parameters": [ 2, -0.9999 ]
    }
}
```

# The Projects Group

As you can see above, the project has the `"GroupId": "Factions"` property. You should see the `blueprints\ScientificProjectGroups\Factions.ScientificProjectGroupSpec.json` file that defines that group:

```json
// ScientificProjectGroups\Factions.ScientificProjectGroupSpec.json
{
    "ScientificProjectGroupSpec": {
        "Id": "Factions",
        "NameKey": "LV.SP.FactionsGroup",
        "DescKey": "LV.SP.FactionsGroupDesc",
        "Order": 40
    }
}
```

You can create, modify and move projects into other groups as you wish.

# Daily and Custom Cost projects

Some other projects like "Precision Tool Maintenance" (`WorkEffUpgrade2.ScientificProjectSpec.json`) are Daily-cost projects. These projects do not have an up-front cost to unlock but instead use `RequiredId` to be locked by a prerequisite project (note that a normal one-time cost project can still be locked with `RequiredId`):

```jsonc
// ScientificProjects\WorkEffs\WorkEffUpgrade2.ScientificProjectSpec.json
{
    "ScientificProjectSpec": {
        "Id": "WorkEffUpgrade2",
        "RequiredId": "WorkEffUpgrade1",
        "GroupId": "WorkEff",
        "NameKey": "LV.SP.WorkEffUpgrade2",
        "EffectKey": "LV.SP.WorkEffUpgrade2Eff",
        "LoreKey": "LV.SP.WorkEffUpgrade2Lore",
        "Icon": "Sprites/Projects/WorkEffUpgrade2",
        "ScienceCost": 15,
        "MaxSteps": 5,
        "HasScalingCost": true,
        "ScalingCostKey": "LV.SP.WorkEffUpgrade2Cost",
        "Parameters": [ 0.1, 15, 20 ],
        "Order": 20
    }
}
```

With `"MaxSteps": 5`, the project is considered a Daily-cost project automatically and `ScienceCost` becomes the normal daily cost per step instead.

However, if you read the Description, you will notice this project cost scales with the number of Adult beavers you have too (and is defined by `"HasScalingCost": true`):

```csv
LV.SP.WorkEffUpgrade2Eff,"{0:+#%} additional work efficiency of all beavers per Level.",
LV.SP.WorkEffUpgrade2Cost,"[Cost] per Level, and {1} for every {2} Adult beavers (rounded up)",
```

With `"HasScalingCost": true`, `ScalingCostKey` is required and it's handled with custom C# code but the text has access to a special placeholder `[Cost]` which is the `ScienceCost`. So for this project `Parameters`, the first number is the Work Efficiency boost **per Level/step**, `ScienceCost` is the flat cost per Level, and the second number is the cost increase per the third number of adult beavers.

For example, let's say we want to keep the flat cost but increase the science cost to 30 per 30 beavers instead, and also increase the maximum Levels/Steps to 10:

```json
// Blueprints\Vanilla\WorkEffUpgrade2.ScientificProjectSpec.json
{
    "ScientificProjectSpec": {
        "MaxSteps": 10,
        "Parameters": [ 0.1, 30, 30 ]
    }
}
```

# Conclusion

This guide likely covers all scenarios in which you can mod Scientific Projects without C# code. You can likely make significant balance changes with this approach but unfortunately you cannot really change the behavior or add a new project.

In order to do that, you will need to code a mod using C#. The [Modding Scientific Projects with C# guide](./coding) would come in handy.

[Back to Scientific Projects](./)