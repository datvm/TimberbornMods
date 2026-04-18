namespace ConfigurableToolGroups.UI;

public class ModdableToolGroupButton
{
    readonly ToolGroupService toolGrps;
    readonly ToolButtonFactory toolBtnFac;
    readonly ModdableToolGroupButtonFactory factory;

    public ToolGroupButton ToolGroupButton { get; }
    public VisualElement Root { get; }
    public VisualElement ToolButtonsElement => ToolGroupButton.ToolButtonsElement;
    public ToolGroupSpec Spec => ToolGroupButton._toolGroup;
    public ModdableToolGroupButtonInfo Info { get; }

    public ModdableToolGroupButton(
        ToolGroupSpec spec,
        ModdableToolGroupButton? parent,
        ToolButtonColor color,
        ToolGroupButtonFactory grpBtnFac,
        ILoc t,
        ModdableToolGroupButtonService serv,
        ToolGroupService toolGrps,
        ToolButtonFactory toolBtnFac,
        ModdableToolGroupButtonFactory factory,
        bool isRoot
    )
    {
        this.toolGrps = toolGrps;
        this.toolBtnFac = toolBtnFac;
        this.factory = factory;

        var btn = ToolGroupButton = grpBtnFac.Create(spec, color);

        // Wrap non-root buttons
        var r = Root = btn.Root;
        if (!isRoot)
        {
            var wrapper = btn._toolGroupButtonWrapper;

            wrapper.AddToClassList("bottom-bar-button--background"); // For some reason adding this does nothing

            var ws = wrapper.style;
            ws.minHeight = ws.maxHeight = 60;
            ws.paddingLeft = ws.paddingRight = 2;
            ws.paddingTop = ws.paddingBottom = 5;
            ws.backgroundImage = serv.BackgroundTexture;
        }

        // Fix tooltip
        r.Q<Label>("Tooltip").text = t.T(spec.DisplayNameLocKey);
        btn._toolGroupButtonWrapper.style.left = 0;

        // Sub elements
        var btns = btn.ToolButtonsElement;
        var s = btns.style;
        s.alignItems = Align.FlexEnd;
        s.position = Position.Absolute;
        s.bottom = Length.Percent(100);
        s.left = Length.Percent(50);
        s.translate = new Translate(Length.Percent(-50), 0);
        s.maxWidth = 1200;

        // Register
        Info = serv.GetOrAddButton(btn, parent?.Info);

        if (!toolGrps._toolGroups.ContainsKey(Spec.Id))
        {
            toolGrps.RegisterGroup(Spec);
        }
    }

    public ModdableToolGroupButton AddChildGroup(ToolGroupSpec spec, ToolButtonColor color = default)
    {
        var btn = factory.Create(spec, this, color);
        AddChildGroup(btn);

        return btn;
    }

    public void AddChildGroup(ModdableToolGroupButton groupButton)
    {
        ToolButtonsElement.Add(groupButton.Root);
    }

    public ToolButton AddChildTool(ITool tool, string toolImageName)
        => AddChildTool(tool, toolBtnFac.LoadImage(toolImageName));

    public ToolButton AddChildTool(ITool tool, Sprite toolImage)
    {
        var btn = toolBtnFac.Create(tool, toolImage, ToolButtonsElement);
        AddChildTool(btn);

        return btn;
    }

    public void AddChildTool(ToolButton toolButton)
    {
        toolGrps.AssignToGroup(Spec, toolButton.Tool);
        ToolGroupButton.AddTool(toolButton);
    }

    public BottomBarElement ToBottomBarElement() => new(Root, ToolButtonsElement);

    static VisualElement CreateSpacer()
    {
        var ve = new VisualElement();
        ve.AddToClassList("tool-group__spacer");
        return ve;
    }

}
