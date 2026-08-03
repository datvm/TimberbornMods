namespace Timbermesh2Blender.Services;

using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

public class BlenderExportService(TextureService textureService)
{
    public Task ExportAsync(TimbermeshFile file, string outputGlbPath)
    {
        var directory = Path.GetDirectoryName(outputGlbPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var scene = BuildScene(file);
        var model = scene.ToGltf2();
        model.SaveGLB(outputGlbPath);
        return Task.CompletedTask;
    }

    public static string GetOutputPath(string inputRoot, string outputRoot, TimbermeshFile file, bool flatten = false)
    {
        string relative;
        if (flatten || File.Exists(inputRoot))
        {
            relative = Path.GetFileNameWithoutExtension(file.FilePath) + ".glb";
        }
        else
        {
            relative = Path.GetRelativePath(inputRoot, file.FilePath);
            relative = Path.ChangeExtension(relative, ".glb");
        }

        return Path.GetFullPath(Path.Combine(outputRoot, relative));
    }

    SceneBuilder BuildScene(TimbermeshFile file)
    {
        var scene = new SceneBuilder();
        var model = file.Model;
        var materialCache = new Dictionary<string, MaterialBuilder>(StringComparer.OrdinalIgnoreCase);
        var nodeBuilders = new NodeBuilder[model.Nodes.Length];

        for (var i = 0; i < model.Nodes.Length; i++)
        {
            var node = model.Nodes[i];
            var builder = new NodeBuilder(string.IsNullOrWhiteSpace(node.Name) ? $"Node_{i}" : node.Name)
                .WithLocalTranslation(ToGltfPosition(node.Position))
                .WithLocalRotation(ToGltfRotation(node.Rotation))
                .WithLocalScale(ToGltfScale(node.Scale));

            ApplyNodeAnimations(builder, node);
            nodeBuilders[i] = builder;
        }

        for (var i = 0; i < model.Nodes.Length; i++)
        {
            var parent = model.Nodes[i].Parent;
            if (parent >= 0 && parent < nodeBuilders.Length)
            {
                nodeBuilders[parent].AddNode(nodeBuilders[i]);
            }
            else
            {
                scene.AddNode(nodeBuilders[i]);
            }
        }

        for (var i = 0; i < model.Nodes.Length; i++)
        {
            var node = model.Nodes[i];
            if (node.VertexCount <= 0 || node.Meshes.Count == 0)
            {
                continue;
            }

            var mesh = BuildMesh(node, materialCache);
            if (mesh is null)
            {
                continue;
            }

            scene.AddRigidMesh(mesh, nodeBuilders[i]);
        }

        return scene;
    }

    MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>? BuildMesh(
        Node node,
        Dictionary<string, MaterialBuilder> materialCache)
    {
        var positions = ReadVector3(node, "position");
        if (positions.Length == 0)
        {
            return null;
        }

        var normals = ReadVector3(node, "normal");
        var uvs = ReadVector2(node, "uv0");
        var colors = ReadVector4(node, "color");

        var mesh = new MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>(
            string.IsNullOrWhiteSpace(node.Name) ? "Mesh" : node.Name);

        foreach (var submesh in node.Meshes)
        {
            if (submesh.Indices.Count < 3)
            {
                continue;
            }

            var material = GetOrCreateMaterial(submesh.Material, materialCache);
            var primitive = mesh.UsePrimitive(material);

            for (var i = 0; i + 2 < submesh.Indices.Count; i += 3)
            {
                var i0 = submesh.Indices[i];
                var i1 = submesh.Indices[i + 1];
                var i2 = submesh.Indices[i + 2];

                if (!IsValidIndex(i0, positions.Length)
                    || !IsValidIndex(i1, positions.Length)
                    || !IsValidIndex(i2, positions.Length))
                {
                    continue;
                }

                // Reverse winding when converting Unity (left-handed) to glTF (right-handed).
                primitive.AddTriangle(
                    CreateVertex(i0, positions, normals, uvs, colors),
                    CreateVertex(i2, positions, normals, uvs, colors),
                    CreateVertex(i1, positions, normals, uvs, colors));
            }
        }

        return mesh;
    }

    MaterialBuilder GetOrCreateMaterial(string materialName, Dictionary<string, MaterialBuilder> cache)
    {
        var key = string.IsNullOrWhiteSpace(materialName) ? "Default" : materialName;
        if (cache.TryGetValue(key, out var existing))
        {
            return existing;
        }

        var builder = new MaterialBuilder(key).WithMetallicRoughnessShader();

        if (textureService.TryGetMaterial(key, out var unityMaterial))
        {
            ApplyUnityMaterial(builder, unityMaterial);
        }
        else
        {
            builder.WithBaseColor(new Vector4(0.75f, 0.75f, 0.75f, 1f));
            builder.WithMetallicRoughness(0f, 0.8f);
        }

        cache[key] = builder;
        return builder;
    }

    static void ApplyUnityMaterial(MaterialBuilder builder, UnityMaterial material)
    {
        var baseColor = new Vector4(
            material.BaseColorFactor.R,
            material.BaseColorFactor.G,
            material.BaseColorFactor.B,
            material.BaseColorFactor.A);

        if (material.BaseColorMap is { HasFile: true, FilePath: { } basePath })
        {
            builder.WithBaseColor(ImageBuilder.From(new SharpGLTF.Memory.MemoryImage(basePath), material.Name + "_BaseColor"), baseColor);
        }
        else
        {
            builder.WithBaseColor(baseColor);
        }

        // Unity metallic-gloss packing differs from glTF metallic-roughness channels,
        // so use scalar factors for a stable preview. Maps can still be painted/replaced in Blender.
        builder.WithMetallicRoughness(material.MetallicFactor, material.RoughnessFactor);

        if (material.NormalMap is { HasFile: true, FilePath: { } normalPath })
        {
            builder.WithNormal(ImageBuilder.From(new SharpGLTF.Memory.MemoryImage(normalPath), material.Name + "_Normal"), material.NormalScale);
        }

        if (material.OcclusionMap is { HasFile: true, FilePath: { } occlusionPath })
        {
            builder.WithOcclusion(ImageBuilder.From(new SharpGLTF.Memory.MemoryImage(occlusionPath), material.Name + "_Occlusion"), material.OcclusionStrength);
        }

        var emission = material.EmissionFactor;
        if (emission.R > 0f || emission.G > 0f || emission.B > 0f || material.EmissionMap is { HasFile: true })
        {
            ImageBuilder? emissionImage = material.EmissionMap is { HasFile: true, FilePath: { } emissionPath }
                ? ImageBuilder.From(new SharpGLTF.Memory.MemoryImage(emissionPath), material.Name + "_Emission")
                : null;

            builder.WithEmissive(emissionImage, new Vector3(emission.R, emission.G, emission.B));
        }

        if (material.AlphaClip)
        {
            builder.WithAlpha(AlphaMode.MASK, material.AlphaCutoff);
        }
    }

    static void ApplyNodeAnimations(NodeBuilder builder, Node node)
    {
        foreach (var animation in node.NodeAnimations)
        {
            if (animation.Frames.Count == 0 || animation.Framerate <= 0f)
            {
                continue;
            }

            var trackName = string.IsNullOrWhiteSpace(animation.Name) ? "Animation" : animation.Name;
            Dictionary<float, Vector3> positions = [];
            Dictionary<float, Quaternion> rotations = [];
            Dictionary<float, Vector3> scales = [];

            for (var frameIndex = 0; frameIndex < animation.Frames.Count; frameIndex++)
            {
                var frame = animation.Frames[frameIndex];
                var time = frameIndex / animation.Framerate;

                if (frame.Position is not null)
                {
                    positions[time] = ToGltfPosition(frame.Position);
                }

                if (frame.Rotation is not null)
                {
                    rotations[time] = ToGltfRotation(frame.Rotation);
                }

                if (frame.Scale is not null)
                {
                    scales[time] = ToGltfScale(frame.Scale);
                }
            }

            if (positions.Count > 0)
            {
                builder.WithLocalTranslation(trackName, positions);
            }

            if (rotations.Count > 0)
            {
                builder.WithLocalRotation(trackName, rotations);
            }

            if (scales.Count > 0)
            {
                builder.WithLocalScale(trackName, scales);
            }
        }
    }

    static VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty> CreateVertex(
        int index,
        Vector3[] positions,
        Vector3[] normals,
        Vector2[] uvs,
        Vector4[] colors)
    {
        var position = ToGltfPosition(positions[index]);
        var normal = index < normals.Length
            ? ToGltfNormal(normals[index])
            : Vector3.UnitY;
        if (normal.LengthSquared() < 1e-8f)
        {
            normal = Vector3.UnitY;
        }
        else
        {
            normal = Vector3.Normalize(normal);
        }

        var uv = index < uvs.Length ? uvs[index] : Vector2.Zero;
        var color = index < colors.Length ? colors[index] : Vector4.One;

        return new VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>(
            new VertexPositionNormal(position, normal),
            new VertexColor1Texture1(color, uv));
    }

    static bool IsValidIndex(int index, int count) => index >= 0 && index < count;

    static Vector3 ToGltfPosition(Vector3Float value) => ToGltfPosition(new Vector3(value.X, value.Y, value.Z));
    static Vector3 ToGltfPosition(Vector3 value) => new(-value.X, value.Y, value.Z);

    static Vector3 ToGltfNormal(Vector3 value) => new(-value.X, value.Y, value.Z);

    static Vector3 ToGltfScale(Vector3Float value) => new(value.X, value.Y, value.Z);

    static Quaternion ToGltfRotation(QuaternionFloat value)
        => ToGltfRotation(new Quaternion(value.X, value.Y, value.Z, value.W));

    static Quaternion ToGltfRotation(Quaternion value)
        // Unity LH Y-up -> glTF RH Y-up (mirror X): negate Y/Z components of quaternion.
        => Quaternion.Normalize(new Quaternion(value.X, -value.Y, -value.Z, value.W));

    static Vector3[] ReadVector3(Node node, string propertyName)
    {
        var property = GetProperty(node, propertyName);
        if (property is null || node.VertexCount <= 0)
        {
            return [];
        }

        var values = new Vector3[node.VertexCount];
        for (var i = 0; i < node.VertexCount; i++)
        {
            var offset = i * 12;
            values[i] = new Vector3(
                BitConverter.ToSingle(property.Data, offset),
                BitConverter.ToSingle(property.Data, offset + 4),
                BitConverter.ToSingle(property.Data, offset + 8));
        }

        return values;
    }

    static Vector2[] ReadVector2(Node node, string propertyName)
    {
        var property = GetProperty(node, propertyName);
        if (property is null || node.VertexCount <= 0)
        {
            return [];
        }

        var values = new Vector2[node.VertexCount];
        for (var i = 0; i < node.VertexCount; i++)
        {
            var offset = i * 8;
            values[i] = new Vector2(
                BitConverter.ToSingle(property.Data, offset),
                BitConverter.ToSingle(property.Data, offset + 4));
        }

        return values;
    }

    static Vector4[] ReadVector4(Node node, string propertyName)
    {
        var property = GetProperty(node, propertyName);
        if (property is null || node.VertexCount <= 0)
        {
            return [];
        }

        var values = new Vector4[node.VertexCount];
        for (var i = 0; i < node.VertexCount; i++)
        {
            var offset = i * 16;
            values[i] = new Vector4(
                BitConverter.ToSingle(property.Data, offset),
                BitConverter.ToSingle(property.Data, offset + 4),
                BitConverter.ToSingle(property.Data, offset + 8),
                BitConverter.ToSingle(property.Data, offset + 12));
        }

        return values;
    }

    static VertexProperty? GetProperty(Node node, string propertyName)
        => node.VertexProperties.FirstOrDefault(p => p.Name == propertyName);
}
