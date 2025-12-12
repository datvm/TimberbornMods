With this mod you can also create your own programmable buttons on the Bottom Bar. The mod provides a few popular base classes you can extend depending on your needs.

See [the Demo project](../../ModdableToolGroupsDemo/) or [the Builtin replacements](../../ConfigurableToolGroups/UI/) for sample codes.

# Custom element

[`CustomBottomBarElement`](../../ConfigurableToolGroups/UI/CustomBottomBarElement.cs) is the most basic class to create your own Bottom Bar element. Everything else builds on top of this class and you will need to `MultiBind` to this type to use it.

## The Spec

First, you need to declare a `CustomBottomBarElementSpec`:

```json
{
    "CustomBottomBarElementSpec": {
        "Id": "GameBlockObjectButtons",
        "Order": 10
    },
    "CustomRootElementSpec": {
        "Location": "Middle"
    }
}
```

As your can see, if it should show up at the Root level, you also need to declare `CustomRootElementSpec`. The default `Location` is `Left`.

All root elements will be sorted by their `Order` value.

## The implementation

[`CustomBlockObjectButtons`](../../ConfigurableToolGroups/UI/CustomBlockObjectProviders/CustomBlockObjectButtons.cs) and its implementation [`GameBlockObjectButtonsCustomRootElement`](../../ConfigurableToolGroups/UI/CustomBlockObjectProviders/GameBlockObjectButtonsCustomRootElement.cs) use `CustomBottomBarElement` to add multiple group buttons to the middle of the Bottom Bar.

```cs
public abstract class CustomBlockObjectButtons(
    ModdableToolGroupButtonFactory grpButtonFac,
    BlockObjectToolButtonFactory boBtnFac
) : CustomBottomBarElement(grpButtonFac, boBtnFac)
{

    // In implementation of GameBlockObjectButtons:
    public override string Id { get; } = nameof(GameBlockObjectButtons);

    public override IEnumerable<BottomBarElement> GetElements()
    {
        // In a loop to output multiple buttons:
        // ...
        yield return btn.ToBottomBarElement();        
    }
}
```

You need to have a `CustomBottomBarElement` implementation with `Id` matching the spec. You can multi-bind it manually or use the mod's extension method `MultiBindCustomTool` for convenience:

```cs
[Context("Game")]
public class MGameConfig : Configurator
{

    public override void Configure()
    {
        this
            .MultiBindCustomTool<BuilderPrioritiesButtonCustomRootElement>()
            // ...
            // This mod replace the original buttons with its own so they can be rearranged.

            .MultiBindCustomTool<GameBlockObjectButtonsCustomRootElement>()
        ;
    }

}
```

## Remove existing Bottom Bar elements

Sometimes you may want to remove existing Bottom Bar elements to avoid duplication. For example the mod itself removes all built-in tools and replace with its own for ordering. The demo project also remove all the Dev buttons to put them into a group instead.

Simply implement `IBottomBarElementsRemover` and bind it like [OriginalDevButtonsRemover](../../ModdableToolGroupsDemo/UI/OriginalPlantingButtonsRemover.cs):

```cs
public class OriginalDevButtonsRemover : IBottomBarElementsRemover
{
    public ImmutableArray<Type> RemovingTypes { get; } = [
        typeof(BeaverGeneratorButtonCustomRootElement),
        typeof(BotGeneratorButtonCustomRootElement),
        typeof(WaterHeightBrushButtonCustomRootElement),
    ];
}

// MultiBind using extension method
[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        // Remove original buttons & add the new Dev group
        this
            .MultiBindElementsRemover<OriginalDevButtonsRemover>()
            .MultiBindCustomTool<DevToolGroupElement>()
        ;
        // ...
    }
}
```

As you can see the above class actually removes the `BeaverGeneratorButtonCustomRootElement` and the likes added by the mod to replace the game's `BeaverGeneratorButton`.

## Custom tool button

![Groupless Button](https://files.catbox.moe/z6uo4b.jpg)

You can create your own tool button quickly using `CustomRootToolElement<T>`. In the game code it's called Groupless tool button:

```cs
public class DevToggleButtonElement : CustomRootToolElement<DevToolToggleTool>
{

    public override string Id { get; } = "LV.DevToggleButton";
    protected override string ImageName { get; } = "ToggleDevTool";

    public DevToggleButtonElement(ToolButtonFactory toolButtonFactory, DevToolToggleTool tool) : base(toolButtonFactory, tool)
    {
        Color = RootToolButtonColor.Red;
    }
}
```

Remember to multibind it.

### Simple Root button

If you just need a simple button without an associated tool (like the game's `ShowOptionsButton`), you can use the built-in `SimpleRootButton`:

```cs
public class DevToggleButtonElement(
    VisualElementLoader loader,
    ToolButtonFactory toolButtonFactory
) : SimpleRootButton(loader, toolButtonFactory), ILoadableSingleton
{

    public override string Id { get; } = "LV.DevToggleButton";
    protected override string ImageName { get; } = "ToggleDevTool";

    public void Load()
    {
        Text = "Dev"; // Leave empty or null for no text
    }

    protected override void OnClicked()
    {
        // Handle the event
    }
}
```

## Custom tool group button

![Multi-layer](https://files.catbox.moe/lh37x2.jpg)

You may find yourself needing multi-level tool groups. `ModdableToolGroupButton` makes it easy to create them. You should start at the root with `CustomRootToolGroupElement` like [`DevToolGroupElement`](../../ModdableToolGroupsDemo/UI/DevToolGroupElement.cs) or [`PlantingGroupElement`](../../ModdableToolGroupsDemo/UI/PlantingGroupElement.cs):

Single layer with simple tool buttons:
```cs
public class DevToolGroupElement(
    BotGeneratorTool botGeneratorTool,
    BeaverGeneratorTool beaverGeneratorTool,
    WaterHeightBrushTool waterHeightBrushTool,

    ToolGroupService toolGroupService,
    ModdableToolGroupButtonFactory buttonFac
) : CustomRootToolGroupElement(toolGroupService, buttonFac)
{
    public override string Id { get; } = "LV.DevTools";

    protected override void AddChildren(ModdableToolGroupButton btn)
    {
        btn.AddChildTool(botGeneratorTool, BotGeneratorButton.ToolImageKey);
        btn.AddChildTool(beaverGeneratorTool, BeaverGeneratorButton.ToolImageKey);
        btn.AddChildTool(waterHeightBrushTool, WaterHeightBrushButton.ToolImageKey);
    }
}
```

Multi-layer with sub groups and custom tool buttons:
```cs
    protected override void AddChildren(ModdableToolGroupButton btn)
    {
        CreateButtonFor(btn, toolGroupService.GetGroup(FieldsButton.ToolGroupId), Fields);
        CreateButtonFor(btn, toolGroupService.GetGroup(ForestryButton.ToolGroupId), Forestry);
    }

    void CreateButtonFor(ModdableToolGroupButton parent, ToolGroupSpec spec, ImmutableArray<PlantableSpec> plantables)
    {
        var btn = parent.AddChildGroup(spec);
        var btns = btn.ToolButtonsElement;

        foreach (var plantable in plantables)
        {
            var tool = plantingToolButtonFactory.CreatePlantingTool(plantable, btns);
            btn.AddChildTool(tool);
        }
    }
```