using System.Reflection.Emit;

namespace TImprove4UX.Patches;

[HarmonyPatch(typeof(EntityPanel))]
public static class EntityPanelPatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(EntityPanel.AddFragments))]
    public static IEnumerable<CodeInstruction> RecordFragmentType(IEnumerable<CodeInstruction> instructions)
    {
        var initializeMethod = typeof(IEntityPanelFragment).Method(nameof(IEntityPanelFragment.InitializeFragment));
        var found = false;

        foreach (var i in instructions)
        {
            yield return i;

            if (i.opcode == OpCodes.Callvirt && (object)i.operand == initializeMethod)
            {
                found = true;

                yield return new(OpCodes.Dup);
                yield return new(OpCodes.Ldloc_1);
                yield return new(OpCodes.Call, typeof(EntityPanelPatches).Method(nameof(InterceptParentAddCall)));
            }
        }

        if (!found)
        {
            throw new InvalidOperationException("Failed to find call to IEntityPanelFragment.InitializeFragment in EntityPanel.AddFragments");
        }
    }

    static void InterceptParentAddCall(VisualElement ve, IEntityPanelFragment fragment) 
        => CollapsibleEntityPanelService.Instance?.RegisterPanelFragment(ve, fragment);

}
