namespace EarthquakeWeather.Services;

public class EarthquakeDevModule(Earthquake eq) : IDevModule
{
    public DevModuleDefinition GetDefinition() =>
        new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Earthquake: Hit now", HitWithEq))
            .Build();

    void HitWithEq()
    {
        eq.Hit();
    }
}
