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
    public List<int> Indices { get; } = [];

    [ProtoMember(2)]
    public string Material { get; set; } = "";
}

[ProtoContract]
public class Model
{
    [ProtoMember(1)]
    public int Version { get; }

    [ProtoMember(2)]
    public string Name { get; } = "";

    [ProtoMember(3)]
    public Node[] Nodes { get; } = [];
}

[ProtoContract]
public class Node
{
    [ProtoMember(1)]
    public int Parent { get; }

    [ProtoMember(2)]
    public string Name { get; } = "";

    [ProtoMember(3)]
    public Vector3Float Position { get; } = new();

    [ProtoMember(4)]
    public QuaternionFloat Rotation { get; } = new();

    [ProtoMember(5)]
    public Vector3Float Scale { get; } = new();

    [ProtoMember(6)]
    public int VertexCount { get; }

    [ProtoMember(7)]
    public List<VertexProperty> VertexProperties { get; } = [];

    [ProtoMember(8)]
    public List<Mesh> Meshes { get; } = [];

    [ProtoMember(9)]
    public List<VertexAnimation> VertexAnimations { get; } = [];

    [ProtoMember(10)]
    public List<NodeAnimation> NodeAnimations { get; } = [];
}

[ProtoContract]
public class NodeAnimation : IAnimation
{
    [ProtoMember(1)]
    public string Name { get; } = "";

    [ProtoMember(2)]
    public float Framerate { get; }

    [ProtoMember(3)]
    public List<NodeAnimationFrame> Frames { get; } = [];

    public float Length => Frames.Count / Framerate;
}

[ProtoContract]
public class NodeAnimationFrame
{
    [ProtoMember(1)]
    public Vector3Float Position { get; } = new();

    [ProtoMember(2)]
    public QuaternionFloat Rotation { get; } = new();

    [ProtoMember(3)]
    public Vector3Float Scale { get; } = new();
}

[ProtoContract]
public class QuaternionFloat
{
    [ProtoMember(1)]
    public float X { get; }

    [ProtoMember(2)]
    public float Y { get; }

    [ProtoMember(3)]
    public float Z { get; }

    [ProtoMember(4)]
    public float W { get; }
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
    public float X { get; }

    [ProtoMember(2)]
    public float Y { get; }

    [ProtoMember(3)]
    public float Z { get; }
}

[ProtoContract]
public class VertexAnimation : IAnimation
{
    [ProtoMember(1)]
    public string Name { get; } = "";

    [ProtoMember(2)]
    public float Framerate { get; }

    [ProtoMember(3)]
    public int AnimatedVertexCount { get; }

    [ProtoMember(4)]
    public List<VertexAnimationFrame> Frames { get; } = [];

    public float Length => Frames.Count / Framerate;
}

[ProtoContract]
public class VertexAnimationFrame
{
    [ProtoMember(1)]
    public List<VertexProperty> VertexProperties { get; } = [];
}

[ProtoContract]
public class VertexProperty
{
    [ProtoMember(1)]
    public string Name { get; } = "";

    [ProtoMember(2)]
    public ScalarType ScalarType { get; }

    [ProtoMember(3)]
    public int ScalarTypeDimension { get; }

    [ProtoMember(4)]
    public byte[] Data { get; } = [];
}
