namespace WaterErosion.Helpers;

public static class WaterErosionUtils
{

    public static string SerializeFloatArray(ReadOnlySpan<float> values)
    {
        var bytes = new byte[values.Length * sizeof(float)];
        MemoryMarshal.AsBytes(values).CopyTo(bytes);

        if (!BitConverter.IsLittleEndian)
        {
            for (var i = 0; i < bytes.Length; i += sizeof(float))
            {
                bytes.AsSpan(i, sizeof(float)).Reverse();
            }
        }

        return Convert.ToBase64String(bytes);
    }

    public static float[] DeserializeFloatArray(string text)
    {
        var bytes = Convert.FromBase64String(text);

        if (bytes.Length % sizeof(float) != 0)
        {
            throw new FormatException("Invalid float array byte length.");
        }

        if (!BitConverter.IsLittleEndian)
        {
            for (var i = 0; i < bytes.Length; i += sizeof(float))
            {
                bytes.AsSpan(i, sizeof(float)).Reverse();
            }
        }

        var values = new float[bytes.Length / sizeof(float)];
        MemoryMarshal.Cast<byte, float>(bytes).CopyTo(values);

        return values;
    }

}
