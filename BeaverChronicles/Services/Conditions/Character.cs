namespace BeaverChronicles.Services.Conditions;

public record CharacterConditionData
{
    public ImmutableArray<string> EntityIds { get; init; } = [];
    public ConditionType ConditionType { get; init; } = ConditionType.Any;
    public bool? IsBeaver { get; init; }
    public bool? IsAdult { get; init; }
    public CharacterType? CharacterType { get; init; }
    public string? Name { get; init; }
}

[MultiBind(typeof(IConditionEvaluator))]
public class CharacterCondition : ConditionEvaluatorBase<CharacterConditionData>
{
    public override string ForType => "Character";

    protected override bool Evaluate(CharacterConditionData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        p ??= new();

        var characters = ev.Controller.GetEntities<Character>(p.EntityIds.Select(FormatEntityId)).ToArray();
        if (characters.Length == 0)
        {
            return false;
        }

        return p.ConditionType.Evaluate(characters, Matches);

        string FormatEntityId(string entityId) => ev.Controller.FormatText(entityId);

        bool Matches(Character character)
        {
            var characterType = character.GetCharacterType();
            if (characterType == CharacterType.Unknown) { return false; }

            if (p.IsBeaver is { } isBeaver && characterType.IsBeaver() != isBeaver) { return false; }
            if (p.IsAdult is { } isAdult && (characterType == CharacterType.AdultBeaver) != isAdult) { return false; }
            if (p.CharacterType is { } expectedType && !expectedType.HasFlag(characterType)) { return false; }
            if (p.Name is { } name
                && !string.Equals(character.FirstName, ev.Controller.FormatText(name), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}

[MultiBind(typeof(IConditionEvaluator))]
public class TriggerCharacter : CharacterCondition
{
    public override string ForType => nameof(TriggerCharacter);
}
