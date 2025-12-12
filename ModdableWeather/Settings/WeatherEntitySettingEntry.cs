namespace ModdableWeather.Settings;

public abstract class WeatherEntitySettingEntry
{
    static readonly Dictionary<Type, ImmutableArray<DescriptiveProperty>> typeProperties = [];

    public abstract string EntityId { get; }

    public virtual void InitValue(GameModeSpec gameMode) => SetValueForDifficulty(gameMode, true);

    public virtual bool CanSupport(GameModeSpec gameMode) => true;
    public void SetValueForDifficulty(GameModeSpec gameMode) => SetValueForDifficulty(gameMode, false);

    public abstract void SetValueForDifficulty(GameModeSpec gameMode, bool firstTime);

    public virtual void SetValue(JToken json)
    {
        if (json is not JObject obj) { return; }

        foreach (var prop in GetSettingProperties())
        {
            var jValue = obj.Property(prop.Property.Name);
            if (jValue?.HasValues != true) { continue; }

            var value = jValue.Value.ToObject(prop.Property.PropertyType);
            prop.Property.SetValue(this, value);
        }
    }

    public virtual JToken? ToJson()
    {
        var obj = new JObject();

        foreach (var prop in GetSettingProperties())
        {
            var value = prop.Property.GetValue(this);
            if (value is null) { continue; }

            obj[prop.Property.Name] = JToken.FromObject(value);
        }

        return obj;
    }

    public ImmutableArray<DescriptiveProperty> GetSettingProperties()
    {
        var t = GetType();
        if (typeProperties.TryGetValue(t, out var properties)) { return properties; }

        List<DescriptiveProperty> props = [];
        foreach (var prop in t.GetProperties())
        {
            var attr = prop.GetCustomAttribute<DescriptionAttribute>();
            if (attr is null) { continue; }

            props.Add(new DescriptiveProperty(attr.Description, prop));
        }

        typeProperties[t] = properties = [.. props];

        return properties;
    }
}

public readonly record struct DescriptiveProperty(string LocKey, PropertyInfo Property);
