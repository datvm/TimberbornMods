namespace SkyblightWeather;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(SkyblightWeather)).PatchAll();
    }

}
