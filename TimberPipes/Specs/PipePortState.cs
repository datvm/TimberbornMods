namespace TimberPipes.Specs;

public enum PipePortState
{
    Closed = 0,
    OpenOut = 1,
    OpenIn = 2,
    Open = OpenOut | OpenIn,
}

public static class PipePortStates
{
    public static PipePortState Flipped(this PipePortState state) => state switch
    {
        PipePortState.OpenIn => PipePortState.OpenOut,
        PipePortState.OpenOut => PipePortState.OpenIn,
        _ => state,
    };

    public static PipePortState WithPause(this PipePortState openState, bool paused, bool isValve)
    {
        if (paused && isValve)
        {
            return PipePortState.Closed;
        }

        return openState;
    }
}
