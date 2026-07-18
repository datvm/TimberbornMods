namespace HousePainter.Models;

/// <summary>
/// A paintable material region on a building instance after painting is enabled.
/// </summary>
public readonly record struct PaintPart(
    string MaterialName,
    AtlasFragment Fragment,
    Color Color
);
