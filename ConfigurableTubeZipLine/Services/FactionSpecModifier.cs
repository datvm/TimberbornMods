namespace ConfigurableTubeZipLine.Services;

public class FactionSpecModifier : BaseBlueprintModifier<FactionSpec>
{

    static FactionSpec AppendMaterial(FactionSpec original, string material) =>
        original with
        {
            MaterialCollectionIds = [.. original.MaterialCollectionIds.Union([material])]
        };

    public override IEnumerable<EditableBlueprint> Modify(IEnumerable<EditableBlueprint> blueprints)
    {
        foreach (var bp in blueprints)
        {
            for (int i = 0; i < bp.Specs.Count; i++)
            {
                var spec = bp.Specs[i];
                if (spec is FactionSpec fs)
                {
                    switch (fs.Id)
                    {
                        case "Folktails" when MSettings.TubewayForFolktails:
                            bp.Specs[i] = AppendMaterial(fs, "IronTeeth");
                            break;
                        case "IronTeeth" when MSettings.ZiplineForIronTeeth:
                            bp.Specs[i] = AppendMaterial(fs, "Folktails");
                            break;
                    }

                    break;
                }
            }

            yield return bp;
        }
    }
}
