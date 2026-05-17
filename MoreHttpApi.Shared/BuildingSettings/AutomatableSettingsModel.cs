namespace MoreHttpApi.Shared.BuildingSettings;

public record AutomatableSettingsModel(Guid? EntityId) : EntityIdModelBase([EntityId])
{
    public Guid? EntityId
    {
        get => EntityIds[0]; 
        set => EntityIds[0] = value;
    }
}
