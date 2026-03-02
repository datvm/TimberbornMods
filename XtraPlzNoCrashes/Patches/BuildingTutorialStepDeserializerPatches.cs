namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch(typeof(BuildingTutorialStepDeserializer))]
public static class BuildingTutorialStepDeserializerPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(BuildingTutorialStepDeserializer.GetToolButtons))]
    public static bool GetAllToolButtons(ToolButtonService ____toolButtonService, IEnumerable<string> templateNames, ref IEnumerable<ToolButton> __result)
    {
        IEnumerable<ToolButton> Inner()
        {
            foreach (string templateName in templateNames)
            {
                foreach (var toolButton in ____toolButtonService.ToolButtons) {
                    if (toolButton.Tool is BlockObjectTool obj && obj.Template.GetSpec<TemplateSpec>().IsNamedExactly(templateName)) {
                        yield return toolButton;
                    }
                }
            }
        }
        __result = Inner();
        return false;
    }

}
