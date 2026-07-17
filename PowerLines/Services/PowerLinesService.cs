namespace PowerLines.Services;

[BindSingleton]
public class PowerLinesService(
    ISpecService specs,
    PowerLineConnectionService connService,
    ILoc t
) : ILoadableSingleton
{
    public readonly ILoc t = t;

    PowerLineSettingsSpec spec = null!;
    public int DefaultMaxConnections => spec.DefaultMaxConnections;
    public int DefaultMaxGeneratorConnections => spec.DefaultGeneratorMaxConnections;
    public float DefaultMaxConnectionLength => spec.DefaultMaxConnectionLength;
    public float DefaultMaxGeneratorConnectionLength => spec.DefaultMaxGeneratorConnectionLength;

    public void Load()
    {
        spec = specs.GetSingleSpec<PowerLineSettingsSpec>();
    }

    public void OnPowerLineDeleted(PowerLineComponent comp)
    {
        connService.DisconnectAllComponentConnections(comp);
    }

}
