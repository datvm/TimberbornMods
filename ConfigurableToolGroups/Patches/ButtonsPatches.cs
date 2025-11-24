
namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(ToolGroupButton))]
public static class ButtonsPatches
{
    public static readonly ConditionalWeakTable<ToolGroupButton, ToolGroupButtonInfo> buttonsInfo = [];

    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupButton.IsVisible), MethodType.Getter)]
    public static bool UpdateVisibility(ToolGroupButton __instance, ref bool __result)
    {
        if (!buttonsInfo.TryGetValue(__instance, out var info)) { return true; }

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
        if (toolGroupOpenedEvent.ToolGroup != __instance._toolGroup 
            || !buttonsInfo.TryGetValue(__instance, out var info)) { return; }

        var parent = info.Parent;
        while (parent is not null)
        {
            var btn = parent.Button;

            btn.ToolButtonsElement.ToggleDisplayStyle(true);
            btn._toolGroupButtonWrapper.AddToClassList(ToolGroupButton.ActiveClassName);

            parent = parent.Parent;
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupButton.OnToolGroupExited))]
    public static void DeactivateParents(ToolGroupButton __instance, ToolGroupExitedEvent toolGroupExitedEvent)
    {
        if (toolGroupExitedEvent.ToolGroup != __instance._toolGroup
            || !buttonsInfo.TryGetValue(__instance, out var info)) { return; }

        var parent = info.Parent;
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
        var service = ContainerRetriever.GetInstance<ModdableToolGroupSpecService>();

        __result = service.RootToolGroup.ChildrenGroups
            .Select(grp =>
            {
                return CreateGroup(grp, __instance, null);
            })
            .Where(q => q is not null)
            .Select(q => BottomBarElement.CreateMultiLevel(q!.Root, q.ToolButtonsElement));
        return false;
    }

    static ToolGroupButton? CreateGroup(ToolGroupDetails grp, GameBlockObjectButtons buttons, ToolGroupButtonInfo? parent)
    {
        if (grp.Empty) { return null; }

        var grpFac = buttons._blockObjectToolGroupButtonFactory;
        var btnFac = grpFac._toolGroupButtonFactory;

        var spec = BlockObjectToolGroupButtonFactory.CreateBlueprint(grp.Spec).GetSpec<ToolGroupSpec>();
        var btn = btnFac.CreateGreen(spec);

        var info = new ToolGroupButtonInfo(btn, parent);
        buttonsInfo.Add(btn, info);

        if (grp.childrenGroups.Count > 0)
        {
            foreach (var subGrp in grp.childrenGroups)
            {
                var subGrpBtn = CreateGroup(subGrp, buttons, info);
                if (subGrpBtn is not null)
                {
                    btn.ToolButtonsElement.Add(subGrpBtn.Root);
                    info.Children.Add(subGrpBtn);
                }
            }
        }

        foreach (var (bo, _) in grp.ChildrenTools)
        {
            if (!bo.UsableWithCurrentFeatureToggles) { continue; }

            var toolBtn = grpFac._blockObjectToolButtonFactory.Create(bo, btn.ToolButtonsElement);
            grpFac._toolGroupService.AssignToGroup(spec, toolBtn.Tool);
            btn.AddTool(toolBtn);
        }

        btnFac._toolGroupService.RegisterGroup(spec);
        return btn;
    }

}
