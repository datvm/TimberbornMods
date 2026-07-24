namespace MoreBuildingRenovations.Components;

public record NumbercruncherRenovationSpec : ComponentSpec;

[AddTemplateModule2(typeof(NumbercruncherRenovationSpec))]
public class NumbercruncherWaterLevel(
    IThreadSafeWaterMap waterMap,
    ScienceService scienceService
) : BaseComponent
{
    Manufactory? man;
    Vector3Int topCellCoords;
    float extraScienceMul;

    public bool IsFullySubmerged => waterMap.CellIsUnderwater(topCellCoords);

    public void Activate(float extraScienceMul)
    {
        var bo = GetComponent<BlockObject>();
        var size = bo.Blocks.Size;
        topCellCoords = bo.TransformCoordinates(new Vector3Int(size.x / 2, size.y / 2, size.z - 1));

        this.extraScienceMul = extraScienceMul;

        man = GetComponent<Manufactory>();
        man.ProductionFinished += OnProductionFinished;
    }

    void OnProductionFinished(object sender, EventArgs e)
    {
        if (extraScienceMul == 0f || !IsFullySubmerged) { return; }

        var science = man!.CurrentRecipe.ProducedSciencePoints;
        if (science == 0) { return; }

        var extra = Mathf.FloorToInt(science * extraScienceMul);
        if (extra > 0)
        {
            scienceService.AddPoints(extra);
        }
    }

    public void Deactivate()
    {
        extraScienceMul = 0;
        man?.ProductionFinished -= OnProductionFinished;
    }

}
