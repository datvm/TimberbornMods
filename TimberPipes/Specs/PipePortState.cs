namespace TimberPipes.Specs;

public enum PipePortState
{
    Closed = 0,
    OpenOut = 1,
    OpenIn = 2,
    Open = OpenOut | OpenIn,
}
