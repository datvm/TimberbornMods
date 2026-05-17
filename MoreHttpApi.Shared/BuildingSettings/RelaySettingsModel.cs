namespace MoreHttpApi.Shared.BuildingSettings;

public record RelaySettingsModel(
    HttpRelayMode Mode,
    Guid? InputA,
    Guid? InputB
) : EntityIdModelBase([InputA, InputB])
{
    public Guid? InputA
    {
        get => EntityIds[0];
        set => EntityIds[0] = value;
    }

    public Guid? InputB
    {
        get => EntityIds[1];
        set => EntityIds[1] = value;
    }
}

public enum HttpRelayMode
{
    Not,
    And,
    Or,
    Xor,
    Passthrough
}
