namespace NoWaterCompression;

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class SettingConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
    }
}


[Context("Game")]
[Context("MapEditor")]
public class ModServicesConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<WaterModService>().AsSingleton();
    }
}