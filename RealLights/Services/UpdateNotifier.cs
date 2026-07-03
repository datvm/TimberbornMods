namespace RealLights.Services;

[BindModUpdateNotifier]
public class UpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(RealLights);
    public string Version => "11.0.0";
    public int VersionNumber => 110000;
    public string MessageLocKey => "";
}
