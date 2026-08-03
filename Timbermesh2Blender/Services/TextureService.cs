namespace Timbermesh2Blender.Services;

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;

public partial class TextureService
{
    static readonly Regex GuidReferenceRegex = GuidReferencePattern();
    static readonly Regex ColorRegex = ColorPattern();
    static readonly Regex Vec2Regex = Vec2Pattern();

    readonly BlueprintProvider bp;
    readonly ConcurrentDictionary<string, UnityMaterial> materialCache = new(StringComparer.OrdinalIgnoreCase);
    readonly object guidIndexLock = new();

    FrozenDictionary<string, string>? guidToAssetPath;

    public string ResourcesPath => bp.GameResourcesFolder;
    public AggregatedCollectionBlueprint Materials { get; }
    public FrozenDictionary<string, string> MaterialPaths { get; }

    public TextureService(BlueprintProvider bp)
    {
        this.bp = bp;
        Materials = bp.AggregatedCollections[typeof(MaterialCollectionSpec)];
        MaterialPaths = BuildMaterialPaths();
    }

    public UnityMaterial GetMaterial(string nameOrPath)
    {
        if (TryGetMaterial(nameOrPath, out var material))
        {
            return material;
        }

        throw new KeyNotFoundException($"Material not found: {nameOrPath}");
    }

    public bool TryGetMaterial(string nameOrPath, [NotNullWhen(true)] out UnityMaterial? material)
    {
        if (materialCache.TryGetValue(nameOrPath, out material))
        {
            return true;
        }

        if (!TryGetMaterialPath(nameOrPath, out var matFilePath))
        {
            material = null;
            return false;
        }

        material = materialCache.GetOrAdd(matFilePath, ParseMatFile);
        materialCache.TryAdd(material.Name, material);
        materialCache.TryAdd(nameOrPath, material);
        return true;
    }

    public string GetMaterialPath(string nameOrPath)
    {
        if (TryGetMaterialPath(nameOrPath, out var path))
        {
            return path;
        }

        throw new KeyNotFoundException($"Material not found: {nameOrPath}");
    }

    public bool TryGetMaterialPath(string nameOrPath, [NotNullWhen(true)] out string? path)
        => MaterialPaths.TryGetValue(nameOrPath, out path);

    public bool TryResolveGuid(string guid, [NotNullWhen(true)] out string? assetPath)
        => GetGuidIndex().TryGetValue(guid, out assetPath);

    FrozenDictionary<string, string> GetGuidIndex()
    {
        if (guidToAssetPath is not null)
        {
            return guidToAssetPath;
        }

        lock (guidIndexLock)
        {
            return guidToAssetPath ??= BuildGuidIndex();
        }
    }

    FrozenDictionary<string, string> BuildMaterialPaths()
    {
        Dictionary<string, string> materialPaths = new(StringComparer.OrdinalIgnoreCase);

        foreach (var relativePath in Materials.Collections.Values
            .SelectMany(static paths => paths)
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalizedRelative = relativePath.Replace('\\', '/');
            var matFile = Path.GetFullPath(Path.Combine(
                ResourcesPath,
                normalizedRelative.Replace('/', Path.DirectorySeparatorChar) + ".mat"));

            if (!File.Exists(matFile))
            {
                Console.Error.WriteLine($"Material file not found: {matFile}");
                continue;
            }

            // Timbermesh references materials by Unity material name (file name without extension).
            var name = Path.GetFileName(normalizedRelative);
            materialPaths[name] = matFile;
            materialPaths[normalizedRelative] = matFile;
        }

        return materialPaths.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    FrozenDictionary<string, string> BuildGuidIndex()
    {
        Dictionary<string, string> index = new(StringComparer.OrdinalIgnoreCase);

        foreach (var metaPath in Directory.EnumerateFiles(ResourcesPath, "*.meta", SearchOption.AllDirectories))
        {
            if (!TryReadMetaGuid(metaPath, out var guid))
            {
                continue;
            }

            var assetPath = metaPath[..^".meta".Length];
            if (!File.Exists(assetPath))
            {
                continue;
            }

            // First wins; duplicates are rare and usually the same asset.
            index.TryAdd(guid, assetPath);
        }

        return index.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    static bool TryReadMetaGuid(string metaPath, [NotNullWhen(true)] out string? guid)
    {
        guid = null;

        using var reader = new StreamReader(metaPath);
        for (var i = 0; i < 20 && reader.ReadLine() is { } line; i++)
        {
            const string prefix = "guid: ";
            var trimmed = line.TrimStart();
            if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            guid = trimmed[prefix.Length..].Trim();
            return guid.Length > 0;
        }

        return false;
    }

    UnityMaterial ParseMatFile(string matFilePath)
    {
        var text = File.ReadAllText(matFilePath);
        var name = Path.GetFileNameWithoutExtension(matFilePath);

        Dictionary<string, TextureMap> textures = new(StringComparer.Ordinal);
        Dictionary<string, float> floats = new(StringComparer.Ordinal);
        Dictionary<string, ColorRgba> colors = new(StringComparer.Ordinal);

        string? section = null;
        string? currentTexProperty = null;
        string? currentTexGuid = null;
        float scaleX = 1f, scaleY = 1f, offsetX = 0f, offsetY = 0f;

        void FlushTexture()
        {
            if (currentTexProperty is null)
            {
                return;
            }

            string? filePath = null;
            if (currentTexGuid is not null)
            {
                GetGuidIndex().TryGetValue(currentTexGuid, out filePath);
            }

            textures[currentTexProperty] = new TextureMap(
                currentTexProperty,
                currentTexGuid,
                filePath,
                scaleX,
                scaleY,
                offsetX,
                offsetY);

            currentTexProperty = null;
            currentTexGuid = null;
            scaleX = 1f;
            scaleY = 1f;
            offsetX = 0f;
            offsetY = 0f;
        }

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            var trimmed = line.Trim();

            if (trimmed.StartsWith("m_Name:", StringComparison.Ordinal))
            {
                name = trimmed["m_Name:".Length..].Trim();
                continue;
            }

            if (trimmed is "m_TexEnvs:")
            {
                FlushTexture();
                section = "TexEnvs";
                continue;
            }

            if (trimmed is "m_Floats:")
            {
                FlushTexture();
                section = "Floats";
                continue;
            }

            if (trimmed is "m_Colors:")
            {
                FlushTexture();
                section = "Colors";
                continue;
            }

            if (trimmed is "m_Ints:" or "m_BuildTextureStacks:" or "m_AllowLocking:")
            {
                FlushTexture();
                section = null;
                continue;
            }

            switch (section)
            {
                case "TexEnvs":
                    if (IsTexturePropertyHeader(line, out var texProperty))
                    {
                        FlushTexture();
                        currentTexProperty = texProperty;
                        continue;
                    }

                    if (currentTexProperty is null)
                    {
                        continue;
                    }

                    if (trimmed.StartsWith("m_Texture:", StringComparison.Ordinal))
                    {
                        currentTexGuid = ExtractGuid(trimmed);
                        continue;
                    }

                    if (trimmed.StartsWith("m_Scale:", StringComparison.Ordinal) && TryParseVec2(trimmed, out scaleX, out scaleY))
                    {
                        continue;
                    }

                    if (trimmed.StartsWith("m_Offset:", StringComparison.Ordinal) && TryParseVec2(trimmed, out offsetX, out offsetY))
                    {
                        continue;
                    }

                    break;

                case "Floats":
                    if (TryParseFloatProperty(trimmed, out var floatName, out var floatValue))
                    {
                        floats[floatName] = floatValue;
                    }
                    break;

                case "Colors":
                    if (TryParseColorProperty(trimmed, out var colorName, out var colorValue))
                    {
                        colors[colorName] = colorValue;
                    }
                    break;
            }
        }

        FlushTexture();

        return new UnityMaterial
        {
            Name = name,
            MatFilePath = matFilePath,
            Textures = textures.ToFrozenDictionary(StringComparer.Ordinal),
            Floats = floats.ToFrozenDictionary(StringComparer.Ordinal),
            Colors = colors.ToFrozenDictionary(StringComparer.Ordinal),
        };
    }

    static bool IsTexturePropertyHeader(string line, [NotNullWhen(true)] out string? propertyName)
    {
        propertyName = null;

        // Texture property headers look like "      _MainTex:" (indented, leading underscore).
        if (line.Length == 0 || line[0] is not (' ' or '\t'))
        {
            return false;
        }

        var trimmed = line.Trim();
        if (trimmed.Length < 2 || trimmed[0] != '_' || !trimmed.EndsWith(':'))
        {
            return false;
        }

        if (trimmed.StartsWith("m_", StringComparison.Ordinal))
        {
            return false;
        }

        propertyName = trimmed[..^1];
        return true;
    }

    static string? ExtractGuid(string textureLine)
    {
        var match = GuidReferenceRegex.Match(textureLine);
        return match.Success ? match.Groups[1].Value : null;
    }

    static bool TryParseVec2(string line, out float x, out float y)
    {
        x = 0f;
        y = 0f;
        var match = Vec2Regex.Match(line);
        if (!match.Success)
        {
            return false;
        }

        return float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x)
            && float.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
    }

    static bool TryParseFloatProperty(string trimmed, [NotNullWhen(true)] out string? name, out float value)
    {
        name = null;
        value = 0f;

        var split = trimmed.IndexOf(':');
        if (split <= 0)
        {
            return false;
        }

        name = trimmed[..split].Trim();
        if (name.Length == 0 || name[0] != '_')
        {
            return false;
        }

        var number = trimmed[(split + 1)..].Trim();
        return float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    static bool TryParseColorProperty(string trimmed, [NotNullWhen(true)] out string? name, out ColorRgba color)
    {
        name = null;
        color = default;

        var split = trimmed.IndexOf(':');
        if (split <= 0)
        {
            return false;
        }

        name = trimmed[..split].Trim();
        if (name.Length == 0 || name[0] != '_')
        {
            return false;
        }

        var match = ColorRegex.Match(trimmed);
        if (!match.Success)
        {
            return false;
        }

        if (!float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
            || !float.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var g)
            || !float.TryParse(match.Groups[3].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var b)
            || !float.TryParse(match.Groups[4].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
        {
            return false;
        }

        color = new ColorRgba(r, g, b, a);
        return true;
    }

    [GeneratedRegex(@"guid:\s*([a-fA-F0-9]+)", RegexOptions.CultureInvariant)]
    private static partial Regex GuidReferencePattern();

    [GeneratedRegex(
        @"\{r:\s*([-\d.]+),\s*g:\s*([-\d.]+),\s*b:\s*([-\d.]+),\s*a:\s*([-\d.]+)\}",
        RegexOptions.CultureInvariant)]
    private static partial Regex ColorPattern();

    [GeneratedRegex(@"\{x:\s*([-\d.]+),\s*y:\s*([-\d.]+)\}", RegexOptions.CultureInvariant)]
    private static partial Regex Vec2Pattern();
}
