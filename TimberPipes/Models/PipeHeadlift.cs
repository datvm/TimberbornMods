namespace TimberPipes.Models;

public static class PipeHeadlift
{
    public static float ExtraAt(BuildingPipe pipe)
    {
        if (pipe.Graph is null || pipe.IsContaminated || pipe.Graph.Contaminated)
        {
            return 0f;
        }

        return pipe.Graph.TransmittedLift.GetValueOrDefault(pipe);
    }
}
