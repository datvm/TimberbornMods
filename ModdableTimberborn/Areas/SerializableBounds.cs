namespace ModdableTimberborn.Areas;

public readonly record struct SerializableBounds(SerializableFloats Center, SerializableFloats Size)
{
    public static implicit operator Bounds(SerializableBounds b) => new((Vector3)b.Center, (Vector3)b.Size);
    public static implicit operator SerializableBounds(Bounds b) => new((SerializableFloats)b.center, (SerializableFloats)b.size);
}

public readonly record struct SerializableBoundsInts(SerializableInts Position, SerializableInts Size)
{
    public static implicit operator BoundsInt(SerializableBoundsInts b) => new((Vector3Int)b.Position, (Vector3Int)b.Size);
    public static implicit operator SerializableBoundsInts(BoundsInt b) => new((SerializableInts)b.position, (SerializableInts)b.size);
}
