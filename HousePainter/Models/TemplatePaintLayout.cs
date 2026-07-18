namespace HousePainter.Models;

/// <summary>
/// Paint targets for a building template: which atlas fragments it uses.
/// </summary>
public readonly record struct TemplatePaintLayout(
    string TemplateName,
    ImmutableArray<string> SourceMaterialNames,
    ImmutableArray<AtlasFragment> Fragments
);
