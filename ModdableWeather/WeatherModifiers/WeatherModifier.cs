namespace ModdableWeather.WeatherModifiers;

public abstract class WeatherModifier
{
    public abstract string Id { get; }
    public WeatherModifierSpec Spec { get; internal set; } = null!;

    public virtual void Start()
    {

    }

    public virtual void End()
    {

    }

}
