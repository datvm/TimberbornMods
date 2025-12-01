namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(ToolGroupButton))]
public static class ButtonsPatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupButton.IsVisible), MethodType.Getter)]
    public static bool UpdateVisibility(ToolGroupButton __instance, ref bool __result)
    {
        var info = ModdableToolGroupButtonService.Instance[__instance];
        if (info is null) { return true; }

        foreach (var grp in info.Children)
        {
            if (grp.IsVisible)
            {
                __result = true;
                return false;
            }
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupButton.OnToolGroupEntered))]
    public static void ActivateParents(ToolGroupButton __instance, ToolGroupEnteredEvent toolGroupOpenedEvent)
    {
        if (toolGroupOpenedEvent.ToolGroup != __instance._toolGroup) { return; }

        var level = 0;

        var parent = ModdableToolGroupButtonService.Instance[__instance]?.Parent;
        if (parent is null) { goto SET_TOP_BEFORE_RETURN; }

        while (parent is not null)
        {
            level++;
            var btn = parent.Button;

            btn.ToolButtonsElement.ToggleDisplayStyle(true);
            btn._toolGroupButtonWrapper.AddToClassList(ToolGroupButton.ActiveClassName);

            parent = parent.Parent;
        }

    SET_TOP_BEFORE_RETURN:
        ToolPanelPositioningService.Instance?.SetTop(level);
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupButton.OnToolGroupExited))]
    public static void DeactivateParents(ToolGroupButton __instance, ToolGroupExitedEvent toolGroupExitedEvent)
    {
        if (toolGroupExitedEvent.ToolGroup != __instance._toolGroup) { return; }

        var parent = ModdableToolGroupButtonService.Instance[__instance]?.Parent;
        if (parent is null) { return; }

        while (parent is not null)
        {
            var btn = parent.Button;

            btn.ToolButtonsElement.ToggleDisplayStyle(false);
            btn._toolGroupButtonWrapper.RemoveFromClassList(ToolGroupButton.ActiveClassName);

            parent = parent.Parent;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameBlockObjectButtons), nameof(GameBlockObjectButtons.CreateRegularBlockObjectToolGroups))]
    public static bool CreateRegularBlockObjectToolGroups(GameBlockObjectButtons __instance, ref IEnumerable<BottomBarElement> __result)
    {
        __result = ModdableToolGroupSpecService.Instance.RootToolGroup.ChildrenGroups
            .Select(grp => CreateGroupButton(grp, __instance, null))
            .Where(q => q is not null)
            .Select(q => BottomBarElement.CreateMultiLevel(q!.Root, q.ToolButtonsElement));

        return false;
    }

    static ToolGroupButton? CreateGroupButton(ToolGroupInfo grp, GameBlockObjectButtons buttons, ToolGroupButtonInfo? parent)
    {
        if (grp.Empty) { return null; }

        var grpFac = buttons._blockObjectToolGroupButtonFactory;
        var btnFac = grpFac._toolGroupButtonFactory;
        var boBtnFac = grpFac._blockObjectToolButtonFactory;

        var spec = BlockObjectToolGroupButtonFactory.CreateBlueprint(grp.Spec).GetSpec<ToolGroupSpec>();
        var btn = btnFac.CreateGreen(spec);
        var info = ModdableToolGroupButtonService.Instance.AddButton(btn, parent);

        var tooltipWrapperS = btn._toolGroupButtonWrapper.style;
        tooltipWrapperS.left = 0;

        var subEl = btn.ToolButtonsElement;
        var subElS = subEl.style;
        subElS.alignItems = Align.FlexEnd;
        subElS.position = Position.Absolute;
        subElS.bottom = new Length(100, LengthUnit.Percent);

        foreach (var child in grp.orderedChildren)
        {
            switch (child)
            {
                case ToolGroupInfo subGrp:
                    var subGrpBtn = CreateGroupButton(subGrp, buttons, info);
                    if (subGrpBtn is not null)
                    {
                        subEl.Add(subGrpBtn.Root);
                        info.Children.Add(subGrpBtn);
                    }
                    break;
                case PlaceableToolInfo pti:
                    var bo = pti.Placeable;
                    if (!bo.UsableWithCurrentFeatureToggles) { continue; }

                    var toolBtn = boBtnFac.Create(bo, subEl);
                    grpFac._toolGroupService.AssignToGroup(spec, toolBtn.Tool);
                    btn.AddTool(toolBtn);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown child type {child.GetType().FullName} in tool group {grp.Spec.Id}.");
            }
        }

        btnFac._toolGroupService.RegisterGroup(spec);

        return btn;
    }

}
