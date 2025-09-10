namespace ModdableTimberborn.Patches;

[HarmonyPatch]
public static class ContainerRetrieverPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(ContainerCreator), nameof(ContainerCreator.CreateContainer))]
    public static void ReceiveContainer(IContainer __result)
    {
        new ContainerRetriever(__result);
    }

}
