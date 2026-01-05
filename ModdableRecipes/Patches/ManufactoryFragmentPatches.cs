using System.Reflection.Emit;

namespace ModdableRecipes.Patches;

[HarmonyPatch(typeof(ManufactoryFragment))]
public static class ManufactoryFragmentPatches
{
    static WeakReference<ModdableManufactoryDropdownProvider>? moddableProvider;

    [HarmonyTranspiler, HarmonyPatch(nameof(ManufactoryFragment.InitializeFragment))]
    public static IEnumerable<CodeInstruction> RemoveTooltipRegistrar(IEnumerable<CodeInstruction> instructions)
    {
        var target = typeof(ManufactoryFragment).Field(nameof(ManufactoryFragment._tooltipRegistrar));

        var list = instructions.ToList();
        var registrarIndex = list.FindIndex(q => q.opcode == OpCodes.Ldfld && q.operand == (object)target) - 1;
        if (registrarIndex < 0)
        {
            throw new InvalidOperationException("Could not find tooltip registrar load.");
        }

        var callIndex = list.FindIndex(registrarIndex + 2, q => q.opcode == OpCodes.Callvirt || q.opcode == OpCodes.Call);
        if (callIndex < 0)
        {
            throw new InvalidOperationException("Could not find tooltip registrar call.");
        }

        list.RemoveRange(registrarIndex, callIndex - registrarIndex + 1);
        return list;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ManufactoryFragment.InitializeFragment))]
    public static void AddOwnTooltipRegistrar(ManufactoryFragment __instance)
    {
        __instance._tooltipRegistrar.Register(__instance._dropdown, () =>
        {
            if (moddableProvider?.TryGetTarget(out var p) != true) { return null; }
            return p.FormatDisplayText(p.GetValue());
        });
    }

    [HarmonyTranspiler, HarmonyPatch(nameof(ManufactoryFragment.ShowFragment))]
    public static IEnumerable<CodeInstruction> ReplaceDropdownProvider(IEnumerable<CodeInstruction> instructions)
    {
        var target = typeof(ManufactoryFragment).Field(nameof(ManufactoryFragment._dropdownItemsSetter));
        var list = instructions.ToList();
        
        var setterIndex = list.FindIndex(q => q.opcode == OpCodes.Ldfld && q.operand == (object)target) - 1;
        if (setterIndex < 0)
        {
            throw new InvalidOperationException("Could not find dropdown provider load.");
        }

        var callIndex = list.FindIndex(setterIndex + 2, q => q.opcode == OpCodes.Callvirt || q.opcode == OpCodes.Call);
        if (callIndex < 0)
        {
            throw new InvalidOperationException("Could not find dropdown provider call.");
        }
        list.RemoveRange(setterIndex, callIndex - setterIndex + 1);

        list.InsertRange(setterIndex,
        [
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, typeof(ManufactoryFragment).Field(nameof(ManufactoryFragment._manufactory))),
            new CodeInstruction(OpCodes.Call, typeof(ManufactoryFragmentPatches).Method(nameof(SetProvider))),
        ]);

        return list;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ManufactoryFragment.ClearFragment))]
    public static void Cleanup()
    {
        moddableProvider = null;
    }

    static void SetProvider(ManufactoryFragment fragment, Manufactory manufactory)
    {
        var p = manufactory.GetComponent<ModdableManufactoryDropdownProvider>();
        moddableProvider = new(p);

        p.UpdateItems();
        fragment._dropdownItemsSetter.SetItems(fragment._dropdown, p);
    }

}
