namespace QuickBar.Services.QuickBarItems;

public abstract class ToolQuickBarItem : IQuickBarItem
{
    public string Title { get; protected set; } = "";
    public Sprite? Sprite { get; protected set; }
    public Texture2D? Texture { get; }

#nullable disable
    public ToolButton ToolButton { get; protected set; }
    public string ToolType { get; protected set; }
#nullable enable

    protected abstract string GetTitle(ILoc t);
    public virtual void SerializeValues(Dictionary<string, string> values) { }

    public abstract void Init(ILoc t, ToolButton toolButton);

    public void Activate()
    {
        ToolButton.Select();
    }

    public abstract ToolButton FindToolButton(IEnumerable<ToolButton> toolButtons, Dictionary<string, string> values);

    public bool IsStillValid() => true;
}

public abstract class ToolQuickBarItem<T> : ToolQuickBarItem
    where T : Tool
{
    public T Tool { get; private set; } = null!;

    protected string GetDefaultTitle()
    {
        if (Tool.DevModeTool)
        {
            return Tool.GetType().Name;
        }

        var desc = Tool.Description();
        var title = desc?.Title;

        return title ?? Tool.GetType().Name;
    }

    public override void Init(ILoc t, ToolButton toolButton)
    {
        ToolButton = toolButton;
        Tool = (T)toolButton.Tool;
        ToolType = Tool.GetType().FullName;
        Sprite = toolButton.Root.Q("ToolImage")?.style.backgroundImage.value.sprite;

        Title = GetTitle(t);
    }

    public static string? GetLabeledTitle(BaseComponent comp, ILoc t)
    {
        var label = comp.GetComponentFast<LabeledEntitySpec>();
        if (!label) { return null; }

        return label.DisplayNameLocKey.T(t);
    }

    public override ToolButton FindToolButton(IEnumerable<ToolButton> toolButtons, Dictionary<string, string> values)
    {
        var filterByType = toolButtons.Where(q => q.Tool is T).ToArray();

        if (filterByType.Length == 0)
        {
            throw new InvalidDataException($"Cannot find any {typeof(T)} tool");
        }
        else if (filterByType.Length == 1)
        {
            return filterByType[0];
        }
        else
        {
            return FindSingleTool(filterByType, values);
        }
    }

    protected virtual ToolButton FindSingleTool(IEnumerable<ToolButton> toolButtons, Dictionary<string, string> values)
    {
        throw new InvalidOperationException($"Cannot find a single {typeof(T)} tool from the list.");
    }

}

public class CursorToolQuickBarItem : ToolQuickBarItem<CursorTool>
{
    protected override string GetTitle(ILoc t) => t.T("LV.OB.Cursor");
}

public class PlantingToolQuickBarItem : ToolQuickBarItem<PlantingTool>
{
    public PlantableSpec PlantableSpec => Tool.PlantableSpec;

    protected override string GetTitle(ILoc t) => GetLabeledTitle(Tool.PlantableSpec, t) ?? GetDefaultTitle();

    public override void SerializeValues(Dictionary<string, string> values)
    {
        values.Add(nameof(PlantableSpec), PlantableSpec.PrefabName);
    }

    protected override ToolButton FindSingleTool(IEnumerable<ToolButton> toolButtons, Dictionary<string, string> values)
    {
        var value = values[nameof(PlantableSpec)];
        var tool = toolButtons.FirstOrDefault(q => ((PlantingTool)q.Tool).PlantableSpec.PrefabName == value);

        return tool ?? throw new Exception($"Cannot find PlantingTool with PlantableSpec '{value}'");
    }
}

public class BuilderPriorityToolQuickBarItem : ToolQuickBarItem<BuilderPriorityTool>
{
    public Priority Priority => Tool._priority;

    protected override string GetTitle(ILoc t) => $"{t.T("ToolGroups.Priority")}: {Tool.Description().Title}";

    public override void SerializeValues(Dictionary<string, string> values)
    {
        values.Add(nameof(Priority), Priority.ToString());
    }

    protected override ToolButton FindSingleTool(IEnumerable<ToolButton> toolButtons, Dictionary<string, string> values)
    {
        var value = values[nameof(Priority)];

        var tool = toolButtons.FirstOrDefault(q => ((BuilderPriorityTool)q.Tool)._priority.ToString() == value);
        return tool ?? throw new Exception($"Cannot find BuilderPriorityTool with Priority '{value}'");
    }
}

public class BlockObjectToolQuickBarItem : ToolQuickBarItem<BlockObjectTool>
{
    public PrefabSpec PrefabSpec { get; private set; } = null!;

    public override void Init(ILoc t, ToolButton toolButton)
    {
        PrefabSpec = ((BlockObjectTool)toolButton.Tool).Prefab.GetComponentFast<PrefabSpec>();

        base.Init(t, toolButton);
    }

    protected override string GetTitle(ILoc t) => GetLabeledTitle(PrefabSpec, t) ?? GetDefaultTitle();

    public override void SerializeValues(Dictionary<string, string> values)
    {
        values.Add(nameof(PrefabSpec), PrefabSpec.PrefabName);
    }

    protected override ToolButton FindSingleTool(IEnumerable<ToolButton> toolButtons, Dictionary<string, string> values)
    {
        var value = values[nameof(PrefabSpec)];
        var tool = toolButtons.FirstOrDefault(q => ((BlockObjectTool)q.Tool).Prefab.GetComponentFast<PrefabSpec>().PrefabName == value);
        return tool ?? throw new Exception($"Cannot find BlockObjectTool with PrefabSpec '{value}'");
    }

}

public class GenericToolQuickBarItem : ToolQuickBarItem<Tool>
{
    protected override string GetTitle(ILoc t) => GetDefaultTitle();

    public override void SerializeValues(Dictionary<string, string> values)
    {
        values.Add("ToolType", ToolType);
    }

    protected override ToolButton FindSingleTool(IEnumerable<ToolButton> toolButtons, Dictionary<string, string> values)
    {
        var toolType = values["ToolType"];
        return toolButtons.Single(q => q.Tool.GetType().FullName == toolType);
    }
}