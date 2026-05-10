namespace WaterErosion.Services;

[BindSingleton]
public class BlockageService(
    EventBus eb,
    DestructionService destructionService,
    IBlockService block
) : ILoadableSingleton
{

    public static readonly ImmutableArray<string> BlockageTemplateNames = ["Blockage", "BlockageThreeQuarter", "BlockageHalf", "BlockageQuarter"];
    public static readonly int BlockageCount = BlockageTemplateNames.Length;

    readonly Dictionary<Vector3Int, BlockageObject> activeBlockages = [];
    readonly HashSet<BlockObject> activeBlockagesSets = [];

    public void Load()
    {
        eb.Register(this);
    }

    public bool TryGetBlockageAt(Vector3Int coords, out BlockageObject blockage) 
        => activeBlockages.TryGetValue(coords, out blockage);

    public void ErodeDirtAt(Vector3Int coords)
    {
        // Check if bottom is solid

    }

    public void ErodeBlockage(BlockageObject blockage, float value)
    {

    }

    bool CanSpawnBlockage()
    {
        return false;
    }

    [OnEvent]
    public void OnEntityFinished(EnteredFinishedStateEvent e)
    {
        var bo = e.BlockObject;
        var templateName = bo.GetTemplateName();
        if (BlockageTemplateNames.Contains(templateName))
        {
            activeBlockages[bo.Coordinates] = new(bo.Coordinates, templateName, BlockageTemplateNames.IndexOf(templateName));
            activeBlockagesSets.Add(bo);
        }
    }

    [OnEvent]
    public void OnEntityRemoved(ExitedFinishedStateEvent e)
    {
        var bo = e.BlockObject;
        if (activeBlockagesSets.Contains(bo))
        {
            activeBlockages.Remove(bo.Coordinates);
            activeBlockagesSets.Remove(bo);
        }
    }

    public readonly record struct BlockageObject(Vector3Int Coordinates, string TemplateName, int Order);
}
