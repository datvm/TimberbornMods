---
title: "Part 4: Useful knowledge of Timberborn architecture"
description: A comprehensive guide to making mods for Timberborn
date: 2025-06-05
tags: mods timberborn guide modding architecture
---

Below are some architecture knowledge I have learned from modding Timberborn. It may help you to understand the game code better and make your mods more efficient.

# Dependency Injection

Timberborn uses Bindito as its in-house dependency injection framework. I write about its lifetime management, scopes and some popular Multibinding like `TemplateModule` in [Bindito](./di).

# Base Component

Timberborn uses `BaseComponent` as the base class for all components. Its `GetComponentFast` is useful to quickly get a component in a prefab.

You can also use `TemplateModule` to dynamically add a component to a prefab with another kind of component. [See more in the DI guide on `TemplateModule`](./di.MD#templatemodule).

```cs
public class MyWaterSourceComponent : BaseComponent
{

    #nullable disable
    WaterSource waterSource;
    EventBus eventBus;
    #nullable enable

    public void Awake() 
    {
        waterSource = GetComponentFast<WaterSource>();
    }

    public void Inject(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public void Start()
    {
        // Use reference to waterSource and eventBus here
    }

}
```

There is also `TickableComponent` that is used for components that need to be updated every game tick.

```cs
public class MyWaterSourceComponent : BaseComponent
{

    #nullable disable
    WaterSource waterSource;
    #nullable enable

    public void Awake() 
    {
        waterSource = GetComponentFast<WaterSource>();
    }

    public override void Tick()
    {
        waterSource.SetSpecifiedStrength(
            waterSource.SpecifiedStrength + Time.deltaTime * 0.1f);
    }

}
```