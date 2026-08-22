namespace BuildingRenovations.Services;

[BindModUpdateNotifier]
public class UpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(BuildingRenovations);
    public string Version => "11.1.0";
    public int VersionNumber => 111000;
    public string MessageLocKey => "";
}
