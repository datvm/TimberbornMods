
namespace QuickBar.Services.QuickBarItems;

public class ToolQuickBarItemProvider(
    ILoc t,
    ToolButtonService toolButtonService
) : IQuickBarItemProvider
{
    public ImmutableHashSet<Type> SupportedType { get; } = [
        typeof(CursorToolQuickBarItem),
        typeof(PlantingToolQuickBarItem),
        typeof(BuilderPriorityToolQuickBarItem),
        typeof(BlockObjectToolQuickBarItem),
        typeof(GenericToolQuickBarItem),
    ];

    public bool TryCreateItem(IOmnibarItem omnibarItem, [NotNullWhen(true)] out IQuickBarItem? quickbarItem)
    {
        quickbarItem = default;
        if (omnibarItem is not OmnibarToolItem toolItem) { return false; }

        ToolQuickBarItem? item = null;

        var toolButton = toolItem.ToolButton;
        var tool = toolButton.Tool;

        switch (tool)
        {
            case CursorTool:
                item = new CursorToolQuickBarItem();
                break;
            case PlantingTool:
                item = new PlantingToolQuickBarItem();
                break;
            case BuilderPriorityTool:
                item = new BuilderPriorityToolQuickBarItem();
                break;
            case BlockObjectTool:
                item = new BlockObjectToolQuickBarItem();
                break;
            default:
                if (tool.GetType().Namespace.StartsWith("Timberborn."))
                {
                    item = new GenericToolQuickBarItem();
                }

                break;
        }

        if (item is not null)
        {
            item.Init(t, toolItem.ToolButton);
            quickbarItem = item;
            return true;
        }

        return false;
    }

    public IQuickBarItem? Deserialize(string data)
    {
        try
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            if (values is null || !values.TryGetValue("Type", out var typeName)) { return null; }

            var type = Type.GetType(typeName);
            if (type is null || !typeof(ToolQuickBarItem).IsAssignableFrom(type))
            {
                throw new NotSupportedException();
            }

            var item = (ToolQuickBarItem)Activator.CreateInstance(type)!;

            var toolButton = item.FindToolButton(toolButtonService.ToolButtons, values);
            item.Init(t, toolButton);

            return item;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to deserialize from data: {data}");
            Debug.LogException(ex);

            return null;
        }
    }

    public string? Serialize(IQuickBarItem item)
    {
        if (item is not ToolQuickBarItem toolItem)
        {
            throw new NotSupportedException();
        }

        Dictionary<string, string> values = [];
        values["Type"] = toolItem.GetType().FullName;
        toolItem.SerializeValues(values);

        return JsonConvert.SerializeObject(values);
    }

}
