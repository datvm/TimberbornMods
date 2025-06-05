---
title: "Part 3: Modding with Harmony and Mod Settings"
description: Use Harmony and Mod Settings to patch Timberborn's code and configure your mods.
date: 2025-06-05
tags: mods timberborn guide modding
---

In this part, we will use the [Configurable Lifetime](https://steamcommunity.com/sharedfiles/filedetails/?id=3475071208) mod ([source code](https://github.com/datvm/TimberbornMods/tree/master/ConfigurableLifetime)) as an example:

![Settings](https://images.steamusercontent.com/ugc/12560402437949246673/79B3DBE53E9D371CFAF333A1045AA5E342B5C2F7/)

- Players can configure the lifetime of beavers and bots using [_Mod Settings_](https://steamcommunity.com/workshop/filedetails/?id=3283831040).
- We apply the changes using [Harmony](https://steamcommunity.com/workshop/filedetails/?id=3284904751).

# Add the dependencies

## DLL references

Subscribe to the two mods above in the Steam Workshop or download them from Mod.io. You will need to add references to them in your project's `.csproj` file:

```xml
<ItemGroup>
    <!-- Harmony -->
    <Using Include="HarmonyLib"/>

    <Reference Include="$(ModPath)3284904751\0Harmony.dll">
        <Private>False</Private>
    </Reference>

    <!-- Mod Settings -->
    <Reference Include="$(ModPath)3283831040\version-0.7\Scripts\*.dll">
        <Private>False</Private>
    </Reference>
</ItemGroup>
```

`<Private>False</Private>` means the DLLs are only used for compilation. Players will have them in their mods.

> **ℹ️ Note:**  
> If you use Steam, the path is `SteamLibrary\steamapps\workshop\content\1062090\` (wherever your `SteamLibrary` folder is).

## Manifest declaration

Declare them as required mods in your `manifest.json` to ensure players have them installed.

```jsonc
{
    // ...
    "RequiredMods": [
        { "Id": "eMka.ModSettings" },
        { "Id": "Harmony" }
    ]
}
```

# Create the Settings

[Source code: `MSettings.cs`](https://github.com/datvm/TimberbornMods/blob/master/ConfigurableLifetime/MSettings.cs)

## Declaration

Create a new class `MSettings` (or any name you like). It should inherit from `ModSettingsOwner`. Here I use a primary constructor:

```cs
public class MSettings(
  ISettings settings,
  ModSettingsOwnerRegistry modSettingsOwnerRegistry,
  ModRepository modRepository)
: ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public override string ModId { get; } = nameof(ConfigurableLifetime);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;
}
```

- `ModId` is the ID of your mod so Mod Settings knows which one to show.
- Since in this mod, settings can be changed at any time, we set `ChangeableOn` to `ModSettingsContext.All`. The default is `MainMenu` only.

## Settings properties

```cs
public ModSetting<float> BeaverLifeMul { get; } = new(1f, ModSettingDescriptor
    .CreateLocalized("LV.CLt.BeaverLifeMul")
    .SetLocalizedTooltip("LV.CLt.BeaverLifeMulDesc"));
public ModSetting<float> ChildhoodDaysMul { get; } = new(1f, ModSettingDescriptor
    .CreateLocalized("LV.CLt.ChildhoodDaysMul")
    .SetLocalizedTooltip("LV.CLt.ChildhoodDaysMulDesc"));
public ModSetting<float> BotLifeMul { get; } = new(1.0f, ModSettingDescriptor
    .CreateLocalized("LV.CLt.BotLifeMul")
    .SetLocalizedTooltip("LV.CLt.BotLifeMulDesc"));
```

Here we define the settings we want to expose to the player. There are many kinds of `ModSetting` (bool, int, float, int slider, string, string select box, etc.). You can see them all [in the mod's source code](https://github.com/eMkaQQ/timberborn-modding/tree/main/Assets/Mods/ModSettings/Scripts/ModSettings.Common).

Keys like `LV.CLt.BotLifeMul` are defined in your Localizations file, as mentioned in [the previous part](./modding-basics#manifest-and-localizations-files).

The mod automatically finds all public `ModSetting` properties and adds them. I did not know this at first and most of my earlier mods I add them manually using `AddCustomModSetting`, in case you want to do that (for example, to loop through a list):

```cs
for (int i = 0; i < DefaultDepths.Length; i++)
{
    var z = i;

    RangeIntModSetting maxDepth = new(DefaultDepths[z], 1, 30,
        ModSettingDescriptor.Create(t.T("LV.CE.MaxDepth", t.T(DynamiteNames[z]))));

    AddCustomModSetting(maxDepth, MaxDepthKey + z);

    maxDepth.ValueChanged += (_, v) => MaxDepths[z] = v;
    MaxDepths[z] = maxDepth.Value;
}
```

## Keep a reference

Harmony methods are static so we need to keep a reference to the settings instance. Since `ModSettingsOwner` is already an `ILoadableSingleton`, we can use its `OnBeforeLoad` and `OnAfterLoad` methods instead:

```cs
public static MSettings? Instance { get; private set; }

public override void OnAfterLoad()
{
    base.OnAfterLoad();
    Instance = this;
}

public void Unload()
{
    Instance = null;
}
```

## Register the `MSettings` class

Now we need to register this class. I usually do this in a [`MConfigs.cs` file](https://github.com/datvm/TimberbornMods/blob/master/ConfigurableLifetime/MConfigs.cs):

```cs
[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}
```

> **💡 Tip:**  
> Even if players cannot change the settings in-game, you should still register the `MSettings` class in the `Game` context so that it can be used in Harmony patches, and so the players can see the settings while in-game.

# Use Harmony patches

[Source code: `LifetimePatches.cs`](https://github.com/datvm/TimberbornMods/blob/master/ConfigurableLifetime/Patches/LifetimePatches.cs)

## Run Harmony patching

Harmony [(documentation)](https://harmony.pardeike.net/articles/intro.html) only needs to run once because it patches the game code and then it is done. Usually, I run it in an `IModStarter` class:

```cs
public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableLifetime)).PatchAll();
    }

}
```

## Patches

In this mod, I need to patch class `Timberborn.LifeSystem.LifeProgressor`'s `IncreaseLifeProgress` method:

```cs
[HarmonyPatch]
public static class LifetimePatches
{
    // ...

    [HarmonyPrefix, HarmonyPatch(typeof(LifeProgressor), nameof(LifeProgressor.IncreaseLifeProgress))]
    public static bool StopLifeProgress(LifeProgressor __instance)
    {
        var mul = MSettings.Instance?.BeaverLifeMul.Value;
        if (mul is null or 1f || IsChild(__instance))
        {
            return true;
        }

        __instance.LifeProgress +=
            __instance._lifeProgressIncreasePerTick
            * mul.Value
            / __instance._bonusManager.Multiplier(LifeProgressor.LifeExpectancyBonusId);

        return false;
    }
}
```

Here, we mark `LifetimePatches` class with the `[HarmonyPatch]` attribute so Harmony knows to look for patches in this class. The `[HarmonyPrefix]` attribute means this method will run before the original method.

Then we use `MSettings.Instance` to get the current `MSettings` instance and access the value the player chose. Anything else is up to the logic of your mod.

> **ℹ️ Note:**  
> It may be disappointing that this is all I can write in this section. Harmony is powerful, but it's up to you to decide how to use it: what should I patch, what should I change, Prefix or Postfix or even the Transpiler patch. You have to read the game code and understand it to know what to do.

# Conclusion

In this part, we learned how to use Harmony to patch Timberborn's code and how to use Mod Settings to allow players to configure our mods. This is a powerful combination that allows you to change game behavior and make it more customizable.

[Part 4: Useful knowledge of Timberborn architecture](./timberborn-architecture)

[Back to Modding Guide](./)