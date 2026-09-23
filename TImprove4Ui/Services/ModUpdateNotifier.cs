namespace TImprove4Ui.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(TImprove4Ui);
    public string Version => "11.2.1";
    public int VersionNumber => 110201;
    public string MessageLocKey => "";
}
