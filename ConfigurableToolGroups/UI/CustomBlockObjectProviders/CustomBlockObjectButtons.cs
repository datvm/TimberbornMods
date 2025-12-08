namespace ConfigurableToolGroups.UI.CustomBlockObjectProviders;

public abstract class CustomBlockObjectButtons(
    BlockObjectToolButtonFactory boBtnFac,
    ToolGroupButtonFactory grpFac,
    ILoc t,
    ModdableToolGroupButtonService moddableToolGroupButtonService,
    ToolGroupService toolGroupService
) : CustomBottomBarElement
{

    protected abstract IEnumerable<ToolGroupInfo> GetRootGroups();

    public override IEnumerable<BottomBarElement> GetElements()
    {
        var root = GetRootGroups();

        foreach (var grp in root)
        {
            var btn = CreateToolGroup(grp, null);
            if (btn is not null)
            {
                yield return BottomBarElement.CreateMultiLevel(btn.Root, btn.ToolButtonsElement);
            }
        }
    }

    ToolGroupButton? CreateToolGroup(ToolGroupInfo grp, ToolGroupButtonInfo? parent)
    {
        if (grp.Empty) { return null; }

        var spec = BlockObjectToolGroupButtonFactory.CreateBlueprint(grp.Spec).GetSpec<ToolGroupSpec>();
        var (btn, btns) = CreateToolGroupButtonElement(spec);
        var info = moddableToolGroupButtonService.AddButton(btn, parent);

        foreach (var child in grp.OrderedChildren)
        {
            switch (child)
            {
                case ToolGroupInfo subGrp:
                    var subGrpBtn = CreateToolGroup(subGrp, info);
                    if (subGrpBtn is not null)
                    {
                        btns.Add(subGrpBtn.Root);
                        info.Children.Add(subGrpBtn);
                    }
                    break;
                case PlaceableToolInfo pti:
                    var bo = pti.Placeable;
                    if (!bo.UsableWithCurrentFeatureToggles) { continue; }

                    var toolBtn = boBtnFac.Create(bo, btns);
                    grpFac._toolGroupService.AssignToGroup(spec, toolBtn.Tool);
                    btn.AddTool(toolBtn);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown child type {child.GetType().FullName} in tool group {grp.Spec.Id}.");
            }
        }

        if (!toolGroupService._toolGroups.ContainsKey(spec.Id))
        {
            toolGroupService.RegisterGroup(spec);
        }

        return btn;
    }

    (ToolGroupButton, VisualElement) CreateToolGroupButtonElement(ToolGroupSpec spec)
    {
        var btn = grpFac.CreateGreen(spec);

        // Tooltip
        var root = btn.Root;
        root.Q<Label>("Tooltip").text = t.T(spec.DisplayNameLocKey);
        btn._toolGroupButtonWrapper.style.left = 0;

        // Subelements
        var btns = btn.ToolButtonsElement;
        var s = btns.style;
        s.alignItems = Align.FlexEnd;
        s.position = Position.Absolute;
        s.bottom = Length.Percent(100);

        return (btn, btns);
    }

}