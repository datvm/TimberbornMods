
namespace BenchmarkAndOptimizer;

public class MStarter : IModStarter
{
    public const string BenchmarkCategory = "Benchmark";
    public const string OptimizeCategory = "Optimize";

    static Harmony harmony = null!;
    public static bool BenchmarkPatched { get; private set; } = false;

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        harmony = new Harmony(nameof(BenchmarkAndOptimizer));

        if (OptimizerSettings.EnableBenchmark)
        {
            harmony.PatchCategory(BenchmarkCategory);
            BenchmarkPatched = true;
        }

        harmony.PatchCategory(OptimizeCategory);
        PatchUpdatableComponents(harmony);
    }

    static void PatchUpdatableComponents(Harmony harmony)
    {
        var prefix = typeof(GameplayPatches).Method(nameof(GameplayPatches.UpdateComponentPrefix));
        var postfix = typeof(GameplayPatches).Method(nameof(GameplayPatches.UpdateComponentPostfix));

        foreach (var t in OptimizableTypeService.UpdatableComponents)
        {
            var m = t.Method(OptimizableTypeService.UpdateMethod);
            harmony.Patch(m, prefix: prefix, postfix: postfix);
        }
    }

}
