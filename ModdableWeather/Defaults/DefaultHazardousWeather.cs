namespace ModdableWeather.Defaults;

public abstract class DefaultHazardousWeather : IModdedHazardousWeather
{
    public abstract string Id { get; }

    public int GetDurationAtCycle(int cycle)
    {
        throw new NotImplementedException();
    }

    public float GetProbability(int cycle)
    {
        throw new NotImplementedException();
    }
}
