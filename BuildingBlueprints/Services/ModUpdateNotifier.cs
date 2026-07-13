namespace BuildingBlueprints.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(BuildingBlueprints);    
    public string Version => "11.0.0";
    public int VersionNumber => 110000;
    public string MessageLocKey => "";    
}
