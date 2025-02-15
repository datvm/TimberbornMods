namespace NoWaterCompression;

[Context("Game")]
public class ModConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
    }
}
