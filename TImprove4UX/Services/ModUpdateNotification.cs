namespace TImprove4UX.Services;

public class ModUpdateNotification : IModUpdateNotifier
{
    public string ModId { get; } = nameof(TImprove4UX);
    public string Version { get; } = "7.3.0";
    public string MessageLocKey { get; } = "LV.T4UX.UpdateText730";
}
