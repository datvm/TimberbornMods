global using ConstantWind.Services;

namespace ConstantWind;

[Context("MainMenu")]
[Context("Game")]
public class ModSettingConfig : Configurator
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
        Bind<WindConfigurationService>().AsSingleton();
    }
}