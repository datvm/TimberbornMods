namespace Omnibar.Models;

public class TodoListEntry
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Completed { get; set; }
    public bool Pin { get; set; }

    public float? Timer { get; set; }

    [Obsolete("For compatibility only, use Buildings instead")]
    public string? Building { get; set; }
    [Obsolete("For compatibility only, use Buildings instead")]
    public int BuildingQuantity { get; set; }

    public bool ShowBuildingDetails { get; set; } = true;
    public List<TodoListEntryBuilding> Buildings { get; init; } = [];

    public string Serialize() => JsonConvert.SerializeObject(this);
    public static TodoListEntry Deserialize(string json)
    {
        var result = JsonConvert.DeserializeObject<TodoListEntry>(json) ?? new();


#pragma warning disable CS0618 // For compability with old saves
        if (result.Building is not null)
        {
            result.Buildings.Add(new(result.Building, result.BuildingQuantity));
            result.Building = null;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        return result;
    }
}

public class TodoListEntryBuilding(string building, int quantity)
{

    public string Building { get; } = building;
    public int Quantity { get; private set; } = quantity;

    [JsonIgnore]
    public BlockObjectTool? BuildingTool { get; set; }

    public event Action<int>? QuantityChanged;

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
        QuantityChanged?.Invoke(quantity);
    }

}

public readonly record struct TodoListBuildingDetails(TodoListEntryBuilding Entry, BuildingSpec BuildingSpec, LabeledEntitySpec LabelSpec)
{

    public TodoListBuildingDetails(TodoListEntryBuilding Entry) : this(
        Entry,
        Entry.BuildingTool!.Prefab.GetComponentFast<BuildingSpec>(),
        Entry.BuildingTool!.Prefab.GetComponentFast<LabeledEntitySpec>()
    )
    { }

}