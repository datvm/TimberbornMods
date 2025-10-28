
namespace TimbermeshMaterialPatcher;

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.IAnimation
public interface IAnimation
{
    string Name { get; }

    float Length { get; }
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.Mesh

[ProtoContract]
public class Mesh
{
    [ProtoMember(1)]
    public List<int> Indices { get; } = new List<int>();

    [ProtoMember(2)]
    public string Material { get; set; }
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.Model

[ProtoContract]
public class Model
{
    [ProtoMember(1)]
    public int Version { get; }

    [ProtoMember(2)]
    public string Name { get; }

    [ProtoMember(3)]
    public Node[] Nodes { get; }
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.Node

[ProtoContract]
public class Node
{
    [ProtoMember(1)]
    public int Parent { get; }

    [ProtoMember(2)]
    public string Name { get; }

    [ProtoMember(3)]
    public Vector3Float Position { get; } = new Vector3Float();

    [ProtoMember(4)]
    public QuaternionFloat Rotation { get; } = new QuaternionFloat();

    [ProtoMember(5)]
    public Vector3Float Scale { get; } = new Vector3Float();

    [ProtoMember(6)]
    public int VertexCount { get; }

    [ProtoMember(7)]
    public List<VertexProperty> VertexProperties { get; } = new List<VertexProperty>();

    [ProtoMember(8)]
    public List<Mesh> Meshes { get; } = new List<Mesh>();

    [ProtoMember(9)]
    public List<VertexAnimation> VertexAnimations { get; } = new List<VertexAnimation>();

    [ProtoMember(10)]
    public List<NodeAnimation> NodeAnimations { get; } = new List<NodeAnimation>();
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.NodeAnimation

[ProtoContract]
public class NodeAnimation : IAnimation
{
    [ProtoMember(1)]
    public string Name { get; }

    [ProtoMember(2)]
    public float Framerate { get; }

    [ProtoMember(3)]
    public List<NodeAnimationFrame> Frames { get; } = new List<NodeAnimationFrame>();

    public float Length => Frames.Count / Framerate;
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.NodeAnimationFrame

[ProtoContract]
public class NodeAnimationFrame
{
    [ProtoMember(1)]
    public Vector3Float Position { get; }

    [ProtoMember(2)]
    public QuaternionFloat Rotation { get; }

    [ProtoMember(3)]
    public Vector3Float Scale { get; }
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.QuaternionFloat

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

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.ScalarType
public enum ScalarType
{
    Unspecified,
    UnsignedByte,
    UnsignedInt,
    Int,
    Float,
    Double
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.Vector3Float

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

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.VertexAnimation
[ProtoContract]
public class VertexAnimation : IAnimation
{
    [ProtoMember(1)]
    public string Name { get; }

    [ProtoMember(2)]
    public float Framerate { get; }

    [ProtoMember(3)]
    public int AnimatedVertexCount { get; }

    [ProtoMember(4)]
    public List<VertexAnimationFrame> Frames { get; } = new List<VertexAnimationFrame>();

    public float Length => Frames.Count / Framerate;
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.VertexAnimationFrame
[ProtoContract]
public class VertexAnimationFrame
{
    [ProtoMember(1)]
    public List<VertexProperty> VertexProperties { get; } = new List<VertexProperty>();
}

// Timberborn.TimbermeshDTO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Timberborn.TimbermeshDTO.VertexProperty
[ProtoContract]
public class VertexProperty
{
    [ProtoMember(1)]
    public string Name { get; }

    [ProtoMember(2)]
    public ScalarType ScalarType { get; }

    [ProtoMember(3)]
    public int ScalarTypeDimension { get; }

    [ProtoMember(4)]
    public byte[] Data { get; }
}
