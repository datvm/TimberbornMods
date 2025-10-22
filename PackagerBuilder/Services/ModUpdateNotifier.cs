namespace PackagerBuilder.Services;

public class ModUpdateNotifier : IModUpdateNotifier
{
    public string ModId { get; } = nameof(PackagerBuilder);
    public string Version { get; } = "7.1.0";
    public string MessageLocKey { get; } = "LV.Pkg.Update710";
}
