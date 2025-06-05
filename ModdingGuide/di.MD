---
title: "Bindito"
description: Timberborn's dependency injection framework
date: 2025-06-05
tags: mods timberborn guide modding architecture bindito di dependency injection
---

Timberborn uses Bindito as its in-house dependency injection framework.

# Declaration & Injection

Since U7, you usually use `Configurator` classes to declare your services:

```cs
[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton(); // Bind a class as a singleton service
        MultiBind<ISomeInterface>().ToExisting<MSettings>(); // Bind an interface to an existing singleton
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<ISomeInterface>().As<SomeImplementation>(); // Bind an interface to its implementation
        MultiBind<TemplateModule>().ToProvider(() => GetTemplateModule()); // Bind a multibinding to a provider function

        // Bind a class as a transient service,
        // meaning a new instance will be created every time it is requested
        Bind<MyClass>().AsTransient();
    }
}
```

You can use multiple `ContextAttribute` on the same class but in my experience, usually it is better to separate them into different classes because sometimes not all services are available in all contexts. For example, `CameraService` is not available in the main menu context.

You can then inject them to your services:

```cs
public class MyService(
    MSettings settings, // Inject a singleton service
    ISomeInterface someInterface, // Inject an interface
    IEnumerable<TemplateModule> templateModules // Inject a multibinding
) { }
```

> **ℹ️ Note:**  
> In multibindings, you can inject them even if no implementation is bound. You will just get an empty collection. However, if you inject a single service that is not bound, it will throw an exception. Be aware of cyclic dependencies as well, Bindito will throw an exception if it detects a cycle.

In some cases, you cannot use constructor injection, for example `BaseComponent` or `MonoBehaviour` that must have a parameterless constructor. In that case, you can use `[Inject]` to inject the dependencies manually:

```cs
public class MyComponent : BaseComponent
{
    MSettings settings = null!;
    ISomeInterface someInterface = null!;

    [Inject]
    public void InjectDependencies(MSettings settings, ISomeInterface someInterface)
    {
        this.settings = settings;
        this.someInterface = someInterface;
    }
}
```

- The method must be public. The name does not matter. You can have multiple methods with `[Inject]` attribute in the same class. It even works for inherited classes.

# Manually get a service

If you need to get a service manually, get a reference to `IContainer` service. I sometimes use this for UI elements:

```cs
public class MyDialogController(IContainer container)
{

    public void Show() 
    {
        // MyDialog is usually a transient service but it can be anything
        var diag = container.GetInstance<MyDialog>();
    }

}
```

There is also the `IContainer.Inject` method that can inject dependencies into an existing object.

# Popular Multibindings

These are some popular multibindings I think you will use often:
- `TemplateModule`: Used to [add a component to a prefab with another component](./timberborn-architecture#base-component).
- `EntityPanelModule`: Add an info panel (the panel to the right of the screen when you select an entity). You can use [TimberUI](../TimberUI/) to make it easier.
- `IDevModule`: Add debug commands to the game.
- `IWaterStrengthModifier`: Used to modify the water strength of a `WaterSource`.

## TemplateModule

The game usually uses `TemplateModule` to add a component to a prefab. For example:

```cs
	protected override void Configure()
	{
		Bind<WaterStrengthService>().AsSingleton();
		MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
	}

	private static TemplateModule ProvideTemplateModule()
	{
		TemplateModule.Builder builder = new TemplateModule.Builder();
		builder.AddDecorator<WaterSourceSpec, WaterSource>();
		builder.AddDecorator<WaterSource, DroughtWaterStrengthModifier>();

        // ...

		return builder.Build();
	}
```

Which means, in the prefab, they only need to add `WaterSourceSpec` with all the required properties. Then, all prefabs with `WaterSourceSpec` will automatically have the `WaterSource` component and the `DroughtWaterStrengthModifier` component added to them.

So for example, in my [Moddable Weather](https://steamcommunity.com/workshop/filedetails/?id=3493039008) mod, I add the Monsoon and SurprisinglyRefreshing like this (using TimberUI's extension method to help with the syntax):

```cs
configurator.BindTemplateModule(h => h
    .AddDecorator<WaterSource, MonsoonWaterStrengthModifier>()
    .AddDecorator<WaterSourceContamination, SurprisinglyRefreshingController>()
    // ...
);
```