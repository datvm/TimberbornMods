namespace ConstantWind;

[Context("MainMenu")]
[Context("Game")]
public class MConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
    }
}
