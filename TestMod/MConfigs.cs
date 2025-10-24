using Newtonsoft.Json.Linq;

namespace TestMod;

[Context("MainMenu")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<TestService>().AsSingleton();
    }
}

public class TestService(ISpecService specs) : ILoadableSingleton
{
    public void Load()
    {
        var spec = specs.GetSingleSpec<MyTestSpec>();
        Debug.Log(spec.Json.GetSerialized("Array"));
    }
}

public record MyTestSpec : ComponentSpec
{
    [Serialize]
    public string String { get; init; }

    [Serialize]
    public SerializedObject Json { get; init; }

}
