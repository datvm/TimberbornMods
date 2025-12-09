namespace NarrativeEvents.Services;

[Context("Game")]
public class GameContextConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<NarrativeService>().AsSingleton();
        containerDefinition.Bind<NarrativeDialogBoxShower>().AsSingleton();
    }
}
