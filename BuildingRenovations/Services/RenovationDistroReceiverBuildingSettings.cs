namespace BuildingRenovations.Services;

[MultiBind(typeof(IBuildingSettings))]
public class RenovationDistroReceiverBuildingSettings(ILoc t, EnabledTextProvider enabledTextProvider)
    : ReceiverBuildingSettings<RenovationDistroReceiver>(t, enabledTextProvider);
