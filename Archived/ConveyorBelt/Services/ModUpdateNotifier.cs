namespace ConveyorBelt.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(ConveyorBelt);
    public string Version => "11.2.0";
    public int VersionNumber => 112000;
    public string MessageLocKey => "LV.CBlt.Update120";
}
