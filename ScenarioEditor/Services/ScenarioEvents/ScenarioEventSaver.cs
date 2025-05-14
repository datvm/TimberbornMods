namespace ScenarioEditor.Services.ScenarioEvents;

public class ScenarioEventSaver : IValueSerializer<ScenarioEvent>
{
    public static readonly ScenarioEventSaver Instance = new();

    static readonly JsonSerializerSettings jsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Objects,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        Formatting = Formatting.None,
    };

    public Obsoletable<ScenarioEvent> Deserialize(IValueLoader valueLoader)
    {
        return JsonConvert.DeserializeObject<ScenarioEvent>(
            valueLoader.AsString(),
            jsonSettings
        )!;
    }

    public void Serialize(ScenarioEvent value, IValueSaver valueSaver)
    {
        valueSaver.AsString(JsonConvert.SerializeObject(value, jsonSettings));
    }
}
 