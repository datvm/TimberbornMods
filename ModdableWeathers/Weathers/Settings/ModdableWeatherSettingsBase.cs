namespace ModdableWeathers.Weathers.Settings;

public abstract partial class DefaultModdableWeatherSettings : IDefaultModdableWeatherSettings
{

    static readonly Dictionary<Type, ImmutableArray<PropertyInfo>> TypeProperties = [];

    static ImmutableArray<PropertyInfo> GetPropertiesFor(Type type)
    {
        if (TypeProperties.TryGetValue(type, out var properties)) { return properties; }

        return TypeProperties[type] = properties = [.. type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<DescriptionAttribute>() is not null)];
    }

    public virtual ImmutableArray<PropertyInfo> Properties => GetPropertiesFor(GetType());

    public virtual bool CanSupport(GameModeSpec gameMode) => true;
    public abstract void SetTo(GameModeSpec gameMode);

    public virtual string Serialize()
    {
        Dictionary<string, object?> values = [];

        foreach (var prop in Properties)
        {
            values[prop.Name] = prop.GetValue(this);
        }

        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public virtual void Deserialize(string serialized)
    {
        var values = JsonConvert.DeserializeObject<Dictionary<string, object?>>(serialized);
        if (values is null) { return; }

        foreach (var prop in Properties)
        {
            if (values.TryGetValue(prop.Name, out var value))
            {
                prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType));
            }
        }
    }

}
