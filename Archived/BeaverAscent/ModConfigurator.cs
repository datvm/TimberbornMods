namespace BeaverAscent;

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class ModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}

[Context("Game")]
[Context("MapEditor")]
public class CameraConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<FreeCameraService>().AsSingleton();
    }
}
