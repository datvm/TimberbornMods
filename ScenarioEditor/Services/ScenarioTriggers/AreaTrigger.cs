namespace ScenarioEditor.Services.ScenarioTriggers;

public class AreaTrigger : IScenarioTrigger
{
    public string NameKey { get; } = "LV.ScE.AreaTrigger";

#nullable disable
    TriggerAreaService triggerAreaService;
#nullable enable

    [Inject]
    public void Inject(TriggerAreaService triggerAreaService)
    {
        this.triggerAreaService = triggerAreaService;
    }

    public List<AreaDefinition> Areas { get; init; } = [];
    public GameObjectDefinition GameObjects { get; init; } = GameObjectDefinition.BlockObjectsOnly;
    public int MinimumObjectsCount { get; init; } = 1;

    public IReadOnlyList<EntityComponent>? LastResult { get; private set; }

    public bool Check(ScenarioEvent scenarioEvent)
    {
        LastResult = triggerAreaService.CheckForArea(this);
        return LastResult is not null;
    }
}

public readonly record struct AreaDefinition(Vector3Int Start, Vector3Int End);