namespace BuildingBlueprints.Services;

//[MultiBind(typeof(IModUpdateNotifier), Contexts = BindAttributeContext.MainMenu)]
public class ModUpdateNotifier : IModUpdateNotifier
{
    public string ModId => nameof(BuildingBlueprints);
    public string Version => "10.2.0";
    public string MessageLocKey => "LV.BB.Update1020";
}
