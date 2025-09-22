namespace TImprove4Ui.Services;

public class ModUpdateNotifier : IModUpdateNotifier
{
    public string ModId { get; } = nameof(TImprove4Ui);
    public string Version { get; } = "7.10.1";
    public string MessageLocKey { get; } = "LV.T4UI.Update7_10_1";
}
