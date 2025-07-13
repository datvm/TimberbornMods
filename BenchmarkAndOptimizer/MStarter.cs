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

        if (OptimizerSettingController.EnableBenchmark)
        {
            harmony.PatchCategory(BenchmarkCategory);
            BenchmarkPatched = true;
        }
        else
        {
            harmony.PatchCategory(OptimizeCategory);
        }
    }

    public static void SwitchBenchmarkOn()
    {
        BenchmarkPatched = true;
    }

}
