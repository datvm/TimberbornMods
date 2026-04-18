namespace TImprove.Services;

public class ModUpdateNotifier : IModUpdateNotifier
{
    public string ModId => nameof(TImprove);
    public string Version => "10.2.0";
    public string MessageLocKey => "LV.TI.Update1220";
}
