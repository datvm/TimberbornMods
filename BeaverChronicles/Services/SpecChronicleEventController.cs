namespace BeaverChronicles.Services;

public class SpecChronicleEventController(
    SpecChronicleEvent ev,
    FrozenDictionary<string, ISpecNodeHandler> handlers,
    EvaluationCacheService evaluationCache,
    HelperCollection helperCollection,
    ChronicleEventConditionService conditionService
)
{
    static InvalidOperationException ThrowInactive() => new("Event is not active");

    public ChronicleTriggerContext? TriggerContext { get; private set; }
    public ChronicleEventContext? Context { get; private set; }

    public bool IsMini { get; } = ev.IsMiniEvent;
    public bool IsActive => Context is not null;
    public ChronicleEventContext ActiveContext => Context ?? throw ThrowInactive();

    public readonly ISpecChronicleEventCustomCode? CustomCode = ev.CustomCode;
    public readonly ChronicleEventSpec Spec = ev.Spec;
    public readonly SpecChronicleEvent Event = ev;
    public readonly HelperCollection HelperCollection = helperCollection;

    public EventHistoryRecord CurrentRecord => ActiveContext.Record;

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

    public int GetTriggerWeight(ChronicleTriggerContext context)
    {
        TriggerContext = context;

        // Expired time
        if (CheckExpiredTime()) { return -1; }

        // Minimum time
        if (!CheckMinimumTime()) { return 0; }

        // Flags
        switch (CheckFlags())
        {
            case FlagCheckResult.Block:
                return -1;
            case FlagCheckResult.CannotTrigger:
                return 0;
        }

        // Custom code
        var condSpec = Spec.Conditions;
        if (condSpec.CustomWeightCode)
        {
            return CustomCode!.GetWeight(Event);
        }

        // Conditions
        if (Spec.Conditions.ConditionNodeId is not { } nodeId || EvaluateConditionNode(nodeId))
        {
            return condSpec.Weight;
        }
        else
        {
            return 0;
        }
    }

    public void SetContext(ChronicleEventContext context)
    {
        TriggerContext = Context = context;
        RecordTriggerParameters();
    }

    public void ConcludeEvent()
    {
        ActiveContext.ConcludeEvent();
        TriggerContext = Context = null;
    }

    bool CheckMinimumTime()
    {
        var cond = Spec.Conditions;
        var minDay = cond.MinDay;
        var minCycle = cond.MinCycle;
        if (minDay <= 1 && minCycle <= 1) { return true; }

        var stats = HelperCollection.GameStats;
        if (minDay > 1 && stats.GameDayAndHours < minDay)
        {
            return false;
        }

        if (minCycle > 1 && stats.GameCycleNumber < minCycle)
        {
            return false;
        }

        return true;
    }

    bool CheckExpiredTime()
    {
        var cond = Spec.Conditions;
        var maxDay = cond.MaxDay;
        var maxCycle = cond.MaxCycle;

        if (maxDay <= 1 && maxCycle <= 1) { return false; }

        var stats = HelperCollection.GameStats;
        if (maxDay > 1 && stats.GameDayAndHours > maxDay)
        {
            return true;
        }

        if (maxCycle > 1 && stats.GameCycleNumber > maxCycle)
        {
            return true;
        }

        return false;
    }

    FlagCheckResult CheckFlags()
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
        return type.Evaluate(flags, HelperCollection.Flags.HasFlag);
    }

    public void RestoreGameState()
    {
        var id = CurrentNodeId;

        var node = Spec.Nodes[id];
        handlers[node.Type].RestoreGameState(node, this);
    }

    public void PostLoadGameState()
    {
        var id = CurrentNodeId;

        var node = Spec.Nodes[id];
        handlers[node.Type].PostLoadGameState(node, this);
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

    void RecordTriggerParameters()
    {
        var record = CurrentRecord;
        var parameters = ActiveContext.Parameters;

        record.CustomParameters.TryAdd("TriggerSource", parameters.Source.ToString());

        if (parameters.GetParameterOrDefault<CharacterParameters>() is { } character)
        {
            RecordCharacter("TriggerCharacter", character);
        }

        if (parameters.GetParameterOrDefault<BeaverGrownUpParameters>() is { } grownUp)
        {
            RecordRawCharacter("TriggerAdult", grownUp.Adult, CharacterType.AdultBeaver);
            RecordRawCharacter("TriggerChild", grownUp.Child, CharacterType.ChildBeaver);
        }

        if (parameters.GetParameterOrDefault<BuildingInstanceParameters>() is { } building)
        {
            RecordBuilding("TriggerBuilding", building);
        }
        else if (parameters.GetParameterOrDefault<BuildingParameters>() is { } buildingParameters)
        {
            RecordBuilding("TriggerBuilding", buildingParameters);
        }

        void RecordCharacter(string prefix, CharacterParameters character)
        {
            record.CustomParameters.TryAdd(prefix + "Id", character.Character.GetEntityId().ToString());
            record.CustomParameters.TryAdd(prefix + "Name", character.Character.FirstName);
            record.CustomParameters.TryAdd(prefix + "IsBeaver", character.IsBeaver.ToString());
            record.CustomParameters.TryAdd(prefix + "IsAdult", character.IsAdult.ToString());
            record.CustomParameters.TryAdd(prefix + "Type", character.CharacterType.ToString());
        }

        void RecordRawCharacter(string prefix, Character character, CharacterType characterType)
        {
            record.CustomParameters.TryAdd(prefix + "Id", character.GetEntityId().ToString());
            record.CustomParameters.TryAdd(prefix + "Name", character.FirstName);
            record.CustomParameters.TryAdd(prefix + "IsBeaver", true.ToString());
            record.CustomParameters.TryAdd(prefix + "IsAdult", (characterType == CharacterType.AdultBeaver).ToString());
            record.CustomParameters.TryAdd(prefix + "Type", characterType.ToString());
        }

        void RecordBuilding(string prefix, BuildingParameters building)
        {
            record.CustomParameters.TryAdd(prefix + "TemplateName", building.TemplateName);
            if (building is BuildingInstanceParameters instance)
            {
                var bo = instance.BlockObject;
                record.CustomParameters.TryAdd(prefix + "Id", bo.GetEntityId().ToString());
                record.CustomParameters.TryAdd(prefix + "IsFinished", bo.IsFinished.ToString());
            }
        }
    }

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

    public IEnumerable<string?> FormatTexts(IEnumerable<string?> inputs)
    {
        foreach (var input in inputs)
        {
            yield return FormatText(input);
        }
    }

    public IEnumerable<string> FormatTextsRemoveEmpty(IEnumerable<string?> inputs)
    {
        foreach (var input in inputs)
        {
            var formatted = FormatText(input);
            if (!string.IsNullOrEmpty(formatted))
            {
                yield return formatted;
            }
        }
    }

    [return: NotNullIfNotNull(nameof(input))]
    public string? FormatTextLoc(string? input)
        => input is null ? null : FormatText(HelperCollection.t.T(input));

    public int FormatTextInt(string? input)
        => int.TryParse(FormatText(input), out var result) ? result : 0;

    public float FormatTextFloat(string? input)
        => float.TryParse(FormatText(input), out var result) ? result : 0;

    public T FormatTextEnum<T>(string? input) where T : struct, Enum
    {
        var text = FormatText(input);
        return Enum.TryParse<T>(text, true, out var result) ? result : throw new InvalidOperationException($"Failed to parse '{text}' as {typeof(T).Name}.");
    }

    static readonly FrozenSet<string> TrueValues = FrozenSet.Create(StringComparer.InvariantCultureIgnoreCase, "1", "true", "yes");
    public bool FormatTextBool(string? input)
    {
        var text = FormatText(input);
        return !string.IsNullOrEmpty(text) && (TrueValues.Contains(text)
            || float.TryParse(text, out var number) && number > 0);
    }

    BaseComponent? GetTriggeringEntity()
    {
        var parameters = TriggerContext?.Parameters;
        if (parameters is null) { return null; }

        if (parameters.TryGetParameter<CharacterParameters>(out var c))
        {
            return c.Character;
        }
        else if (parameters.TryGetParameter<BuildingInstanceParameters>(out var b))
        {
            return b.BlockObject;
        }

        return null;
    }

    public IEnumerable<BaseComponent> GetEntities(IEnumerable<string> entitiesIds)
    {
        if (!IsActive)
        {
            var entity = GetTriggeringEntity();
            if (entity)
            {
                yield return entity!;
            }

            yield break;
        }

        var cp = CurrentRecord.CustomParameters;
        var entities = HelperCollection.FindEntity;

        foreach (var id in entitiesIds)
        {
            // 1. Check if it's an array
            if (cp.TryGetValue(id + "_Count", out var countStr) && int.TryParse(countStr, out var count))
            {
                for (int i = 0; i < count; i++)
                {
                    if (cp.TryGetValue($"{id}_{i + 1}", out var elementId) && entities.TryFindEntity(elementId, out var e))
                    {
                        yield return e;
                    }
                }
            }

            // 2. Check if the CP itself is an entity
            else if (cp.TryGetValue(id, out var entityId) && entities.TryFindEntity(entityId, out var e))
            {
                yield return e;
            }

            // 3. Check if the string itself is a GUID
            else if (entities.TryFindEntity(id, out e))
            {
                yield return e;
            }
        }
    }

    public IEnumerable<T> GetEntities<T>(IEnumerable<string> entitiesIds)
    {
        foreach (var e in GetEntities(entitiesIds))
        {
            var comp = e.GetComponent<T>();
            if (comp is not null)
            {
                yield return comp;
            }
        }
    }

    string? GetPlaceholderValue(string placeholder)
    {
        if (placeholder.StartsWith("CP_"))
        {
            var key = placeholder[3..];

            if (!IsActive) { return null; }
            return CurrentRecord.CustomParameters.TryGetValue(key, out var value) ? value : null;
        }

        if (placeholder.StartsWith("GS_"))
        {
            var stat = placeholder[3..];
            return HelperCollection.GameStats.GetStat(stat)?.ToString();
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

    public GoodAmount FormatItem(FormattableGoodItem item) => new(FormatText(item.Id), FormatTextInt(item.Amount));
    public IEnumerable<GoodAmount> FormatItems(IEnumerable<FormattableGoodItem> items) => items.Select(FormatItem);
    public BonusStat FormatBonus(FormattableGoodItem item) => new(FormatText(item.Id), FormatTextFloat(item.Amount));
    public IEnumerable<BonusStat> FormatBonuses(IEnumerable<FormattableGoodItem> items) => items.Select(FormatBonus);
}

public enum FlagCheckResult
{
    Block,
    CanTrigger,
    CannotTrigger
}
