
namespace Hospital.PrefabAdder;

public class HospitalSpecAdder : ISpecModifier
{
    public int Priority { get; }

    public T ModifyGetSingleSpec<T>(T current) where T : ComponentSpec => current;

    public IEnumerable<T> ModifyGetSpecs<T>(IEnumerable<T> current) where T : ComponentSpec
    {
        if (current is IEnumerable<PrefabGroupSpec> specs)
        {
            foreach (var spec in specs)
            {
                if (spec.Id is "Buildings.IronTeeth" or "Buildings.Folktails")
                {
                    yield return (T)(ComponentSpec)(spec with
                    {
                        Paths = [.. spec.Paths.Concat(HospitalAssetProvider.HospitalPaths)]
                    });
                }
                else
                {
                    yield return (T)(ComponentSpec) spec;
                }
            }
        }
        else
        {
            foreach (var spec in current)
            {
                yield return spec;
            }
        }
    }
}
