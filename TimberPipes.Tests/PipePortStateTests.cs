namespace TimberPipes.Tests;

public class PipePortStateTests
{
    [Fact]
    public void FlipSwapsOneWayPorts()
    {
        Assert.Equal(PipePortState.OpenOut, PipePortState.OpenIn.Flipped());
        Assert.Equal(PipePortState.OpenIn, PipePortState.OpenOut.Flipped());
        Assert.Equal(PipePortState.Open, PipePortState.Open.Flipped());
        Assert.Equal(PipePortState.Closed, PipePortState.Closed.Flipped());
    }

    [Fact]
    public void PauseClosesValve()
    {
        Assert.Equal(PipePortState.Closed, PipePortState.OpenIn.WithPause(true, true));
        Assert.Equal(PipePortState.Closed, PipePortState.OpenOut.WithPause(true, true));
        Assert.Equal(PipePortState.Closed, PipePortState.Open.WithPause(true, true));
    }

    [Fact]
    public void PauseLeavesPumpOpen()
    {
        Assert.Equal(PipePortState.OpenIn, PipePortState.OpenIn.WithPause(true, false));
        Assert.Equal(PipePortState.OpenOut, PipePortState.OpenOut.WithPause(true, false));
    }

    [Fact]
    public void PauseLeavesPipeOpen()
    {
        Assert.Equal(PipePortState.Open, PipePortState.Open.WithPause(true, false));
    }

    [Fact]
    public void ResumeKeepsValveOriented()
    {
        Assert.Equal(PipePortState.OpenIn, PipePortState.OpenIn.WithPause(false, true));
        Assert.Equal(PipePortState.OpenOut, PipePortState.OpenOut.WithPause(false, true));
    }
}
