using Timberborn.TemplateSystem;

namespace ConfigurableTubeZipLine;

[Context("Game")]
[Context("MainMenu")]
public class SettingConfigurator : IConfigurator
{

    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
    }

}

[Context("Game")]
public class GameConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.MultiBind<TemplateModule>().ToProvider(() =>
        {
            var b = new TemplateModule.Builder();

            b.AddDecorator<Tube, ConfigurableTubeService>();

            return b.Build();
        }).AsSingleton();
    }
}