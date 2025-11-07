global using SaveEveryday.Services;

namespace SaveEveryday;

[Context("MainMenu")]
[Context("Game")]
public class MSettingsConfig : Configurator
{
    
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }

}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<SaveEverydayService>().AsSingleton();
        Bind<AutosaveWarningService>().AsSingleton();
    }
}
