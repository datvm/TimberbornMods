namespace TimberUi.Helpers;

public readonly record struct SerializableFloats(float X, float Y, float Z, float W)
{
    public static readonly SerializableFloatsSerializer Serializer = new();

    public SerializableFloats(float x) : this(x, 0f, 0f, 0f) { }
    public SerializableFloats(float x, float y) : this(x, y, 0f, 0f) { }
    public SerializableFloats(float x, float y, float z) : this(x, y, z, 0f) { }

    public string Serialize() => $"{X};{Y};{Z};{W}";

    public static SerializableFloats Deserialize(string serialized)
    {
        var parts = serialized.Split(';');
        return new(Parse(0), Parse(1), Parse(2), Parse(3));

        float Parse(int index) => index < parts.Length && float.TryParse(parts[index], out var value) ? value : 0f;
    }

    public void Deconstruct(out float x, out float y, out float z, out float w)
    {
        x = X;
        y = Y;
        z = Z;
        w = W;
    }

    public void Deconstruct(out float x, out float y, out float z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    public void Deconstruct(out float x, out float y)
    {
        x = X;
        y = Y;
    }

    public static implicit operator SerializableFloats(Vector3 v) => new(v.x, v.y, v.z);
    public static implicit operator Vector3(SerializableFloats f) => new(f.X, f.Y, f.Z);

    public static implicit operator SerializableFloats(Vector2 v) => new(v.x, v.y);
    public static implicit operator Vector2(SerializableFloats f) => new(f.X, f.Y);

    public static implicit operator SerializableFloats(Color c) => new(c.r, c.g, c.b, c.a);
    public static implicit operator Color(SerializableFloats f) => new(f.X, f.Y, f.Z, f.W);

    public static implicit operator SerializableFloats(Quaternion q) => new(q.x, q.y, q.z, q.w);
    public static implicit operator Quaternion(SerializableFloats f) => new(f.X, f.Y, f.Z, f.W);

    public static implicit operator SerializableFloats(Vector4 v) => new(v.x, v.y, v.z, v.w);
    public static implicit operator Vector4(SerializableFloats f) => new(f.X, f.Y, f.Z, f.W);

    public static implicit operator SerializableFloats(Rect r) => new(r.x, r.y, r.width, r.height);
    public static implicit operator Rect(SerializableFloats f) => new(f.X, f.Y, f.Z, f.W);

    public static implicit operator SerializableFloats(Vector3Int v) => new(v.x, v.y, v.z);
    public static implicit operator SerializableFloats(Vector2Int v) => new(v.x, v.y);
    public static implicit operator SerializableFloats(RectInt r) => new(r.x, r.y, r.width, r.height);
    public static implicit operator SerializableFloats(SerializableInts i) => new(i.X, i.Y, i.Z, i.W);

    public class SerializableFloatsSerializer : IValueSerializer<SerializableFloats>
    {

        public Obsoletable<SerializableFloats> Deserialize(IValueLoader valueLoader)
            => SerializableFloats.Deserialize(valueLoader.AsString());

        public void Serialize(SerializableFloats value, IValueSaver valueSaver)
            => valueSaver.AsString(value.Serialize());
    }

}
