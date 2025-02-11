using Timberborn.MechanicalSystem;
using Timberborn.MechanicalSystemUI;

namespace AlwaysConnected;

[HarmonyPatch(typeof(MechanicalGraphManager))]
public static class MechanicalGraphManagerPatch
{
    static readonly FieldInfo mechanicalGraphRegistryField = typeof(MechanicalGraphFactory).GetField("_mechanicalGraphRegistry", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPatch(nameof(MechanicalGraphManager.AddNode)), HarmonyPostfix]
    public static void PostfixAddNode(MechanicalGraphFactory ____mechanicalGraphFactory)
    {
        Patch(____mechanicalGraphFactory);
    }

    [HarmonyPatch(nameof(MechanicalGraphManager.RemoveNode)), HarmonyPostfix]
    public static void PostfixRemoveNode(MechanicalGraphFactory ____mechanicalGraphFactory)
    {
        Patch(____mechanicalGraphFactory);
    }

    static void Patch(MechanicalGraphFactory ____mechanicalGraphFactory)
    {
        var registry = (MechanicalGraphRegistry)mechanicalGraphRegistryField.GetValue(____mechanicalGraphFactory);

        ____mechanicalGraphFactory.Join([.. registry.MechanicalGraphs]);
    }

    // Fixing Forester crashing
    [HarmonyPrefix, HarmonyPatch(typeof(MechanicalModel), nameof(MechanicalModel.UpdateModel))]
    public static bool PrefixUpdateModel(MechanicalModel __instance)
    {
        if (__instance._mechanicalNode?.Transputs.Any() != true)
        {
            Debug.Log("Skipping UpdateModel to prevent crash");
            return false;
        }

        return true;
    }

}
