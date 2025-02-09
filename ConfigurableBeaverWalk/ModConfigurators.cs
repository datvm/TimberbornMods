namespace ConfigurableBeaverWalk;


[Context("MainMenu")]
[Context("Game")]
public class ModConfigurators : IConfigurator
{

    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}
