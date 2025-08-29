namespace BuildingHP.Services;

public class RenovationSpecService(ISpecService specService, ILoc t) : ILoadableSingleton
{

    public FrozenDictionary<string, RenovationGroupSpec> Groups { get; private set; } = FrozenDictionary<string, RenovationGroupSpec>.Empty;
    public FrozenDictionary<string, RenovationSpec> Renovations { get; private set; } = FrozenDictionary<string, RenovationSpec>.Empty;
    
    public void Load()
    {
        Groups = specService.GetSpecs<RenovationGroupSpec>()
            .ToFrozenDictionary(x => x.Id);

        Renovations = specService.GetSpecs<RenovationSpec>()
            .ToFrozenDictionary(x => x.Id);
        foreach (var r in Renovations.Values)
        {
            try
            {
                r.Description = string.Format(t.T(r.DescLoc), [..r.Parameters])
                    .Replace("[Days]", r.Days.ToString("0.##"));
            }
            catch (Exception ex)
            {
                throw new FormatException("Error formatting description for renovation " + r.Id, ex);
            }
        }
    }

}
