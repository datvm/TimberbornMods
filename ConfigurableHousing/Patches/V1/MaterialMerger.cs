namespace ConfigurableHousing.Patches.V1;

public class MaterialMerger(MSettings s, ISpecService specs) : IMaterialCollectionIdsProvider
{

    public IEnumerable<string> GetMaterialCollectionIds() => s.AddOtherFaction.Value ? GetAllMaterialIds : [];

    string[] GetAllMaterialIds => [.. specs.GetSpecs<FactionSpec>()
        .SelectMany(q => q.MaterialCollectionIds)
        .Distinct()];

}
