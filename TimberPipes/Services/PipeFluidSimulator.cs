namespace TimberPipes.Services;

[BindSingleton]
public class PipeFluidSimulator(
    PipeRegistry pipeRegistry,
    ISpecService specs
) : ITickableSingleton, ILoadableSingleton
{
    PipeSimulationSpec spec = null!;

    public void Load()
    {
        spec = specs.GetSingleSpec<PipeSimulationSpec>();
    }

    public void Tick()
    {
        foreach (var graph in pipeRegistry.Graphs)
        {
            Process(graph);
        }
    }

    void Process(PipeGraph graph)
    {

    }

}
