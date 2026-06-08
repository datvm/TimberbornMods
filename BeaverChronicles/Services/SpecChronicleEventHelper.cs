namespace BeaverChronicles.Services;

public class SpecChronicleEventHelper
{
    public readonly ChronicleEventContext Context;
    public readonly ChronicleEventFlagHelper FlagHelper;
    public readonly ISpecChronicleEventCustomCode? CustomCode;
    public readonly ChronicleEventSpec Spec;
    public readonly SpecChronicleEvent Event;

    public EventHistoryRecord CurrentRecord { get; }
    public EventHistoryPage CurrentPage
    {
        get
        {
            if (CurrentRecord.Pages.Count == 0)
            {
                throw new InvalidOperationException("Current record has no pages.");
            }
            return CurrentRecord.CurrentPage;
        }
    }

    public string CurrentNodeId
    {
        get => CurrentRecord.CustomParameters.TryGetValue("CurrentNodeId", out var value) ? value
            : throw new InvalidOperationException("CurrentNodeId is not set in the current record.");
        set => CurrentRecord.CustomParameters["CurrentNodeId"] = value;
    }

    readonly FrozenDictionary<string, ISpecNodeHandler> handlers;
    readonly EvaluationCacheService evaluationCache;
    readonly ChronicleEventConditionService conditionService;

    public SpecChronicleEventHelper(
        SpecChronicleEvent ev,
        ChronicleEventContext context,
        FrozenDictionary<string, ISpecNodeHandler> handlers,
        EvaluationCacheService evaluationCache,
        ChronicleEventConditionService conditionService
    )
    {
        Event = ev;
        this.handlers = handlers;
        this.evaluationCache = evaluationCache;
        this.conditionService = conditionService;

        Context = context;
        FlagHelper = context.FlagHelper;
        CustomCode = ev.CustomCode;
        Spec = ev.Spec;

        CurrentRecord = context.ActiveRecord!;
    }

    public FlagCheckResult CheckFlags()
    {
        var cond = Spec.Conditions;

        if (SatisfiedFlags(cond.BlockedFlags, cond.BlockedFlagsCondition)) { return FlagCheckResult.Block; }
        if (SatisfiedFlags(cond.RequiredNoFlags, cond.RequiredNoFlagsCondition)) { return FlagCheckResult.CannotTrigger; }

        // empty required flags means no flags are required, so it can trigger
        return cond.RequiredFlags.Length == 0 || SatisfiedFlags(cond.RequiredFlags, cond.RequiredFlagsCondition)
            ? FlagCheckResult.CanTrigger
            : FlagCheckResult.CannotTrigger;
    }

    bool SatisfiedFlags(ImmutableArray<string> flags, ConditionType type)
    {
        if (flags.Length == 0) { return false; }
        return type.Evaluate(flags, FlagHelper!.HasFlag);
    }

    public void RestoreGameState()
    {
        var id = CurrentNodeId;

        var node = Spec.Nodes[id];
        handlers[node.Type].RestoreGameState(node, this);
    }

    public void TriggerNode(string? id) => Event.TriggerNode(id);

    public void TriggerNode(ChronicleEventNodeSpec node)
    {
        CurrentNodeId = node.Id;
        handlers[node.Type].HandleNode(node, this);
    }

    public bool EvaluateConditionNode(string id) =>
        Spec.Nodes[id] is not { IsConditionNode: true } n
            ? throw new InvalidOperationException($"Node {id} is not a condition node.")
            : evaluationCache.GetOrEvaluate($"ActiveEvent.Condition.{id}", () =>
            {
                return conditionService.Evaluate(Event, n, n.GetData<ConditionData>());
            });

    [return: NotNullIfNotNull(nameof(input))]
    public string? FormatText(string? input)
    {
        if (input == null) { return null; }
        if (input.Length == 0) { return ""; }

        StringBuilder? output = null;

        var curr = 0;
        do
        {
            var nextOpen = input.IndexOf('{', curr);
            if (nextOpen == -1)
            {
                if (output is null) { return input; }
                break;
            }

            var close = input.IndexOf('}', nextOpen + 1);
            if (close == -1)
            {
                if (output is null) { return input; }
                break;
            }

            output ??= new(input.Length);
            if (nextOpen > curr)
            {
                output.Append(input, curr, nextOpen - curr);
            }

            var placeholder = input[(nextOpen + 1)..close];
            var value = GetPlaceholderValue(placeholder);
            if (value is null)
            {
                output.Append('{');
                curr = nextOpen + 1;
            }
            else
            {
                output.Append(value);
                curr = close + 1;
            }
        } while (true);

        if (curr < input.Length)
        {
            output.Append(input[curr..]);
        }

        return output.ToString();
    }

    string? GetPlaceholderValue(string placeholder)
    {
        if (placeholder.StartsWith("CP_"))
        {
            var key = placeholder[3..];
            return CurrentRecord.CustomParameters.TryGetValue(key, out var value) ? value : null;
        }

        if (TryGetPrefix("S", Spec.Parameters.Strings, x => x, out var flagValue)) { return flagValue; }
        if (TryGetPrefix("I", Spec.Parameters.Ints, x => x.ToString(), out flagValue)) { return flagValue; }
        if (TryGetPrefix("F", Spec.Parameters.Floats, x => x.ToString("F2"), out flagValue)) { return flagValue; }
        if (TryGetPrefix("%", Spec.Parameters.Floats, x => x.ToString("P0"), out flagValue)) { return flagValue; }

        return null;

        bool TryGetPrefix<T>(string prefix, IReadOnlyList<T> list, Func<T, string> format, out string? value)
        {
            value = null;
            if (!placeholder.StartsWith(prefix)) { return false; }
            if (!int.TryParse(placeholder[prefix.Length..], out var index)
                || (index < 1)
                || index > list.Count) { return false; }

            value = format(list[index - 1]);
            return value is not null;
        }
    }

    public enum FlagCheckResult
    {
        Block,
        CanTrigger,
        CannotTrigger
    }

}
