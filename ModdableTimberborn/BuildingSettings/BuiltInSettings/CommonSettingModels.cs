namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record ValueSettingModel<T>(T Value)
{
    public static implicit operator T(ValueSettingModel<T> model) => model.Value;
}

public record PrioritySettingModel(Priority Value) : ValueSettingModel<Priority>(Value)
{
    public string T(ILoc t) => Value.T(t);
    public static implicit operator PrioritySettingModel(Priority priority) => new(priority);
}

public record BoolSettingModel(bool Value) : ValueSettingModel<bool>(Value)
{
    public string T(ILoc t) => t.TYesNo(Value);

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

    [JsonIgnore]
    public bool None => string.IsNullOrEmpty(Value);
    public override string ToString() => Value;
}

[method: JsonConstructor]
public record CachableStringSettingModel<T>(string Value) : StringSettingModel(Value)
    where T : class
{
    [JsonIgnore]
    public T? CachedValue { get; set; }
    [JsonIgnore]
    public string? CachedDisplay { get; set; }

    public CachableStringSettingModel(string? Value, T? cachedValue, ILoc t, Func<T, string> getDisplay) : this(Value ?? "")
    {
        CachedValue = cachedValue;
        CachedDisplay = cachedValue is null
            ? t.TNone()
            : getDisplay(cachedValue);
    }

    public T? EnsureCached(ILoc t, Func<string, T?> GetValue, Func<T, string> GetDisplay)
    {
        if (CachedDisplay is not null) { return CachedValue; }

        if (None)
        {
            CachedDisplay = t.TNone();
            return null;
        }

        var v = CachedValue = GetValue(Value);
        CachedDisplay = v is null
            ? t.T("LV.MT.UnknownValue", Value)
            : GetDisplay(v);

        return v;
    }

}