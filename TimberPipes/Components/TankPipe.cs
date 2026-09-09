namespace TimberPipes.Components;

[AddTemplateModule2(typeof(Stockpile))]
public class TankPipe : BaseComponent, IGeneratedBuildingPipeComponent
{
    public BuildingPipeSpec? GetBuildingPipeSpec()
    {
        if (GetComponent<StockpileSpec>() is not { WhitelistedGoodType: PipeFluids.LiquidGoodType })
        {
            return null;
        }

        return BuildingPipeLayout.FromBlockObject(GetComponent<BlockObject>(), PipePortState.Open);
    }
}
