namespace MoreBuildingRenovations.Components;

public record InventorRenovationSpec : ComponentSpec;

[AddTemplateModule2(typeof(InventorRenovationSpec))]
public class InventorRenovationBonus(ScienceService scienceService) : BaseComponent, IAwakableComponent
{

    int extraPoints = 0;
    Manufactory man = null!;

    public void Awake()
    {
        man = GetComponent<Manufactory>();
    }

    public void AddExtraScience(int extraPoints)
    {
        this.extraPoints = extraPoints;
        man.ProductionFinished += OnProductionFinished;
    }

    void OnProductionFinished(object sender, EventArgs e)
    {
        if (extraPoints == 0) { return; }

        var extra = man.CurrentRecipe.ProducedSciencePoints * extraPoints;
        if (extra > 0)
        {
            scienceService.AddPoints(extra);
        }
    }

    public void Deactivate()
    {
        extraPoints = 0;
        man.ProductionFinished -= OnProductionFinished;
    }

}
