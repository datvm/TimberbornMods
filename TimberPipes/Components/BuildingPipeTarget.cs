namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingSpec))]
public class BuildingPipeTarget(ValvePipeService service) : BaseComponent, IInitializableEntity
{
#nullable disable
    BlockObject blockObject;
    Inventories inventories;
    IBuildingConnectionProvider connectionProvider;
#nullable enable

    public BlockObject BlockObject => blockObject;
    public Inventories Inventories => inventories;

    public void InitializeEntity()
    {
        blockObject = GetComponent<BlockObject>();
        inventories = this.GetComponentOrNull<Inventories>();
        connectionProvider = GetEnabledComponent<IBuildingConnectionProvider>()
            ?? new DefaultBuildingConnectionProvider(this, service);
    }

    public IBuildingPipeConnection? TryConnecting(BuildingPipe pipe, PipePortDefinition approach, bool give)
        => connectionProvider?.TryConnecting(pipe, approach, give);
}
