namespace BeaverChronicles.Services.Conditions;

public record TriggerCharacterData
{
    public bool? IsBeaver { get; init; }
    public bool? IsAdult { get; init; }
    public CharacterType? CharacterType { get; init; }
    public string? Name { get; init; }
    public string? EntityId { get; init; }
    public bool Expected { get; init; } = true;
}

[MultiBind(typeof(IConditionEvaluator))]
public class TriggerCharacter : ConditionEvaluatorBase<TriggerCharacterData>
{
    public override string ForType => nameof(TriggerCharacter);

    protected override bool Evaluate(TriggerCharacterData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        p ??= new();

        var character = ev.Controller.TriggerContext?.Parameters.GetParameterOrDefault<CharacterParameters>();
        var result = character is not null && Matches(character);
        return p.Expected ? result : !result;

        bool Matches(CharacterParameters characterParameters)
        {
            if (p.IsBeaver is { } isBeaver && characterParameters.IsBeaver != isBeaver)
            {
                return false;
            }

            if (p.IsAdult is { } isAdult && characterParameters.IsAdult != isAdult)
            {
                return false;
            }

            if (p.CharacterType is { } characterType && characterParameters.CharacterType != characterType)
            {
                return false;
            }

            if (p.Name is { } name
                && !string.Equals(characterParameters.Character.FirstName, ev.Controller.FormatText(name), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (p.EntityId is { } entityId && Guid.TryParse(entityId, out var guid)
                && characterParameters.Character.GetEntityId() != guid)
            {
                return false;
            }

            return true;
        }
    }
}
