namespace ModdableTimberborn;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance.ConfigureStarter();
    }
}
