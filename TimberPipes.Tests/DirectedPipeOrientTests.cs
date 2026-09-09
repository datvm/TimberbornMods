namespace TimberPipes.Tests;

public class DirectedPipeOrientTests
{
    [Fact]
    public void VerticalKeepsSpecDirection()
    {
        var bottom = new PipePortSpec { Directions = Directions3D.Bottom, State = PipePortState.OpenIn };
        var top = new PipePortSpec { Directions = Directions3D.Top, State = PipePortState.OpenOut };

        Assert.Equal(PipePortState.OpenIn, DirectedPipeOrient.State(bottom, false));
        Assert.Equal(PipePortState.OpenOut, DirectedPipeOrient.State(top, false));
    }

    [Fact]
    public void HorizontalFlipReversesFlow()
    {
        var down = new PipePortSpec { Directions = Directions3D.Down, State = PipePortState.OpenIn };
        var up = new PipePortSpec { Directions = Directions3D.Up, State = PipePortState.OpenOut };

        Assert.Equal(PipePortState.OpenOut, DirectedPipeOrient.State(down, true));
        Assert.Equal(PipePortState.OpenIn, DirectedPipeOrient.State(up, true));
        Assert.Equal(PipePortState.OpenIn, DirectedPipeOrient.State(down, false));
        Assert.Equal(PipePortState.OpenOut, DirectedPipeOrient.State(up, false));
    }

    [Fact]
    public void OccupancyOpenOutFlipsOnlyWhenFlipped()
    {
        var spec = new PipePortSpec
        {
            Directions = Directions3D.Down | Directions3D.Left | Directions3D.Up | Directions3D.Right | Directions3D.Bottom | Directions3D.Top,
            State = PipePortState.OpenOut,
        };

        Assert.Equal(PipePortState.OpenOut, DirectedPipeOrient.State(spec, false));
        Assert.Equal(PipePortState.OpenIn, DirectedPipeOrient.State(spec, true));
    }
}
