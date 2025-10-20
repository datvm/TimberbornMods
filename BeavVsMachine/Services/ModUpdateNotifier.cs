namespace BeavVsMachine.Services;

public class ModUpdateNotifier : IModUpdateNotifier
{
    public string ModId { get; } = nameof(BeavVsMachine);
    public string Version { get; } = "7.1.0";
    public string MessageLocKey { get; } = "LV.BVM.UpdateText710";
}
