namespace TImprove4Ui.Services;

[MultiBind(typeof(IModUpdateNotifier), Contexts = BindAttributeContext.MainMenu)]
public class ModUpdateNotifier : IModUpdateNotifier
{
    public string ModId { get; } = nameof(TImprove4Ui);
    public string Version { get; } = "7.10.2";
    public string MessageLocKey { get; } = "LV.T4UI.Update7_10_2";
}
