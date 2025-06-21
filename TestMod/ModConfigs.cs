
namespace TestMod;

[Context("Game")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<TestService>().AsSingleton();
    }
}

class TestService : ILoadableSingleton
{
    public void Load()
    {
        Debug.LogException(new Exception("Test exception"));
        Debug.Log("This should run");
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TestMod));
        harmony.PatchAll();
    }

}
