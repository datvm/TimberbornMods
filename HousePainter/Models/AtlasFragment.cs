namespace HousePainter.Models;

/// <summary>
/// One source material tile inside a FactionAtlas (UV rect in atlas space).
/// </summary>
public readonly record struct AtlasFragment(
    string AtlasName,
    string MaterialName,
    string MaterialPath,
    Vector2 UVOffset,
    Vector2 UVScale
);
