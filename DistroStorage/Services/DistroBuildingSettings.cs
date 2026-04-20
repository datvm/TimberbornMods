namespace DistroStorage.Services;

public abstract class ReceiverBuildingSettings<TSender>(ILoc t, EnabledTextProvider enabledTextProvider) : BuildingSettingsBase<TSender, DistroReceiverSerializableModel>(t)
    where TSender : BaseComponent, IDistroReceiver, IDuplicable<TSender>
{

    public override string DescribeModel(DistroReceiverSerializableModel model) 
        => t.T("LV.DS.SettingsDisplay", enabledTextProvider.Get(model.Enabled), model.Priority.T(t));

    protected override bool ApplyModel(DistroReceiverSerializableModel model, TSender target)
    {
        target.Deserialize(model);
        return true;
    }

    protected override DistroReceiverSerializableModel GetModel(TSender duplicable) => duplicable.Serialize();
}

public abstract class SenderBuildingSettings<TSender>(ILoc t, EnabledTextProvider enabledTextProvider) : BuildingSettingsBase<TSender, DistroSenderSerializableModel>(t)
    where TSender : BaseComponent, IDistroSender, IDuplicable<TSender>
{

    public override string DescribeModel(DistroSenderSerializableModel model) => enabledTextProvider.Get(model.Enabled);

    protected override bool ApplyModel(DistroSenderSerializableModel model, TSender target)
    {
        target.Deserialize(model);
        return true;
    }

    protected override DistroSenderSerializableModel GetModel(TSender duplicable) => duplicable.Serialize();
}

[MultiBind(typeof(IBuildingSettings))]
public class ConstructionSiteDistroReceiverBuildingSettings(ILoc t, EnabledTextProvider enabledTextProvider) : ReceiverBuildingSettings<ConstructionSiteDistroReceiver>(t, enabledTextProvider);

[MultiBind(typeof(IBuildingSettings))]
public class ManufactoryDistroReceiverBuildingSettings(ILoc t, EnabledTextProvider enabledTextProvider) : ReceiverBuildingSettings<ManufactoryDistroReceiver>(t, enabledTextProvider);

[MultiBind(typeof(IBuildingSettings))]
public class StockpileDistroSenderBuildingSettings(ILoc t, EnabledTextProvider enabledTextProvider) : SenderBuildingSettings<StockpileDistroSender>(t, enabledTextProvider);