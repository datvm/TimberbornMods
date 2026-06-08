namespace BeaverChronicles.Services;

[BindSingleton]
public class SpecChronicleEventHelperFactory(
    IEnumerable<ISpecNodeHandler> handlers,
    EvaluationCacheService evaluationCache,
    ChronicleEventConditionService conditionService
)
{

    readonly FrozenDictionary<string, ISpecNodeHandler> handlers 
        = handlers.ToFrozenDictionary(h => h.ForType);
    public bool CanHandle(string nodeType) => handlers.ContainsKey(nodeType);

    public SpecChronicleEventHelper Create(SpecChronicleEvent ev, ChronicleEventContext context)
        => new(ev, context, handlers, evaluationCache, conditionService);

}
