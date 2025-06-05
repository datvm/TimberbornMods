---
title: "Part 5: Advanced Modding Techniques"
description: Some techniques I use in my mods to make modding easier and less repetitive.
date: 2025-03-13
tags: mods timberborn guide modding
---

Since I have made many mods, I need to keep my `.csproj` files short, and also reuse the publicized assemblies so I don't have to publicize them for each mod.

# Publicizer

Feel free to use my [Publicizer](https://github.com/datvm/TimberbornMods/tree/master/GameAssemblyPublicizer). It's a Console app that uses `BepInEx.AssemblyPublicizer` package, find all Timberborn assemblies in the `Timberborn\Timberborn_Data\Managed` folder, and publicizes them to the `out` folder. I only need to run this app once after each Timberborn update, and then I can use the publicized assemblies in my mods.

Beside the game's assemblies, I also publicize some other mods' assemblies in there. See below on how to refer them.

# Project Structure

Since I have made many mods, I need to keep my `.csproj` files shorter by reusing the targets. For example, a "complicated" mod like [Scientific Projects](https://steamcommunity.com/workshop/filedetails/?id=3442389397) has this `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputModFolderName>ScientificProjects</OutputModFolderName>
        <GameSolutionFolder>..\..\</GameSolutionFolder>
    </PropertyGroup>
    <Import Project="../../Common.target" />
    <Import Project="../../CommonTimberUI.target" />
    <Import Project="../../CommonBuffDebuff.target" />
</Project>
```

Feel free to check the `.targert` files in my [TimberbornMods repository](https://github.com/datvm/TimberbornMods/tree/master). For example, the `Common.target` with added explanations:

```xml
<!-- I even modularize the Common.target file further -->
<Import Project=".\CommonProperties.target" />
<Import Project=".\CommonGlobalUsings.target" />

<!-- Define where the mod will be output in each csproj file -->
<Target Name="Validate">
    <Error Condition="'$(OutputModFolderName)'==''" Text="OutputModFolderName was not specified" />
</Target>

<!-- General properties -->
<PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>preview</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <DebugSymbols>true</DebugSymbols>
    <DebugType>embedded</DebugType>
</PropertyGroup>

<!-- Copy the built files to the Timberborn mod folder -->
<Target Name="CopyFiles" AfterTargets="PrepareFilesForCopy">
    <Message Text="Deleting..." />
    <RemoveDir Directories="$(ModFolder)" />

    <Message Text="Copying..." />
    <Copy SourceFiles="@(ModManifestFile)" DestinationFolder="$(ModFolder)" />
    <!-- ... -->
</Target>

<!-- Use the publicized and other assemblies -->
<ItemGroup>
    <!-- IgnoresAccessChecks.cs is needed to use the publicized assemblies -->
    <Compile Include="$(GameSolutionFolder)GameAssemblyPublicizer\IgnoresAccessChecks.cs" Link="IgnoresAccessChecks.cs" />

    <!-- Refer to all the Publicized assemblies -->
    <Reference Include="$(GameSolutionFolder)GameAssemblyPublicizer\out\common\**\*.dll">
        <Private>false</Private>
    </Reference>

    <!-- These are not publicized but sometimes we need them -->
    <Reference Include="$(AssemblyPath)\System.Collections.Immutable.dll">
        <Private>false</Private>
    </Reference>
    <Reference Include="$(AssemblyPath)\Newtonsoft.Json.dll">
        <Private>false</Private>
    </Reference>
</ItemGroup>
```

Other `.target` are for projects that only use a certain feature like `CommonTimberUI` or `CommonHarmony`.

# Find components without Unity

Sometimes you may want to know which components are attached to a game object. When using AssetRipper, you can find the `.prefab` files but they use UUID references which may not be very useful in finding out what it is. You can use my [FileFinder](https://github.com/datvm/TimberbornMods/tree/master/FileFinder) to map the UUIDs to the actual component names.

You simply need to replace the `FileInput` constant and run it. It will search for all the UUID and print out the relevant files.