namespace ConfigurableTubeZipLine.Services;

public class ZiplineCableInclinationModifier : BaseSpecModifier<ZiplineConnectionServiceSpec>
{
    
    protected override IEnumerable<NamedSpec<ZiplineConnectionServiceSpec>> Modify(IEnumerable<NamedSpec<ZiplineConnectionServiceSpec>> specs)
    {
        foreach (var s in specs)
        {
            yield return new(s.Name, s.Spec with
            {
                MaxCableInclination = MSettings.ZiplineMaxInclination
            });
        }
    }

}

public class ZiplineSpeedModifier : BaseSpecModifier<ZiplineCableNavMeshSpec>
{
    protected override IEnumerable<NamedSpec<ZiplineCableNavMeshSpec>> Modify(IEnumerable<NamedSpec<ZiplineCableNavMeshSpec>> specs)
    {
        foreach (var s in specs)
        {
            yield return new(s.Name, s.Spec with
            {
                CableUnitCost = ModHelpers.CalculateCost(MSettings.ZiplineSpeed)
            });
        }
    }
}