using System.Reflection.Emit;

namespace ModdableTimberborn.HaulingSystem.Patches;

/// <summary>
/// For inventories registered as extra hauler targets, stock is searched in the
/// <b>carrier/hauler's workplace district</b> (the job was only offered to those districts).
/// Unregistered receivers keep vanilla <c>receiving.District</c>.
/// </summary>
[HarmonyPatch, HarmonyPatchCategory(ExtraHaulerTargetConfig.PatchCategoryName)]
public static class CarrierInventoryFinderPatches
{
    /// <summary>
    /// Replace District with carrier
    /// </summary>
    /* IL:
     	// DistrictCenter district = receivingInventory.GetComponent<DistrictBuilding>().District;
	    IL_0007: ldarg.2
	    IL_0008: callvirt instance !!0 [Timberborn.BaseComponentSystem]Timberborn.BaseComponentSystem.BaseComponent::GetComponent<class [Timberborn.GameDistricts]Timberborn.GameDistricts.DistrictBuilding>()
	    IL_000d: callvirt instance class [Timberborn.GameDistricts]Timberborn.GameDistricts.DistrictCenter [Timberborn.GameDistricts]Timberborn.GameDistricts.DistrictBuilding::get_District()
	    IL_0012: stloc.1
     */
    [HarmonyTranspiler, HarmonyPatch(typeof(CarrierInventoryFinder), nameof(CarrierInventoryFinder.TryCarryFromAnyInventoryInternal))]
    public static IEnumerable<CodeInstruction> ReplaceDistrictFinding(IEnumerable<CodeInstruction> instructions)
    {
        var list = instructions.ToList();

        var districtAsm = list.FindIndex(i => i.opcode == OpCodes.Stloc_1);
        if (districtAsm == -1)
        {
            throw new InvalidOperationException("Could not find district assignment instruction (Stloc_1)");
        }

        var starting = list.FindLastIndex(districtAsm -1, i => i.opcode == OpCodes.Ldarg_2);
        if (starting == -1)
        {
            throw new InvalidOperationException("Could not find starting instruction (Ldarg_2)");
        }

        list.RemoveRange(starting, districtAsm - starting); // Keep Stloc_1
        list.InsertRange(starting, [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Call, typeof(CarrierInventoryFinderPatches).Method(nameof(GetCarrierDistrict)))
        ]);

        return list;
    }

    /// <summary>
    /// Stock district for fill-from-any: the hauler's workplace district
    /// (job was only offered within registered districts).
    /// </summary>
    static DistrictCenter? GetCarrierDistrict(CarrierInventoryFinder carrier)
    {
        var worker = carrier.GetComponent<Worker>();
        if (!worker || !worker.Workplace)
        {
            return null;
        }

        return worker.Workplace.GetComponent<DistrictBuilding>()?.District;
    }
}

