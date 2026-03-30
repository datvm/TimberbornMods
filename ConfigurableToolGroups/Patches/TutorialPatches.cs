using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ConfigurableToolGroups.Patches;

[HarmonyPatch]
public static class TutorialPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BuildingTutorialStepDeserializer), nameof(BuildingTutorialStepDeserializer.GetToolButtons))]
    public static bool RedirectGTB(IEnumerable<string> templateNames, BuildingTutorialStepDeserializer __instance, ref IEnumerable<ToolButton> __result)
        => RedirectGTB(templateNames, __instance._toolButtonService, ref __result);

    [HarmonyPrefix, HarmonyPatch(typeof(ConnectBuildingsTutorialStepDeserializer), nameof(ConnectBuildingsTutorialStepDeserializer.GetToolButtons))]
    public static bool RedirectGTB(ImmutableArray<string> templateNames, ConnectBuildingsTutorialStepDeserializer __instance, ref IEnumerable<ToolButton> __result)
        => RedirectGTB(templateNames, __instance._toolButtonService, ref __result);

    [HarmonyPrefix, HarmonyPatch(typeof(PowerBuildingsTutorialStepDeserializer), nameof(PowerBuildingsTutorialStepDeserializer.GetToolButtons))]
    public static bool RedirectGTB(string[] templateNames, PowerBuildingsTutorialStepDeserializer __instance, ref IEnumerable<ToolButton> __result)
        => RedirectGTB(templateNames, __instance._toolButtonService, ref __result);

    const string TargettingMethod = nameof(ToolButtonService.GetToolButton);
    [HarmonyTranspiler, HarmonyPatch(typeof(MarkTreesTutorialStepDeserializer), nameof(MarkTreesTutorialStepDeserializer.Create))]
    public static IEnumerable<CodeInstruction> RedirectGTB(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var ins in instructions)
        {
            if (ins.opcode == OpCodes.Callvirt && ins.operand is MethodInfo mi && mi.Name == TargettingMethod)
            {
                var genericType = mi.GetGenericArguments().First();

                yield return new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(typeof(TutorialPatches), nameof(GetToolButtonByType))
                    .MakeGenericMethod(genericType)
                );
            }
            else
            {
                yield return ins;
            }
        }
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(MarkPlantablesTutorialStepDeserializer), nameof(MarkPlantablesTutorialStepDeserializer.Create))]
    public static IEnumerable<CodeInstruction> TranspileMarkPlantable(IEnumerable<CodeInstruction> instructions) 
        => TranspileGetToolButton(instructions);

    [HarmonyTranspiler, HarmonyPatch(typeof(UnlockBuildingTutorialStepDeserializer), nameof(UnlockBuildingTutorialStepDeserializer.Create))]
    public static IEnumerable<CodeInstruction> TranspileUnlockBuilding(IEnumerable<CodeInstruction> instructions)
        => TranspileGetToolButton(instructions);

    static IEnumerable<CodeInstruction> TranspileGetToolButton(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var ins in instructions)
        {
            if (ins.opcode == OpCodes.Callvirt && ins.operand is MethodInfo mi && mi.Name == TargettingMethod)
            {
                var genericType = mi.GetGenericArguments().First();
                yield return new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(typeof(TutorialPatches), nameof(GetToolButtonByTypeAndPredicate))
                    .MakeGenericMethod(genericType)
                );
            }
            else
            {
                yield return ins;
            }
        }
    }

    static bool RedirectGTB(IEnumerable<string> templateNames, ToolButtonService tbs, ref IEnumerable<ToolButton> __result)
    {
        __result = GetToolButtonsByTemplateNames(tbs, templateNames);
        return false;
    }

    public static IEnumerable<ToolButton> GetToolButtonsByTemplateNames(ToolButtonService toolButtonService, IEnumerable<string> templateNames)
    {
        foreach (var n in templateNames)
        {
            yield return GetToolButtonByTemplateName(toolButtonService, n);
        }
    }

    public static ToolButton GetToolButtonByTemplateName(ToolButtonService toolButtonService, string templateName)
        => GetToolButtonByTypeAndPredicate<BlockObjectTool>(toolButtonService, t => t.Template.GetSpec<TemplateSpec>().IsNamedExactly(templateName));

    public static ToolButton GetToolButtonByType<T>(ToolButtonService toolButtonService) where T : ITool
        => GetToolButtonByTypeAndPredicate<T>(toolButtonService, _ => true);

    public static ToolButton GetToolButtonByTypeAndPredicate<T>(ToolButtonService toolButtonService, Predicate<T> predicate)
        where T : ITool
        => toolButtonService.ToolButtons.First(tb => tb.Tool is T t && predicate(t));

}
