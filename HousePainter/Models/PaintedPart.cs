namespace HousePainter.Models;

public record PaintedPart(string MaterialName)
{
    public string? Label { get; set; }

    /// <summary>Null means label-only / not painted.</summary>
    public SerializableFloats? Color { get; set; }
}
