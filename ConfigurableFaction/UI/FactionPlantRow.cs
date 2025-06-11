namespace ConfigurableFaction.UI;

public class FactionPlantRow : FactionPrefabRow
{
    protected override HashSet<NormalizedPrefabSpec> ExistingList => options.ExistingPlantablesPrefabName;
    protected override HashSet<string> GetList() => options.Plantables;

    protected override void OnPrefabSelected(NormalizedPrefabSpec spec, bool add)
    {
        if (add)
        {
            optionsService.AddPlantable(options, spec.Path);
        }
        else
        {
            optionsService.RemovePlantable(options, spec.Path);
        }
    }
}
