namespace Timbermesh2Blender.Models;

/// <summary>
/// Parsed Unity material ready for glTF / Blender export.
/// <see cref="Name"/> is the original timbermesh / Unity material name and should be preserved.
/// </summary>
public sealed class UnityMaterial
{
    public required string Name { get; init; }
    public required string MatFilePath { get; init; }

    public IReadOnlyDictionary<string, TextureMap> Textures { get; init; }
        = FrozenDictionary<string, TextureMap>.Empty;

    public IReadOnlyDictionary<string, float> Floats { get; init; }
        = FrozenDictionary<string, float>.Empty;

    public IReadOnlyDictionary<string, ColorRgba> Colors { get; init; }
        = FrozenDictionary<string, ColorRgba>.Empty;

    public TextureMap? BaseColorMap => GetTexture("_MainTex") ?? GetTexture("_BaseMap");
    public TextureMap? NormalMap => GetTexture("_BumpMap") ?? GetTexture("_NormalMap");
    public TextureMap? MetallicRoughnessMap => GetTexture("_MetallicGlossMap") ?? GetTexture("_MaskMap");
    public TextureMap? OcclusionMap => GetTexture("_AmbientOcclusion") ?? GetTexture("_OcclusionMap");
    public TextureMap? EmissionMap => GetTexture("_EmissionMap");

    public ColorRgba BaseColorFactor
    {
        get
        {
            if (Colors.TryGetValue("_BaseColor", out var baseColor))
            {
                return baseColor;
            }

            if (Colors.TryGetValue("_Color", out var color))
            {
                return color;
            }

            return ColorRgba.White;
        }
    }

    public ColorRgba EmissionFactor => Colors.TryGetValue("_EmissionColor", out var emission)
        ? emission
        : ColorRgba.Black;

    public float MetallicFactor => GetFloat("_Metallic", 0f);
    public float Smoothness => GetFloat("_Smoothness", GetFloat("_Glossiness", 0f));
    public float RoughnessFactor => 1f - Smoothness;
    public float NormalScale => GetFloat("_BumpScale", 1f);
    public float OcclusionStrength => GetFloat("_OcclusionStrength", 1f);
    public bool AlphaClip => GetFloat("_AlphaClip", 0f) > 0.5f || GetFloat("_Cutout", 0f) > 0.5f;
    public float AlphaCutoff => GetFloat("_Cutoff", 0.5f);

    public TextureMap? GetTexture(string propertyName)
        => Textures.TryGetValue(propertyName, out var map) && map.FilePath is not null
            ? map
            : null;

    public float GetFloat(string propertyName, float defaultValue)
        => Floats.TryGetValue(propertyName, out var value) ? value : defaultValue;
}

public sealed record TextureMap(
    string PropertyName,
    string? Guid,
    string? FilePath,
    float ScaleX = 1f,
    float ScaleY = 1f,
    float OffsetX = 0f,
    float OffsetY = 0f
)
{
    public bool HasFile => FilePath is not null && File.Exists(FilePath);
};

public readonly record struct ColorRgba(float R, float G, float B, float A)
{
    public static ColorRgba White { get; } = new(1f, 1f, 1f, 1f);
    public static ColorRgba Black { get; } = new(0f, 0f, 0f, 1f);
}
