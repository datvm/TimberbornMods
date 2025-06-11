namespace ConfigurableFaction.Services;

public class UpdateNotifier : IModUpdateNotifier
{
    public string ModId { get; } = nameof(ConfigurableFaction);
    public string Version { get; } = "7.2.0";
    public string MessageLocKey { get; } = "LV.CFac.UpdateNotify";
}