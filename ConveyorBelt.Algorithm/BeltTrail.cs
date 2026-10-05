namespace ConveyorBelt.Algorithm;

public readonly record struct BeltSpan(string Id, float From, float To, bool Rest);

// One slice of a good's travel during the tick that just happened.
// Start and End are fractions of that tick. Rest stays visible after End.
public readonly record struct BeltTrail(string Id, float From, float To, float Start, float End, bool Rest)
{
    public static float Portion(float hours, float tickHours)
    {
        if (tickHours <= 0f)
        {
            return 1f;
        }

        var t = hours / tickHours;
        if (t < 0f)
        {
            return 0f;
        }

        if (t > 1f)
        {
            return 1f;
        }

        return t;
    }

    public static BeltTrail Slice(string id, float from, float to, float spentHours, float usedHours, float tickHours, bool rest)
        => new(id, from, to, Portion(spentHours, tickHours), Portion(spentHours + usedHours, tickHours), rest);

    public bool Shows(float progress)
    {
        if (progress < Start)
        {
            return false;
        }

        if (Rest)
        {
            return true;
        }

        return progress < End;
    }

    public float Along(float progress)
    {
        if (End <= Start || progress >= End)
        {
            return To;
        }

        var t = (progress - Start) / (End - Start);
        return From + (To - From) * t;
    }
}
