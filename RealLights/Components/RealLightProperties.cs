namespace RealLights.Components;

public readonly record struct CustomRealLightProperties([JsonConverter(typeof(ColorHandler))]Color? Color, float? Range, float? Intensity)
{
    public CustomRealLightProperties Modify(CustomRealLightProperties? add) => new(
            add?.Color ?? Color,
            add?.Range ?? Range,
            add?.Intensity ?? Intensity);

    public static implicit operator CustomRealLightProperties(RealLightProperties props) => new(props.Color, props.Range, props.Intensity);
    public static explicit operator RealLightProperties(CustomRealLightProperties props) => new(
        props.Color ?? default,
        props.Range ?? default,
        props.Intensity ?? default);

}

public readonly record struct RealLightProperties(Color Color, float Range, float Intensity)
{

    public RealLightProperties Modify(CustomRealLightProperties? add)
    {
        if (add is null) { return this; }

        return (RealLightProperties)((CustomRealLightProperties)this).Modify(add);
    }

    public static implicit operator RealLightProperties(RealLightLightSpec spec) => new(spec.Color, spec.Range, spec.Intensity);

}