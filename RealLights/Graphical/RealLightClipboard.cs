namespace RealLights.Graphical;

public readonly record struct RealLightClipboard(bool ForceOff, bool ForceNightLight, ImmutableArray<RealLightProperties> CustomProperties);