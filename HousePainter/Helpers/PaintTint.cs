namespace HousePainter.Helpers;

/// <summary>
/// Albedo multiply compensation for EnvironmentURP atlas materials.
/// </summary>
public static class PaintTint
{

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    /// <summary>
    /// HDR overdrive so multiply against dark wood/plaster still reads as color.
    /// </summary>
    public const float ColorBoost = 1.45f;

    public static int ColorPropertyId => ColorId;

    /// <summary>
    /// Normalize max channel to 1 (keep hue, full intensity), then boost.
    /// </summary>
    public static Color Compensate(Color tint)
    {
        if (tint is { r: >= 0.99f, g: >= 0.99f, b: >= 0.99f })
        {
            return Color.white;
        }

        var max = Mathf.Max(tint.r, Mathf.Max(tint.g, tint.b));
        if (max <= 1e-4f)
        {
            return Color.white;
        }

        return new(
            tint.r / max * ColorBoost,
            tint.g / max * ColorBoost,
            tint.b / max * ColorBoost,
            1f
        );
    }

    public static void ResetMaterialBase(Material material)
    {
        if (material.HasProperty(ColorId))
        {
            material.SetColor(ColorId, Color.white);
        }

        if (material.HasProperty(EmissionColorId))
        {
            material.SetColor(EmissionColorId, Color.clear);
        }
    }

}
