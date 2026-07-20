namespace HousePainter.Models;

public record PaintedPart(string MaterialName)
{
    public string? Label { get; set; }
    public SerializableFloats? Color { get; set; }
}
