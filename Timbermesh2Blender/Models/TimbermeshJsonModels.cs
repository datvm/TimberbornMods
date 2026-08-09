namespace Timbermesh2Blender.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Machine-friendly intermediate for AI / text editing of Timbermesh models.
/// Round-trips with <c>tmesh2glb dump</c> and <c>tmesh2glb pack</c>.
/// </summary>
public class TimbermeshJsonDocument
{
    public const string FormatId = "timbermesh-json";
    public const int CurrentFormatVersion = 1;

    [JsonPropertyName("format")]
    public string Format { get; set; } = FormatId;

    [JsonPropertyName("formatVersion")]
    public int FormatVersion { get; set; } = CurrentFormatVersion;

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("modelVersion")]
    public int ModelVersion { get; set; }

    /// <summary>Convenience list of distinct material names (dump only; ignored on pack).</summary>
    [JsonPropertyName("materials")]
    public List<string> Materials { get; set; } = [];

    [JsonPropertyName("nodeCount")]
    public int NodeCount { get; set; }

    [JsonPropertyName("totalTriangles")]
    public int TotalTriangles { get; set; }

    [JsonPropertyName("nodes")]
    public List<TimbermeshJsonNode> Nodes { get; set; } = [];
}

public class TimbermeshJsonNode
{
    /// <summary>Index in the final packed node array (0-based). Parents refer to these indices.</summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>Parent node index, or -1 for roots.</summary>
    [JsonPropertyName("parent")]
    public int Parent { get; set; } = -1;

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("position")]
    public TimbermeshJsonVec3 Position { get; set; } = new();

    [JsonPropertyName("rotation")]
    public TimbermeshJsonQuat Rotation { get; set; } = new();

    [JsonPropertyName("scale")]
    public TimbermeshJsonVec3 Scale { get; set; } = new() { X = 1, Y = 1, Z = 1 };

    [JsonPropertyName("vertexCount")]
    public int VertexCount { get; set; }

    /// <summary>AABB of position attribute in model space (dump only; ignored on pack).</summary>
    [JsonPropertyName("bounds")]
    public TimbermeshJsonBounds? Bounds { get; set; }

    [JsonPropertyName("triangleCount")]
    public int TriangleCount { get; set; }

    [JsonPropertyName("meshes")]
    public List<TimbermeshJsonMesh> Meshes { get; set; } = [];

    /// <summary>Vertex attributes keyed by name (position, normal, uv0, color, ...).</summary>
    [JsonPropertyName("attributes")]
    public Dictionary<string, TimbermeshJsonAttribute> Attributes { get; set; } = new(StringComparer.Ordinal);

    [JsonPropertyName("nodeAnimations")]
    public List<TimbermeshJsonNodeAnimation> NodeAnimations { get; set; } = [];

    [JsonPropertyName("vertexAnimations")]
    public List<TimbermeshJsonVertexAnimation> VertexAnimations { get; set; } = [];
}

public class TimbermeshJsonMesh
{
    [JsonPropertyName("material")]
    public string Material { get; set; } = "";

    [JsonPropertyName("indices")]
    public List<int> Indices { get; set; } = [];
}

public class TimbermeshJsonAttribute
{
    public const string EncodingBase64 = "base64";
    public const string EncodingFloatArray = "float-array";

    [JsonPropertyName("scalarType")]
    public string ScalarType { get; set; } = nameof(Models.ScalarType.Float);

    [JsonPropertyName("dimension")]
    public int Dimension { get; set; }

    /// <summary><c>base64</c> (default, lossless) or <c>float-array</c> (decoded numbers).</summary>
    [JsonPropertyName("encoding")]
    public string Encoding { get; set; } = EncodingBase64;

    /// <summary>Base64-encoded raw little-endian attribute buffer when <see cref="Encoding"/> is base64.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>Flat float values when <see cref="Encoding"/> is float-array (length = vertexCount * dimension).</summary>
    [JsonPropertyName("values")]
    public List<float>? Values { get; set; }
}

public class TimbermeshJsonNodeAnimation
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("framerate")]
    public float Framerate { get; set; }

    [JsonPropertyName("frames")]
    public List<TimbermeshJsonNodeAnimationFrame> Frames { get; set; } = [];
}

public class TimbermeshJsonNodeAnimationFrame
{
    [JsonPropertyName("position")]
    public TimbermeshJsonVec3 Position { get; set; } = new();

    [JsonPropertyName("rotation")]
    public TimbermeshJsonQuat Rotation { get; set; } = new();

    [JsonPropertyName("scale")]
    public TimbermeshJsonVec3 Scale { get; set; } = new() { X = 1, Y = 1, Z = 1 };
}

public class TimbermeshJsonVertexAnimation
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("framerate")]
    public float Framerate { get; set; }

    [JsonPropertyName("animatedVertexCount")]
    public int AnimatedVertexCount { get; set; }

    [JsonPropertyName("frames")]
    public List<TimbermeshJsonVertexAnimationFrame> Frames { get; set; } = [];
}

public class TimbermeshJsonVertexAnimationFrame
{
    [JsonPropertyName("attributes")]
    public Dictionary<string, TimbermeshJsonAttribute> Attributes { get; set; } = new(StringComparer.Ordinal);
}

public class TimbermeshJsonVec3
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }
}

public class TimbermeshJsonQuat
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    [JsonPropertyName("w")]
    public float W { get; set; } = 1f;
}

public class TimbermeshJsonBounds
{
    [JsonPropertyName("min")]
    public TimbermeshJsonVec3 Min { get; set; } = new();

    [JsonPropertyName("max")]
    public TimbermeshJsonVec3 Max { get; set; } = new();
}
