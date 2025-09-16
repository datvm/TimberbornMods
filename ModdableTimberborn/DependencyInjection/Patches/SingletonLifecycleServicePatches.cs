namespace ModdableTimberborn.DependencyInjection.Patches;

[HarmonyPatchCategory(DependencyInjectionRegistry.PatchCategoryName), HarmonyPatch(typeof(SingletonLifecycleService))]
public static class SingletonLifecycleServicePatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(SingletonLifecycleService.LoadSingletons))]
    public static IEnumerable<CodeInstruction> InterceptLoad(IEnumerable<CodeInstruction> instructions)
    {
        return TranspileFrontTailRunners(
            instructions,
            typeof(ILoadableSingleton).Method(nameof(ILoadableSingleton.Load)),
            typeof(SingletonLifecycleServicePatches).Method(nameof(FrontRunLoad)),
            typeof(SingletonLifecycleServicePatches).Method(nameof(TailRunLoad))
        );
    }

    [HarmonyTranspiler, HarmonyPatch(nameof(SingletonLifecycleService.PostLoadSingletons))]
    public static IEnumerable<CodeInstruction> InterceptPostLoad(IEnumerable<CodeInstruction> instructions)
    {
        return TranspileFrontTailRunners(
            instructions,
            typeof(IPostLoadableSingleton).Method(nameof(IPostLoadableSingleton.PostLoad)),
            typeof(SingletonLifecycleServicePatches).Method(nameof(FrontRunPostLoad)),
            typeof(SingletonLifecycleServicePatches).Method(nameof(TailRunPostLoad))
        );
    }

    static IEnumerable<CodeInstruction> TranspileFrontTailRunners(
        IEnumerable<CodeInstruction> instructions,
        MethodInfo expectingMethod,
        MethodInfo frontRunMethod,
        MethodInfo tailRunMethod)
    {
        var found = false;

        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Call, typeof(SingletonLifecycleServicePatches).Method(nameof(StartRun)));

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(expectingMethod))
            {
                // Duplicate the instance twice (once for each call)
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Dup);

                yield return new CodeInstruction(OpCodes.Call, frontRunMethod); // Call FrontRun
                yield return instruction; // Original call to Load                
                yield return new CodeInstruction(OpCodes.Call, tailRunMethod); // Call TailRun

                found = true;
            }
            else
            {
                yield return instruction; // Just pass through other instructions
            }
        }

        yield return found
            ? CodeInstruction.Call(() => FinishRun())
            : throw new InvalidOperationException($"Failed to find call to {expectingMethod.Name}.");
    }

    static void StartRun(SingletonLifecycleService service) => DependencyInjectionRegistry.Instance.StartRun(service);
    static void FinishRun() => DependencyInjectionRegistry.Instance.FinishRun();

    static void FrontRunLoad(ILoadableSingleton s) => DependencyInjectionRegistry.Instance.FrontRunLoad(s);
    static void TailRunLoad(ILoadableSingleton s) => DependencyInjectionRegistry.Instance.TailRunLoad(s);
    static void FrontRunPostLoad(IPostLoadableSingleton s) => DependencyInjectionRegistry.Instance.FrontRunPostLoad(s);
    static void TailRunPostLoad(IPostLoadableSingleton s) => DependencyInjectionRegistry.Instance.TailRunPostLoad(s);

}
