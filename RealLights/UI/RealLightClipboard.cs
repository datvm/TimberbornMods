namespace RealLights.UI;

public readonly record struct RealLightClipboard(bool ForceOff, bool ForceNightLight, ImmutableArray<RealLightProperties> CustomProperties)
{
    public static readonly RealLightClipboard Empty = new(false, false, []);
}