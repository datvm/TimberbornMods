namespace TimberUi.Helpers;

public readonly record struct SerializableInts(int X, int Y, int Z, int W)
{
    public static readonly SerializableIntsSerializer Serializer = new();

    public SerializableInts(int x) : this(x, 0, 0, 0) { }
    public SerializableInts(int x, int y) : this(x, y, 0, 0) { }
    public SerializableInts(int x, int y, int z) : this(x, y, z, 0) { }

    public string Serialize() => $"{X};{Y};{Z};{W}";

    public static SerializableInts Deserialize(string serialized)
    {
        var parts = serialized.Split(';');
        return new(Parse(0), Parse(1), Parse(2), Parse(3));

        int Parse(int index) => index < parts.Length && int.TryParse(parts[index], out var value) ? value : 0;
    }

    public void Deconstruct(out int x, out int y, out int z, out int w)
    {
        x = X;
        y = Y;
        z = Z;
        w = W;
    }

    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public static implicit operator SerializableInts(Vector3Int vector) => new(vector.x, vector.y, vector.z);
    public static implicit operator Vector3Int(SerializableInts ints) => new(ints.X, ints.Y, ints.Z);

    public static implicit operator SerializableInts(Vector2Int vector) => new(vector.x, vector.y);
    public static implicit operator Vector2Int(SerializableInts ints) => new(ints.X, ints.Y);

    public static implicit operator SerializableInts(RectInt rect) => new(rect.x, rect.y, rect.width, rect.height);
    public static implicit operator RectInt(SerializableInts ints) => new(ints.X, ints.Y, ints.Z, ints.W);

    public class SerializableIntsSerializer : IValueSerializer<SerializableInts>
    {
        public void Serialize(SerializableInts value, IValueSaver valueSaver)
            => valueSaver.AsString(value.Serialize());

        public Obsoletable<SerializableInts> Deserialize(IValueLoader valueLoader)
            => SerializableInts.Deserialize(valueLoader.AsString());
    }

}
