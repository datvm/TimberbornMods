namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class PumpToDumpProximityAchievement(
    EventBus eb,
    IBlockService blockService
) : EbAchievementBase(eb)
{
    public const string AchId = "LV.MA.PumpToDumpProximity";

    const string PumpTemplatePart = "WaterPump";
    const string DumpTemplatePart = "FluidDump";
    
    public override string Id => AchId;

    [OnEvent]
    public void OnBuildingFinished(EnteredFinishedStateEvent e)
    {
        var bo = e.BlockObject;
        var template = bo.GetTemplateName();

        var isPump = template.Contains(PumpTemplatePart);
        var isDump = !isPump && template.Contains(DumpTemplatePart);

        if (!isPump && !isDump)
        {
            return;
        }

        var adjacent = FindAdjacentBuilding(bo, isPump ? DumpTemplatePart : PumpTemplatePart);
        if (adjacent is null)
        {
            return;
        }

        Unlock();
    }

    BlockObject? FindAdjacentBuilding(BlockObject bo, string expectingTemplate)
    {
        HashSet<Vector3Int> visited = [];
        var blocks = bo.PositionedBlocks.GetAllBlocks();

        foreach (var b in blocks)
        {
            visited.Add(b.Coordinates);
        }

        foreach (var b in blocks)
        {
            foreach (var n in Deltas.Neighbors4Vector3Int)
            {
                var c = b.Coordinates + n;
                if (visited.Contains(c)) { continue; }
                visited.Add(c);

                foreach (var obj in blockService.GetObjectsAt(c))
                {
                    if (!obj.IsFinished) { continue; }

                    if (obj.GetTemplateName().Contains(expectingTemplate))
                    {
                        return obj;
                    }
                }
            }
        }

        return null;
    }

}
