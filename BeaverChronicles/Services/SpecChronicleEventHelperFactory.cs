namespace BeaverChronicles.Services;

[BindSingleton]
public class SpecChronicleEventHelperFactory(
    IEnumerable<ISpecNodeHandler> handlers,
    EvaluationCacheService evaluationCache
) : ILoadableSingleton
{

    readonly FrozenDictionary<ChronicleEventNodeType, ISpecNodeHandler> handlers 
        = handlers.ToFrozenDictionary(h => h.ForType);

    public void Load()
    {
        foreach (var e in TimberUiUtils.GetSortedEnumValues<ChronicleEventNodeType>())
        {
            if (!handlers.ContainsKey(e))
            {
                throw new InvalidOperationException($"No handler found for ChronicleEventNodeType {e}");
            }
        }
    }

    public SpecChronicleEventHelper Create(SpecChronicleEvent ev)  => new(ev, handlers, evaluationCache);

}
