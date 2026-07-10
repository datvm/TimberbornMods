namespace ConveyorBelt.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(ConveyorBelt);
    public string Version => "11.1.0";
    public int VersionNumber => 111000;
    public string MessageLocKey => "LV.CBlt.Update110";
}
