namespace ConfigurableToolGroups.UI.CustomBlockObjectProviders;

public abstract class CustomBlockObjectButtons(
    ModdableToolGroupButtonFactory grpButtonFac,
    BlockObjectToolButtonFactory boBtnFac
) : CustomBottomBarElement
{

    protected abstract IEnumerable<BlockObjectToolGroupInfo> GetRootGroups();

    public override IEnumerable<BottomBarElement> GetElements()
    {
        var root = GetRootGroups();

        foreach (var grp in root)
        {
            var btn = CreateToolGroup(grp, null);
            if (btn is not null)
            {
                yield return btn.ToBottomBarElement();
            }
        }
    }

    ModdableToolGroupButton? CreateToolGroup(BlockObjectToolGroupInfo grp, ModdableToolGroupButton? parent)
    {
        if (grp.Empty) { return null; }

        var btn = grpButtonFac.Create(grp.Spec.ToToolGroupSpec(), parent, ToolButtonColor.Green);
        parent?.AddChildGroup(btn);
        var btns = btn.ToolButtonsElement;

        foreach (var child in grp.OrderedChildren)
        {
            switch (child)
            {
                case BlockObjectToolGroupInfo subGrp:
                    CreateToolGroup(subGrp, btn);
                    break;
                case PlaceableToolInfo pti:
                    var bo = pti.Placeable;
                    if (!bo.UsableWithCurrentFeatureToggles) { continue; }

                    var toolBtn = boBtnFac.Create(bo, btns);
                    btn.AddChildTool(toolBtn);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown child type {child.GetType().FullName} in tool group {grp.Spec.Id}.");
            }
        }

        return btn;
    }

}