namespace FasterUnderwaterMovement;

[Context("MainMenu")]
[Context("Game")]
public class ModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}