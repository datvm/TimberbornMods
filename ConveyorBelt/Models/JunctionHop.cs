namespace ConveyorBelt.Models;

public readonly record struct JunctionHop(string Id, Direction3D Direction, float Start, float End)
{
    public bool Shows(float progress) => progress >= Start && progress < End;

    public float Along(float progress)
    {
        if (End <= Start || progress >= End)
        {
            return 1f;
        }

        var t = (progress - Start) / (End - Start);
        if (t < 0f)
        {
            return 0f;
        }

        return t;
    }
}
