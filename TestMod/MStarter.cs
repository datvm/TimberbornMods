namespace TestMod;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(TestMod));
        h.PatchAll();
    }


}

[HarmonyPatch]
public static class TestPatch
{

    //[HarmonyPrefix, HarmonyPatch(typeof(InputService), nameof(InputService.CallInputProcessors))]
    public static bool PrintDebug(InputService __instance)
    {


        __instance._inputProcessorsCopy.AddRange(__instance._inputProcessors);
        try
        {
            for (int i = 0; i < __instance._priorityInputProcessors.Count; i++)
            {
                __instance._priorityInputProcessors[i].ProcessInput();
            }
            int num = __instance._inputProcessorsCopy.Count - 1;

            Debug.Log("==================================");
            while (num >= 0)
            {
                var processor = __instance._inputProcessorsCopy[num];
                var processed = processor.ProcessInput();

                Debug.Log($"Processor: {processor.GetType().Name}, Processed: {processed}");

                if (processed)
                {
                    break;
                }

                num--;

            }
        }
        finally
        {
            __instance._inputProcessorsCopy.Clear();
        }

        return false;
    }
}