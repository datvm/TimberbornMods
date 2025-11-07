global using ConfigurableAutoSave.Services;

namespace ConfigurableAutoSave;

[Context("Game")]
[Context("MainMenu")]
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
        Bind<ConfigurableAutoSaveService>().AsSingleton();
        Bind<AutosaveWarningService>().AsSingleton();
    }
}
