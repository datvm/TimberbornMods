---
title: "Timberborn Modding Guide"
description: A comprehensive guide to making mods for Timberborn
date: 2025-03-13
tags: mods timberborn guide modding
---

I have made about 30+ mods for Timberborn and [all source code are available here](https://github.com/datvm/TimberbornMods). This guide is a collection of my experience and knowledge about modding Timberborn. I hope it will help you to make your own mods for Timberborn. Throughout this guide, I will introduce a few mods in that repository to illustrate the concepts and techniques I am talking about.

> **Note:**  
> Feel free to use my code in your mods. I would appreciate it if you credit me but it is not required. Unless you make a carbon copy, feel free to make and upload similar mods to mine. I don't mind competition and I am happy to see more mods for Timberborn.

- **None of my mods uses Unity** and so this guide will not cover Unity modding. You can still do powerful things without Unity except maybe making new graphical assets.

- You will need some basic knowledge of C# and general programming concepts but making mods is also a good way to improve your programming skills. The game code itself is extremely well-organized and easy to read so you will have a good time exploring it and even learning from it.

Useful external links:
- [Official Timberborn Discord](https://discord.gg/6VZfcKGM2Z): you can ask modding question there at `#mod-creators` channel.
- [Official Timberborn Modding](https://github.com/mechanistry/timberborn-modding).
- [My mods' source code](https://github.com/datvm/TimberbornMods).

> **Warning:**  
> This guide will use Update 7 as the base version (which at this time is still in experimental branch). I will probably mention how to make it compatible to U6 but U7 will be the way forward.

## Table of Contents

1. [Getting Started](./getting-started): Setting up your modding environment and making a simple code-less mod.
2. [Modding Basics](./modding-basics): Make a simple C# mod for Timberborn.
3. [Modding with Harmony and Mod Settings](./mod-settings-and-harmony): How to use Harmony to patch Timberborn's code and use Mod Settings to configure your mods.
4. _(Optional)_ [Useful knowledge of Timberborn architecture](./timberborn-architecture): My experience and knowledge about Timberborn's codebase that may come in handy when making mods.
5. _(Optional)_ [Advanced Modding Techniques](./advanced-modding): Some techniques I use in my mods to make modding easier and less repetitive.

Extra useful stuff:

- [Attach a debugger to Timberborn](./attach-debugger): How to attach a debugger to Timberborn and debug the game code or your mods using DnSpy and Visual Studio.

The following sections are more specific to certain types of mods that I made:

1. [UI modding with TimberUi](./timber-ui): How to make UI mods for Timberborn using [TimberUi](../TimberUI).
2. [Make a mod with Buff & Debuff mod](../BuffDebuff): Adding customizable buffs and debuffs to various in-game entities.
3. [Make a mod with Scientific Projects mod](../ScientificProjects): Adding new research projects to the game.
4. [Make a code-less mod (using JSON) with Moddable prefab](../ModdablePrefab): Adding components to a prefab or changing their values using JSON (Blueprints) files.