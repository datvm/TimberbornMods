namespace ConfigurableTubeZipLine.Services;

public class FactionSpecModifier : BaseSpecModifier<FactionSpec>
{

    protected override IEnumerable<NamedSpec<FactionSpec>> Modify(IEnumerable<NamedSpec<FactionSpec>> specs)
    {
        foreach (var namedSpec in specs)
        {
            switch (namedSpec.Spec.Id)
            {
                case "Folktails" when MSettings.TubewayForFolktails:
                    yield return AppendMaterial(namedSpec, "IronTeeth");
                    break;
                case "IronTeeth" when MSettings.ZiplineForIronTeeth:
                    yield return AppendMaterial(namedSpec, "Folktails");
                    break;
                default:
                    yield return namedSpec;
                    break;
            }
        }
    }

    static NamedSpec<FactionSpec> AppendMaterial(in NamedSpec<FactionSpec> original, string material) =>
        original with
        {
            Spec = original.Spec with
            {
                MaterialCollectionIds = [.. original.Spec.MaterialCollectionIds.Union([material])]
            }
        };

}
