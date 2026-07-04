namespace BeaverChronicles.Services.Conditions;

public record CharacterConditionData
{
    public ImmutableArray<string> EntityIds { get; init; } = [];
    public ConditionType ConditionType { get; init; } = ConditionType.Any;
    public bool? IsBeaver { get; init; }
    public bool? IsAdult { get; init; }
    public CharacterType? CharacterType { get; init; }
    public string? Name { get; init; }
    public bool? Alive { get; init; }
    public ImmutableArray<string> ActiveNeedIds { get; init; } = [];
    public ImmutableArray<string> InactiveNeedIds { get; init; } = [];
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
            this.LogVerbose(node, () => $"No characters found. Evaluated to false.");
            return false;
        }

        return p.ConditionType.Evaluate(characters, Matches);

        string FormatEntityId(string entityId) => ev.Controller.FormatText(entityId);

        bool Matches(Character character)
        {
            string log = $"- Character {character.FirstName}: ";

            var characterType = character.GetCharacterType();
            if (characterType == CharacterType.Unknown)
            {
                this.LogVerbose(node, () => $"{log} Unknown character type -> False");
                return false;
            }

            if (p.IsBeaver is { } isBeaver && characterType.IsBeaver() != isBeaver)
            {
                this.LogVerbose(node, () => $"{log} IsBeaver={isBeaver} does not match character type -> False");
                return false;
            }
            if (p.IsAdult is { } isAdult && (characterType == CharacterType.AdultBeaver) != isAdult)
            {
                this.LogVerbose(node, () => $"{log} IsAdult={isAdult} does not match character type -> False");
                return false;
            }
            if (p.CharacterType is { } expectedType && !expectedType.HasFlag(characterType))
            {
                this.LogVerbose(node, () => $"{log} CharacterType={expectedType} does not match character type -> False");
                return false;
            }
            if (p.Name is { } name
                && !string.Equals(character.FirstName, ev.Controller.FormatText(name), StringComparison.InvariantCultureIgnoreCase))
            {
                this.LogVerbose(node, () => $"{log} Name={name} does not match character name -> False");
                return false;
            }
            if (p.Alive is { } alive && character.Alive != alive)
            {
                this.LogVerbose(node, () => $"{log} Alive={alive} does not match character state -> False");
                return false;
            }
            if (!NeedsMatch(p.ActiveNeedIds, true))
            {
                return false;
            }
            if (!NeedsMatch(p.InactiveNeedIds, false))
            {
                return false;
            }

            this.LogVerbose(node, () => $"{log} Matches -> True");
            return true;

            bool NeedsMatch(ImmutableArray<string> needIds, bool expectedActive)
            {
                if (needIds.Length == 0) { return true; }

                var needMan = character.GetNeedManager();
                foreach (var needId in needIds)
                {
                    var formattedNeedId = ev.Controller.FormatText(needId);
                    var actualNeedActive = needMan.NeedIsActive(formattedNeedId);
                    if (actualNeedActive != expectedActive)
                    {
                        this.LogVerbose(node, () => $"{log} Need {formattedNeedId} active state was {actualNeedActive}, expected {expectedActive} -> False");
                        return false;
                    }
                }

                return true;
            }
        }
    }
}

[MultiBind(typeof(IConditionEvaluator))]
public class TriggerCharacter : CharacterCondition
{
    public override string ForType => nameof(TriggerCharacter);
}
