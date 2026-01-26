namespace ConfigurableFaction.Definitions;

public class PlantDef(PlantableSpec Plantable, Blueprint Blueprint, DataAggregatorService dataAggregator, ILoc t) : TemplateDefBase(Blueprint, dataAggregator, t)
{
    public PlantableSpec Plantable { get; } = Plantable;
    public string PlanterId => Plantable.ResourceGroup;

    public static PlantDef? Create(Blueprint bp, DataAggregatorService dataAggregator, ILoc t)
        => bp.CreateDefinition<PlantDef, PlantableSpec>(spec => new(spec, bp, dataAggregator, t));

    protected override void InitializeRequirements(DataAggregatorService dataAggregator)
    {
        throw new NotImplementedException();
    }
}
