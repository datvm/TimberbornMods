namespace TimberPipes.Models;

public static class DirectedPipeOrient
{
    public static PipePortState State(PipePortSpec spec, bool flipped, bool keepFlowWhenFlipped = false)
        => flipped && !keepFlowWhenFlipped ? spec.State.Flipped() : spec.State;
}
