namespace Omnibar.Models;

public class ToDoListEntry
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Completed { get; set; }
    public bool Pin { get; set; }

    public float? Timer { get; set; }

    public string? Building { get; set; }
    public int BuildingQuantity { get; set; }

    [JsonIgnore]
    public BlockObjectTool? BuildingTool { get; set; }

    public string Serialize() => JsonConvert.SerializeObject(this);
    public static ToDoListEntry Deserialize(string json) => JsonConvert.DeserializeObject<ToDoListEntry>(json) ?? new();

}

public class ToDoListItem(string prefabName, int amount)
{
    public string PrefabName { get; set; } = prefabName;
    public int Amount { get; set; } = amount;

    [JsonIgnore]
    public object? ExtraData { get; set; }

}
