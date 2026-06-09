namespace BeaverChronicles.Services;

[BindSingleton]
public class SpecChronicleEventControllerFactory(
    IEnumerable<ISpecNodeHandler> handlers,
    EvaluationCacheService evaluationCache,
    ChronicleEventConditionService conditionService,
    HelperCollection helperCollection
)
{

    readonly FrozenDictionary<string, ISpecNodeHandler> handlers 
        = handlers.ToFrozenDictionary(h => h.ForType);
    public bool CanHandle(string nodeType) => handlers.ContainsKey(nodeType);

    public SpecChronicleEventController Create(SpecChronicleEvent ev)
        => new(ev, handlers, evaluationCache, helperCollection, conditionService);

}
