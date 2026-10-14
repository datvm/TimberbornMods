namespace ConveyorBelt.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(ConveyorBelt);
    public string Version => "11.4.0";
    public int VersionNumber => 114000;
    public string MessageLocKey => "LV.CBlt.ModUpdate114000";
}
