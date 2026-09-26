namespace ConveyorBelt.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(ConveyorBelt);
    public string Version => "11.3.0";
    public int VersionNumber => 113000;
    public string MessageLocKey => "LV.CBlt.ModUpdate113000";
}
