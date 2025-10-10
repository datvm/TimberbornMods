namespace BuildingHP.Services;

public class UpdateNotification : IModUpdateNotifier
{
    public string ModId { get; } = nameof(BuildingHP);
    public string Version { get; } = "7.1.0";
    public string MessageLocKey { get; } = "LV.BHP.UpdateText710";
}
