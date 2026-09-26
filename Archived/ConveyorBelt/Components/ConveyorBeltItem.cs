namespace ConveyorBelt.Components;

public class ConveyorBeltItem(string goodId)
{
    public string GoodId => goodId;
    public float Position { get; set; }

    public bool Stuck { get; set; }

    public string Serialize() => $"{goodId};{Position}";
    public static ConveyorBeltItem Deserialize(string serialized)
    {
        var parts = serialized.Split(';');
        if (parts.Length != 2)
        {
            throw new FormatException($"Invalid serialized ConveyorBeltItem: {serialized}");
        }
        var goodId = parts[0];
        if (!float.TryParse(parts[1], out var position))
        {
            throw new FormatException($"Invalid position in serialized ConveyorBeltItem: {serialized}");
        }
        return new(goodId) { Position = position };
    }
}
