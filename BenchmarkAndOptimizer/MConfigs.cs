namespace BenchmarkAndOptimizer;

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModUtils.PrintLog = File.Exists(Path.Combine(modEnvironment.ModPath, "Resources/printlog"));
        new Harmony(nameof(BenchmarkAndOptimizer)).PatchAll();
    }

}
