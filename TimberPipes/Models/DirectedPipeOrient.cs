namespace TimberPipes.Models;

public static class DirectedPipeOrient
{
    public static PipePortState State(PipePortSpec spec, bool flipped)
        => flipped ? spec.State.Flipped() : spec.State;

    public static bool HasVertical(PipePortSpec spec)
    {
        foreach (var d in spec.Directions)
        {
            if (d is Direction3D.Top or Direction3D.Bottom)
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryOutflow(PipePortSpec spec, bool flipped, out Direction3D direction)
    {
        direction = default;
        if (State(spec, flipped) != PipePortState.OpenOut)
        {
            return false;
        }

        foreach (var d in spec.Directions)
        {
            direction = d;
            return true;
        }

        return false;
    }
}
