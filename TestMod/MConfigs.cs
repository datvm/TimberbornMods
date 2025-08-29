namespace TestMod;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<TestWeather>().AsSingleton();
    }
}

class TestWeather(IContainer container) : IPostLoadableSingleton
{

    public void PostLoad()
    {
        var moddableWeatherAsm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(q => q.GetName().Name == "ModdableWeather");
        if (moddableWeatherAsm is null)
        {
            Debug.Log("No ModdableWeather");
            return;
        }

        var registryType = moddableWeatherAsm.GetTypes()
            .First(q => q.FullName == "ModdableWeather.Services.ModdableWeatherRegistry");
        var registry = container.GetInstance(registryType);

        var temperates = (IEnumerable<object>)registryType.GetProperty("TemperateWeathers").GetValue(registry);
        var moddableWeatherInterface = moddableWeatherAsm.GetType("ModdableWeather.Specs.IModdedWeather");
        var idProp = moddableWeatherInterface.GetProperty("Id");

        List<string> ids = [];
        foreach (var w in temperates)
        {
            ids.Add((string)idProp.GetValue(w));
        }

        Debug.Log(string.Join(", ", ids));
    }

}
