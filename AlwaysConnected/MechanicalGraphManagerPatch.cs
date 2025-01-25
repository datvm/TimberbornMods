using HarmonyLib;
using Timberborn.MechanicalSystem;

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

}
