namespace Timbermesh2Blender.Services;

using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class TimbermeshJsonService
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public static TimbermeshJsonDocument ToDocument(Model model, bool decodeVertices)
    {
        List<TimbermeshJsonNode> nodes = [];
        HashSet<string> materials = new(StringComparer.OrdinalIgnoreCase);
        var totalTriangles = 0;

        for (var i = 0; i < model.Nodes.Length; i++)
        {
            var node = model.Nodes[i];
            var jsonNode = ToJsonNode(node, i, decodeVertices);
            nodes.Add(jsonNode);
            totalTriangles += jsonNode.TriangleCount;

            foreach (var mesh in jsonNode.Meshes)
            {
                if (!string.IsNullOrWhiteSpace(mesh.Material))
                {
                    materials.Add(mesh.Material);
                }
            }
        }

        return new TimbermeshJsonDocument
        {
            Format = TimbermeshJsonDocument.FormatId,
            FormatVersion = TimbermeshJsonDocument.CurrentFormatVersion,
            Name = model.Name,
            ModelVersion = model.Version,
            Materials = materials.Order(StringComparer.OrdinalIgnoreCase).ToList(),
            NodeCount = nodes.Count,
            TotalTriangles = totalTriangles,
            Nodes = nodes,
        };
    }

    public static Model ToModel(TimbermeshJsonDocument document)
    {
        if (!string.Equals(document.Format, TimbermeshJsonDocument.FormatId, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(document.Format))
        {
            throw new InvalidDataException(
                $"Unsupported format '{document.Format}'. Expected '{TimbermeshJsonDocument.FormatId}'.");
        }

        if (document.FormatVersion > TimbermeshJsonDocument.CurrentFormatVersion)
        {
            throw new InvalidDataException(
                $"Unsupported formatVersion {document.FormatVersion}. Max supported is {TimbermeshJsonDocument.CurrentFormatVersion}.");
        }

        if (document.Nodes.Count == 0)
        {
            return new Model
            {
                Version = document.ModelVersion,
                Name = document.Name ?? "",
                Nodes = [],
            };
        }

        // Array order is the packed index order. Remap parent indices if dump indices were non-sequential
        // (e.g. after deleting nodes and leaving original index values).
        var ordered = document.Nodes;
        Dictionary<int, int> indexMap = [];
        for (var i = 0; i < ordered.Count; i++)
        {
            var declared = ordered[i].Index;
            // Prefer explicit index when unique; otherwise fall back to array position only for mapping sources.
            if (!indexMap.ContainsKey(declared))
            {
                indexMap[declared] = i;
            }
        }

        // Also map by array position so parents that already use 0..n-1 still work.
        for (var i = 0; i < ordered.Count; i++)
        {
            indexMap.TryAdd(i, i);
        }

        var nodes = new Node[ordered.Count];
        for (var i = 0; i < ordered.Count; i++)
        {
            nodes[i] = ToNode(ordered[i], i, indexMap);
        }

        return new Model
        {
            Version = document.ModelVersion,
            Name = document.Name ?? "",
            Nodes = nodes,
        };
    }

    public static async Task WriteDocumentAsync(TimbermeshJsonDocument document, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, document, SerializerOptions);
    }

    public static async Task<TimbermeshJsonDocument> ReadDocumentAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var document = await JsonSerializer.DeserializeAsync<TimbermeshJsonDocument>(stream, SerializerOptions);
        if (document is null)
        {
            throw new InvalidDataException($"Failed to parse timbermesh JSON: {path}");
        }

        return document;
    }

    public static string GetDumpOutputPath(string inputRoot, string outputRoot, string inputFilePath, bool flatten)
    {
        string relative;
        if (flatten || File.Exists(inputRoot))
        {
            relative = Path.GetFileNameWithoutExtension(inputFilePath) + ".timbermesh.json";
        }
        else
        {
            relative = Path.GetRelativePath(inputRoot, inputFilePath);
            relative = Path.ChangeExtension(relative, null);
            if (relative.EndsWith(".timbermesh", StringComparison.OrdinalIgnoreCase))
            {
                relative = relative[..^".timbermesh".Length];
            }

            relative += ".timbermesh.json";
        }

        return Path.GetFullPath(Path.Combine(outputRoot, relative));
    }

    public static string GetPackOutputPath(string inputRoot, string outputRoot, string inputFilePath, bool flatten)
    {
        string relative;
        if (flatten || File.Exists(inputRoot))
        {
            relative = StripJsonExtension(Path.GetFileName(inputFilePath));
        }
        else
        {
            relative = Path.GetRelativePath(inputRoot, inputFilePath);
            relative = StripJsonExtension(relative);
        }

        if (!relative.EndsWith(".timbermesh", StringComparison.OrdinalIgnoreCase))
        {
            relative = Path.ChangeExtension(relative, ".timbermesh") ?? relative + ".timbermesh";
        }

        return Path.GetFullPath(Path.Combine(outputRoot, relative));
    }

    static string StripJsonExtension(string path)
    {
        if (path.EndsWith(".timbermesh.json", StringComparison.OrdinalIgnoreCase))
        {
            return path[..^".json".Length];
        }

        if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return Path.ChangeExtension(path, ".timbermesh") ?? path;
        }

        return path;
    }

    static TimbermeshJsonNode ToJsonNode(Node node, int index, bool decodeVertices)
    {
        var meshes = node.Meshes.Select(static m => new TimbermeshJsonMesh
        {
            Material = m.Material ?? "",
            Indices = [.. m.Indices],
        }).ToList();

        var triangleCount = meshes.Sum(static m => m.Indices.Count / 3);
        Dictionary<string, TimbermeshJsonAttribute> attributes = new(StringComparer.Ordinal);

        foreach (var property in node.VertexProperties)
        {
            if (string.IsNullOrWhiteSpace(property.Name))
            {
                continue;
            }

            attributes[property.Name] = ToJsonAttribute(property, decodeVertices);
        }

        return new TimbermeshJsonNode
        {
            Index = index,
            Parent = node.Parent,
            Name = node.Name ?? "",
            Position = ToJsonVec3(node.Position),
            Rotation = ToJsonQuat(node.Rotation),
            Scale = ToJsonVec3(node.Scale),
            VertexCount = node.VertexCount,
            Bounds = TryComputeBounds(node),
            TriangleCount = triangleCount,
            Meshes = meshes,
            Attributes = attributes,
            NodeAnimations = node.NodeAnimations.Select(ToJsonNodeAnimation).ToList(),
            VertexAnimations = node.VertexAnimations.Select(a => ToJsonVertexAnimation(a, decodeVertices)).ToList(),
        };
    }

    static Node ToNode(TimbermeshJsonNode json, int packedIndex, Dictionary<int, int> indexMap)
    {
        var parent = json.Parent;
        if (parent >= 0)
        {
            if (!indexMap.TryGetValue(parent, out var mappedParent))
            {
                throw new InvalidDataException(
                    $"Node '{json.Name}' (packed index {packedIndex}) has parent {parent}, which is not present in the document.");
            }

            parent = mappedParent;
        }

        if (parent == packedIndex)
        {
            throw new InvalidDataException(
                $"Node '{json.Name}' (packed index {packedIndex}) cannot be its own parent.");
        }

        List<VertexProperty> properties = [];
        foreach (var (name, attribute) in json.Attributes)
        {
            properties.Add(ToVertexProperty(name, attribute, json.VertexCount));
        }

        return new Node
        {
            Parent = parent,
            Name = json.Name ?? "",
            Position = ToVec3(json.Position),
            Rotation = ToQuat(json.Rotation),
            Scale = ToVec3(json.Scale ?? new TimbermeshJsonVec3 { X = 1, Y = 1, Z = 1 }),
            VertexCount = json.VertexCount,
            VertexProperties = properties,
            Meshes = json.Meshes.Select(static m => new Mesh
            {
                Material = m.Material ?? "",
                Indices = [.. m.Indices],
            }).ToList(),
            NodeAnimations = json.NodeAnimations.Select(ToNodeAnimation).ToList(),
            VertexAnimations = json.VertexAnimations.Select(ToVertexAnimation).ToList(),
        };
    }

    static TimbermeshJsonAttribute ToJsonAttribute(VertexProperty property, bool decodeVertices)
    {
        var scalarType = property.ScalarType;
        var dimension = property.ScalarTypeDimension;
        var data = property.Data ?? [];

        if (decodeVertices && scalarType == ScalarType.Float && dimension > 0 && data.Length > 0)
        {
            var floatCount = data.Length / sizeof(float);
            List<float> values = new(floatCount);
            for (var i = 0; i < floatCount; i++)
            {
                values.Add(BitConverter.ToSingle(data, i * sizeof(float)));
            }

            return new TimbermeshJsonAttribute
            {
                ScalarType = scalarType.ToString(),
                Dimension = dimension,
                Encoding = TimbermeshJsonAttribute.EncodingFloatArray,
                Values = values,
            };
        }

        return new TimbermeshJsonAttribute
        {
            ScalarType = scalarType.ToString(),
            Dimension = dimension,
            Encoding = TimbermeshJsonAttribute.EncodingBase64,
            Data = Convert.ToBase64String(data),
        };
    }

    static VertexProperty ToVertexProperty(string name, TimbermeshJsonAttribute attribute, int vertexCount)
    {
        var scalarType = ParseScalarType(attribute.ScalarType);
        var dimension = attribute.Dimension;
        var encoding = attribute.Encoding ?? TimbermeshJsonAttribute.EncodingBase64;
        byte[] data;

        if (string.Equals(encoding, TimbermeshJsonAttribute.EncodingFloatArray, StringComparison.OrdinalIgnoreCase))
        {
            if (attribute.Values is null)
            {
                throw new InvalidDataException($"Attribute '{name}' uses float-array encoding but has no values.");
            }

            data = new byte[attribute.Values.Count * sizeof(float)];
            for (var i = 0; i < attribute.Values.Count; i++)
            {
                BitConverter.TryWriteBytes(data.AsSpan(i * sizeof(float)), attribute.Values[i]);
            }
        }
        else if (string.Equals(encoding, TimbermeshJsonAttribute.EncodingBase64, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(encoding))
        {
            data = string.IsNullOrEmpty(attribute.Data)
                ? []
                : Convert.FromBase64String(attribute.Data);
        }
        else
        {
            throw new InvalidDataException(
                $"Attribute '{name}' has unknown encoding '{encoding}'. Use '{TimbermeshJsonAttribute.EncodingBase64}' or '{TimbermeshJsonAttribute.EncodingFloatArray}'.");
        }

        if (vertexCount > 0 && dimension > 0 && scalarType == ScalarType.Float)
        {
            var expected = vertexCount * dimension * sizeof(float);
            if (data.Length > 0 && data.Length != expected)
            {
                Console.Error.WriteLine(
                    $"Warning: attribute '{name}' data length {data.Length} does not match vertexCount*dimension*4 = {expected}.");
            }
        }

        return new VertexProperty
        {
            Name = name,
            ScalarType = scalarType,
            ScalarTypeDimension = dimension,
            Data = data,
        };
    }

    static TimbermeshJsonNodeAnimation ToJsonNodeAnimation(NodeAnimation animation) => new()
    {
        Name = animation.Name ?? "",
        Framerate = animation.Framerate,
        Frames = animation.Frames.Select(static f => new TimbermeshJsonNodeAnimationFrame
        {
            Position = ToJsonVec3(f.Position),
            Rotation = ToJsonQuat(f.Rotation),
            Scale = ToJsonVec3(f.Scale),
        }).ToList(),
    };

    static NodeAnimation ToNodeAnimation(TimbermeshJsonNodeAnimation animation) => new()
    {
        Name = animation.Name ?? "",
        Framerate = animation.Framerate,
        Frames = animation.Frames.Select(static f => new NodeAnimationFrame
        {
            Position = ToVec3(f.Position),
            Rotation = ToQuat(f.Rotation),
            Scale = ToVec3(f.Scale ?? new TimbermeshJsonVec3 { X = 1, Y = 1, Z = 1 }),
        }).ToList(),
    };

    static TimbermeshJsonVertexAnimation ToJsonVertexAnimation(VertexAnimation animation, bool decodeVertices) => new()
    {
        Name = animation.Name ?? "",
        Framerate = animation.Framerate,
        AnimatedVertexCount = animation.AnimatedVertexCount,
        Frames = animation.Frames.Select(frame =>
        {
            Dictionary<string, TimbermeshJsonAttribute> attributes = new(StringComparer.Ordinal);
            foreach (var property in frame.VertexProperties)
            {
                if (string.IsNullOrWhiteSpace(property.Name))
                {
                    continue;
                }

                attributes[property.Name] = ToJsonAttribute(property, decodeVertices);
            }

            return new TimbermeshJsonVertexAnimationFrame { Attributes = attributes };
        }).ToList(),
    };

    static VertexAnimation ToVertexAnimation(TimbermeshJsonVertexAnimation animation) => new()
    {
        Name = animation.Name ?? "",
        Framerate = animation.Framerate,
        AnimatedVertexCount = animation.AnimatedVertexCount,
        Frames = animation.Frames.Select(frame =>
        {
            List<VertexProperty> properties = [];
            foreach (var (name, attribute) in frame.Attributes)
            {
                properties.Add(ToVertexProperty(name, attribute, animation.AnimatedVertexCount));
            }

            return new VertexAnimationFrame { VertexProperties = properties };
        }).ToList(),
    };

    static TimbermeshJsonBounds? TryComputeBounds(Node node)
    {
        var property = node.VertexProperties.FirstOrDefault(static p =>
            string.Equals(p.Name, "position", StringComparison.OrdinalIgnoreCase));
        if (property is null || property.ScalarType != ScalarType.Float || property.ScalarTypeDimension != 3)
        {
            return null;
        }

        var data = property.Data;
        if (data is null || data.Length < 12 || node.VertexCount <= 0)
        {
            return null;
        }

        var count = Math.Min(node.VertexCount, data.Length / 12);
        if (count <= 0)
        {
            return null;
        }

        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var minZ = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;
        var maxZ = float.NegativeInfinity;

        for (var i = 0; i < count; i++)
        {
            var offset = i * 12;
            var x = BitConverter.ToSingle(data, offset);
            var y = BitConverter.ToSingle(data, offset + 4);
            var z = BitConverter.ToSingle(data, offset + 8);

            if (x < minX) { minX = x; }
            if (y < minY) { minY = y; }
            if (z < minZ) { minZ = z; }
            if (x > maxX) { maxX = x; }
            if (y > maxY) { maxY = y; }
            if (z > maxZ) { maxZ = z; }
        }

        if (!float.IsFinite(minX))
        {
            return null;
        }

        return new TimbermeshJsonBounds
        {
            Min = new TimbermeshJsonVec3 { X = minX, Y = minY, Z = minZ },
            Max = new TimbermeshJsonVec3 { X = maxX, Y = maxY, Z = maxZ },
        };
    }

    static ScalarType ParseScalarType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ScalarType.Float;
        }

        if (Enum.TryParse<ScalarType>(value, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            && Enum.IsDefined(typeof(ScalarType), number))
        {
            return (ScalarType)number;
        }

        throw new InvalidDataException($"Unknown scalarType '{value}'.");
    }

    static TimbermeshJsonVec3 ToJsonVec3(Vector3Float? value) => value is null
        ? new TimbermeshJsonVec3()
        : new TimbermeshJsonVec3 { X = value.X, Y = value.Y, Z = value.Z };

    static TimbermeshJsonQuat ToJsonQuat(QuaternionFloat? value) => value is null
        ? new TimbermeshJsonQuat()
        : new TimbermeshJsonQuat { X = value.X, Y = value.Y, Z = value.Z, W = value.W };

    static Vector3Float ToVec3(TimbermeshJsonVec3? value) => value is null
        ? new Vector3Float()
        : new Vector3Float { X = value.X, Y = value.Y, Z = value.Z };

    static QuaternionFloat ToQuat(TimbermeshJsonQuat? value) => value is null
        ? new QuaternionFloat()
        : new QuaternionFloat { X = value.X, Y = value.Y, Z = value.Z, W = value.W };
}
