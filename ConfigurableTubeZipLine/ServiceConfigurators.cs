using Bindito.Core.Internal;
using UnityEngine.InputSystem.Utilities;

namespace ConfigurableTubeZipLine;

[Context("Game")]
[Context("MainMenu")]
public class SettingConfigurator : Configurator
{

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class GameConfigurator : Configurator
{

    public override void Configure()
    {
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            var b = new TemplateModule.Builder();

            b.AddDecorator<Tube, ConfigurableTubeService>();

            return b.Build();
        }).AsSingleton();

    }

}