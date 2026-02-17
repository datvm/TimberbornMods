namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class StockpilePrioritySettings(ILoc t) : BuildingSettingsBase<StockpilePriority, ValueSettingModel<StockpilePriorityState>>(t)
{
    public override string DescribeModel(ValueSettingModel<StockpilePriorityState> model) => t.T("StockpilePriority." + model.Value);

    protected override bool ApplyModel(ValueSettingModel<StockpilePriorityState> model, StockpilePriority target)
    {
        switch (model.Value)
        {
            case StockpilePriorityState.Accept:
                target.Accept();
                break;
            case StockpilePriorityState.Empty:
                target.Empty();
                break;
            case StockpilePriorityState.Obtain:
                target.Obtain();
                break;
            case StockpilePriorityState.Supply:
                target.Supply();
                break;
        }

        return true;
    }

    protected override ValueSettingModel<StockpilePriorityState> GetModel(StockpilePriority duplicable) => new(
        duplicable.IsAcceptActive   ? StockpilePriorityState.Accept :
        duplicable.IsEmptyActive    ? StockpilePriorityState.Empty :
        duplicable.IsObtainActive   ? StockpilePriorityState.Obtain :
                                      StockpilePriorityState.Supply
    );
}

public enum StockpilePriorityState
{
    Accept,
    Empty,
    Obtain,
    Supply
}