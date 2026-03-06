namespace MoreHttpApi.Shared;

public readonly record struct HttpSerializableInts(int X = 0, int Y = 0, int Z = 0, int W = 0)
{   
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
}

public readonly record struct HttpSerializableFloats(float X = 0f, float Y = 0f, float Z = 0f, float W = 0f)
{
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
}