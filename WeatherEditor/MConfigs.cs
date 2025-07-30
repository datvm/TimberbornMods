global using WeatherEditor.Services;
global using WeatherEditor.UI;

namespace WeatherEditor;

[Context("Game")]
public class ModGameConfig : Configurator
{

    public static readonly bool HasModdableWeather = AppDomain.CurrentDomain
        .GetAssemblies()
        .Any(q => q.GetName().Name == nameof(ModdableWeather));

    public override void Configure()
    {
        if (HasModdableWeather)
        {
            this.BindSingleton<IWeatherModService, ModdableWeatherModService>();
        }
        else
        {
            this.BindSingleton<IWeatherModService, GameWeatherModService>();
        }

        this
            .BindSingleton<GameSaverService>()

            .BindSingleton<WeatherEditorController>()
            .BindTransient<WeatherEditorDialog>()
        ;
    }

}
