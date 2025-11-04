namespace ConfigurableTubeZipLine.Patches;

[HarmonyPatch]
public static class ZiplineObstaclePatches
{

    [HarmonyTranspiler, HarmonyPatch(typeof(ZiplineConnectionBlockFactory), nameof(ZiplineConnectionBlockFactory.Load))]
    public static IEnumerable<CodeInstruction> ZiplineThroughObstaclesTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        var setter = typeof(ZiplineConnectionBlockFactory).PropertySetter(nameof(ZiplineConnectionBlockFactory.ZiplineConnectionBlock));

        foreach (var ins in instructions)
        {
            if (ins.Calls(setter))
            {
                found = true;
                yield return CodeInstruction.Call(() => ModifySpec);
            }

            yield return ins;
        }

        if (!found)
        {
            throw new InvalidOperationException("Failed to apply ZiplineThroughObstaclesTranspiler: setter not found.");
        }
    }

    static BlockObjectSpec ModifySpec(BlockObjectSpec spec)
    {
        if (!MSettings.ZiplineThroughObstacles) { return spec; }

        return spec with
        {
            BlocksSpec = spec.BlocksSpec with
            {
                BlockSpecs = [.. spec.BlocksSpec.BlockSpecs.Select(bs => bs with
                {
                    Occupations = BlockOccupations.None,
                })],
            },
        };
    }

}
