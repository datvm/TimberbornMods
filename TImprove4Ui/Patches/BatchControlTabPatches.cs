namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class BatchControlTabPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BatchControlTab), nameof(WorkplacesBatchControlTab.GetHeader))]
    public static bool OnGetHeader(BatchControlTab __instance, ref VisualElement? __result)
    {
        if (__instance is not WorkplacesBatchControlTab) { return true; }

        __result = WorkplacesBatchControlTabService.Instance?.CreateHeader();
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(WorkplacesBatchControlRowFactory), nameof(WorkplacesBatchControlRowFactory.Create))]
    public static void OnWorkplaceRowCreated(BatchControlRow __result)
    {
        WorkplacesBatchControlTabService.Instance?.OnBatchControlRowCreated(__result);
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(BatchControlRowGroupFactory), nameof(BatchControlRowGroupFactory.Create), [typeof(BatchControlRow), typeof(IComparer<BatchControlRow>), typeof(string)])]
    public static IEnumerable<CodeInstruction> OnCreatingRowGroup(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        var method = typeof(VisualElement).Method(nameof(VisualElement.Add), [typeof(VisualElement)]);

        foreach (var ins in instructions)
        {
            yield return ins;

            if (ins.Calls(method))
            {
                yield return new(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, typeof(BatchControlTabPatches).Method(nameof(AddCollapsiblePanel)));

                found = true;
            }
        }

        if (!found)
        {
            throw new Exception("Failed to patch WorkplacesBatchControlTabPatches.OnCreatingRowGroup");
        }
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(BatchControlRowGroup), nameof(BatchControlRowGroup.AddRow))]
    public static IEnumerable<CodeInstruction> AddToCollapsiblePanelInstead(IEnumerable<CodeInstruction> instructions)
    {
        var method = typeof(BatchControlRowGroup).PropertyGetter(nameof(BatchControlRowGroup.Root));
        var found = false;

        foreach (var ins in instructions)
        {
            yield return ins;

            if (ins.Calls(method))
            {
                // Get its Container property
                yield return new CodeInstruction(OpCodes.Call, typeof(BatchControlTabPatches).Method(nameof(GetContainer)));
                found = true;
            }
        }

        if (!found)
        {
            throw new Exception("Failed to patch WorkplacesBatchControlTabPatches.AddToCollapsiblePanelInstead");
        }
    }

    static VisualElement GetContainer(VisualElement panel) => panel is CollapsiblePanel cp ? cp.Container : panel;

    static CollapsiblePanel AddCollapsiblePanel(VisualElement ve, BatchControlRow header)
    {
        var collapsible = new CollapsiblePanel();
        collapsible.Container.Add(ve);
        collapsible.SetExpand(!MSettings.AutoCollapseManagementGroupsValue);

        var headerContainer = collapsible.HeaderLabel.parent;
        header.Root
            .SetFlexGrow()
            .InsertSelfAfter(collapsible.HeaderLabel);
        collapsible.HeaderLabel.RemoveFromHierarchy();
        header.Root.RegisterCallback<ClickEvent>(_ => collapsible.ToggleExpand());

        return collapsible;
    }
}
