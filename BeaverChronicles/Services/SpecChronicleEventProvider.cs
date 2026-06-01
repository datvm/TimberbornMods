namespace BeaverChronicles.Services;

[MultiBind(typeof(IChronicleEventsProvider))]
public class SpecChronicleEventProvider(
    ISpecService specs,
    IEnumerable<ISpecChronicleEventCustomCode> customCodes,
    SpecChronicleEventHelperFactory helperFac
) : IChronicleEventsProvider
{

    public IEnumerable<IChronicleEvent> GetEvents()
    {
        var codeById = customCodes.ToDictionary(c => c.Id);
        
        var evSpecs = specs.GetSpecs<ChronicleEventSpec>();

        foreach (var spec in evSpecs)
        {
            if (!codeById.Remove(spec.Id, out var customCode) && spec.NeedCustomCode)
            {
                throw new InvalidOperationException($"Spec {spec.Id} requires custom code, but none was found.");
            }

            yield return new SpecChronicleEvent(spec, customCode, helperFac);
        }

        if (codeById.Count > 0)
        {
            var extraCodes = string.Join(", ", codeById.Keys);
            throw new InvalidOperationException($"Custom code provided for non-existent specs: {extraCodes}");
        }
    }

}
