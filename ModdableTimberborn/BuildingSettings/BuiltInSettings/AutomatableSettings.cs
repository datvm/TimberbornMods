namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record AutomatableSettingsModel(Guid? EntityId) : EntityIdModelBase([EntityId])
{
    public Guid? EntityId
    {
        get => EntityIds[0]; 
        set => EntityIds[0] = value;
    }
}

public class AutomatableSettings(
    EntityRegistry entityRegistry,
    ILoc t
) : BuildingSettingsBase<Automatable, AutomatableSettingsModel>(t)
{

    public override string DescribeModel(AutomatableSettingsModel model) => entityRegistry.DescribeEntity(model.EntityId, t);

    protected override bool ApplyModel(AutomatableSettingsModel model, Automatable target)
    {
        var automator = entityRegistry.TryGetAutomator(model.EntityId);
        target.SetInput(automator);
        return true;
    }

    protected override AutomatableSettingsModel GetModel(Automatable duplicable) 
        => new(duplicable._inputConnection.Transmitter?.GetComponent<EntityComponent>().EntityId);

}
