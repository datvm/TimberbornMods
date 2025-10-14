namespace BlockObjectDebugger.Services;

public class BuildingsSpecModifier : BaseSpecModifier<PrefabGroupSpec>
{
    protected override IEnumerable<PrefabGroupSpec> Modify(IEnumerable<PrefabGroupSpec> specs)
    {
        foreach (var spec in specs)
        {
            if (spec.Id.Contains("Buildings"))
            {
                yield return spec with
                {
                    Paths = [..spec.Paths.Union(ModUtils.OccupationStrings.Select(ModUtils.GetBuildingPath))],
                };
            }
            else
            {
                yield return spec;
            }
        }
    }
}
