namespace BeaverChronicles.Events;

[MultiBind(typeof(IChronicleEventsProvider))]
public class SpecChronicleEventProvider(
    ISpecService specs,
    IEnumerable<ISpecChronicleEventCustomCode> customCodes,
    SpecChronicleEventControllerFactory helperFac,
    ChronicleEventConditionService conditions
) : IChronicleEventsProvider
{

    public IEnumerable<IChronicleEvent> GetEvents()
    {
        var codeById = customCodes.ToDictionary(c => c.Id);

        var evSpecs = specs.GetSpecs<ChronicleEventSpec>();

        foreach (var spec in evSpecs)
        {
            spec.Nodes.Initialize();
            ValidateSpec(spec);

            if (!codeById.Remove(spec.Id, out var customCode) && spec.NeedCustomCode)
            {
                throw new InvalidOperationException($"Spec {spec.Id} requires custom code, but none was found.");
            }


            var ev = spec.IsMini
                ? new MiniSpecChronicleEvent(spec, customCode, helperFac)
                : new SpecChronicleEvent(spec, customCode, helperFac);
            ev.Initialize();
            yield return ev;
        }

        if (codeById.Count > 0)
        {
            var extraCodes = string.Join(", ", codeById.Keys);
            throw new InvalidOperationException($"Custom code provided for non-existent specs: {extraCodes}");
        }
    }

    public void ValidateSpec(ChronicleEventSpec spec)
    {
        if (string.IsNullOrEmpty(spec.Id))
        {
            throw new InvalidDataException("Spec Id cannot be null or empty. Blueprint name: " + spec.Blueprint.Name);
        }

        var isMini = spec.IsMini;

        foreach (var n in spec.Nodes.Items)
        {
            if (!helperFac.CanHandle(n.Type))
            {
                throw new InvalidDataException($"{GetErrPrefix(n)}Node type {n.Type} is not supported.");
            }

            if (n.Type == ConditionNodeHandler.NodeType)
            {
                var data = n.GetData<ConditionData>();

                if (data.Conditions.Length == 0)
                {
                    throw new InvalidDataException($"{GetErrPrefix(n)}Empty conditions array is not allowed.");
                }

                foreach (var c in data.Conditions)
                {
                    if (!conditions.HasEvaluator(c.Type))
                    {
                        throw new InvalidDataException($"{GetErrPrefix(n)}Condition type {c.Type} is not supported.");
                    }
                }
            }

            if (n.Type == TimeLimitNodeHandler.NodeType)
            {
                if (isMini)
                {
                    throw new InvalidDataException($"{GetErrPrefix(n)}Mini events cannot contain a TimeLimit node.");
                }

                var data = n.GetData<TimeLimitData>();
                var days = data.Days;
                var hours = data.Hours;
                if (days is not null && hours is not null)
                {
                    throw new InvalidDataException($"{GetErrPrefix(n)}TimeLimit node cannot have both days and hours set. Set one or the other only.");
                }

                if (days is null && hours is null)
                {
                    if (data.Payments.Length == 0 && data.Subscriptions.Length == 0)
                    {
                        throw new InvalidDataException($"{GetErrPrefix(n)}Indefinite TimeLimit node must have at least one payment or subscription.");
                    }
                }
            }
        }

        string GetErrPrefix(ChronicleEventNodeSpec n) => $"Spec {spec.Id}, Node {n.Id}: ";
    }

}
