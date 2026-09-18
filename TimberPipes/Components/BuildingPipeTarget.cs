namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingSpec))]
public class BuildingPipeTarget(ValvePipeService service) : BaseComponent, IInitializableEntity, IInitializablePreview
{
#nullable disable
    BlockObject blockObject;
    Inventories inventories;
    IBuildingConnectionProvider connectionProvider;
#nullable enable

    public BlockObject BlockObject => blockObject;
    public Inventories Inventories => inventories;

    public void InitializeEntity() => Init();

    public void InitializePreview() => Init();

    void Init()
    {
        if (blockObject)
        {
            return;
        }

        blockObject = GetComponent<BlockObject>();
        inventories = this.GetComponentOrNull<Inventories>();
        connectionProvider = GetEnabledComponent<IBuildingConnectionProvider>()
            ?? new DefaultBuildingConnectionProvider(this, service);
    }

    public IBuildingPipeConnection? TryConnecting(BuildingPipe pipe, PipePortDefinition approach, bool give)
        => connectionProvider?.TryConnecting(pipe, approach, give);
}
