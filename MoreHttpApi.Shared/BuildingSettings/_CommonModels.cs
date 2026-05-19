namespace MoreHttpApi.Shared.BuildingSettings;

public interface IEntityIdModel
{
    Guid?[] EntityIds { get; set; }
}

public record EntityIdModelBase(Guid?[] EntityIds) : IEntityIdModel
{
    public Guid?[] EntityIds { get; set; } = EntityIds;
}

public record ValueSettingModel<T>(T Value)
{
    public static implicit operator T(ValueSettingModel<T> model) => model.Value;
}

public record PrioritySettingModel(HttpPriority Value) : ValueSettingModel<HttpPriority>(Value);

public record BoolSettingModel(bool Value) : ValueSettingModel<bool>(Value)
{
    public static implicit operator BoolSettingModel(bool on) => new(on);
}

public record IntSettingModel(int Value) : ValueSettingModel<int>(Value)
{
    public static implicit operator IntSettingModel(int value) => new(value);
}

public record FloatSettingModel(float Value) : ValueSettingModel<float>(Value)
{
    public static implicit operator FloatSettingModel(float value) => new(value);
}

public record StringSettingModel(string Value) : ValueSettingModel<string>(Value)
{
    public static implicit operator StringSettingModel(string value) => new(value);
    public bool None => string.IsNullOrEmpty(Value);
    public override string ToString() => Value;
}