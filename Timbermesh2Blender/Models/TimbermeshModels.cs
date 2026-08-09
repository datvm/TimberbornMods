namespace Timbermesh2Blender.Models;

public record TimbermeshFile(string FilePath, Model Model)
{
    public string Name { get; } = Path.GetFileNameWithoutExtension(FilePath);
}

public interface IAnimation
{
    string Name { get; }

    float Length { get; }
}

[ProtoContract]
public class Mesh
{
    [ProtoMember(1)]
    public List<int> Indices { get; set; } = [];

    [ProtoMember(2)]
    public string Material { get; set; } = "";
}

[ProtoContract]
public class Model
{
    [ProtoMember(1)]
    public int Version { get; set; }

    [ProtoMember(2)]
    public string Name { get; set; } = "";

    [ProtoMember(3)]
    public Node[] Nodes { get; set; } = [];
}

[ProtoContract]
public class Node
{
    [ProtoMember(1)]
    public int Parent { get; set; } = -1;

    [ProtoMember(2)]
    public string Name { get; set; } = "";

    [ProtoMember(3)]
    public Vector3Float Position { get; set; } = new();

    [ProtoMember(4)]
    public QuaternionFloat Rotation { get; set; } = new();

    [ProtoMember(5)]
    public Vector3Float Scale { get; set; } = new() { X = 1, Y = 1, Z = 1 };

    [ProtoMember(6)]
    public int VertexCount { get; set; }

    [ProtoMember(7)]
    public List<VertexProperty> VertexProperties { get; set; } = [];

    [ProtoMember(8)]
    public List<Mesh> Meshes { get; set; } = [];

    [ProtoMember(9)]
    public List<VertexAnimation> VertexAnimations { get; set; } = [];

    [ProtoMember(10)]
    public List<NodeAnimation> NodeAnimations { get; set; } = [];
}

[ProtoContract]
public class NodeAnimation : IAnimation
{
    [ProtoMember(1)]
    public string Name { get; set; } = "";

    [ProtoMember(2)]
    public float Framerate { get; set; }

    [ProtoMember(3)]
    public List<NodeAnimationFrame> Frames { get; set; } = [];

    public float Length => Framerate <= 0f ? 0f : Frames.Count / Framerate;
}

[ProtoContract]
public class NodeAnimationFrame
{
    [ProtoMember(1)]
    public Vector3Float Position { get; set; } = new();

    [ProtoMember(2)]
    public QuaternionFloat Rotation { get; set; } = new();

    [ProtoMember(3)]
    public Vector3Float Scale { get; set; } = new() { X = 1, Y = 1, Z = 1 };
}

[ProtoContract]
public class QuaternionFloat
{
    [ProtoMember(1)]
    public float X { get; set; }

    [ProtoMember(2)]
    public float Y { get; set; }

    [ProtoMember(3)]
    public float Z { get; set; }

    [ProtoMember(4)]
    public float W { get; set; } = 1f;
}

public enum ScalarType
{
    Unspecified,
    UnsignedByte,
    UnsignedInt,
    Int,
    Float,
    Double,
}

[ProtoContract]
public class Vector3Float
{
    [ProtoMember(1)]
    public float X { get; set; }

    [ProtoMember(2)]
    public float Y { get; set; }

    [ProtoMember(3)]
    public float Z { get; set; }
}

[ProtoContract]
public class VertexAnimation : IAnimation
{
    [ProtoMember(1)]
    public string Name { get; set; } = "";

    [ProtoMember(2)]
    public float Framerate { get; set; }

    [ProtoMember(3)]
    public int AnimatedVertexCount { get; set; }

    [ProtoMember(4)]
    public List<VertexAnimationFrame> Frames { get; set; } = [];

    public float Length => Framerate <= 0f ? 0f : Frames.Count / Framerate;
}

[ProtoContract]
public class VertexAnimationFrame
{
    [ProtoMember(1)]
    public List<VertexProperty> VertexProperties { get; set; } = [];
}

[ProtoContract]
public class VertexProperty
{
    [ProtoMember(1)]
    public string Name { get; set; } = "";

    [ProtoMember(2)]
    public ScalarType ScalarType { get; set; }

    [ProtoMember(3)]
    public int ScalarTypeDimension { get; set; }

    [ProtoMember(4)]
    public byte[] Data { get; set; } = [];
}
