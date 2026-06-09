namespace BeaverChronicles.Events;

public record DayTimeParameters(int Day, int Cycle, int CycleDay, float PartialDays, float Hours, string WeatherId);
public record WeatherWarningParameters(string HazardousWeatherId, DayTimeParameters DayTime);

public record CharacterParameters(Character Character, bool IsBeaver, bool IsAdult)
    : ITriggerParameterWith<CharacterParameters>
{
    CharacterParameters ITriggerParameterWith<CharacterParameters>.Parameter => this;

    public CharacterType CharacterType { get; } = IsBeaver
        ? (IsAdult ? CharacterType.AdultBeaver : CharacterType.ChildBeaver)
        : CharacterType.Bot;
}
public record BeaverGrownUpParameters(Character Adult, Character Child);
public record NeedChangedParameters(NeedSpec Need, bool IsActive, CharacterParameters Character)
    : ITriggerParameterWith<CharacterParameters>
{
    CharacterParameters ITriggerParameterWith<CharacterParameters>.Parameter => Character;
}

public record ToolParameters(ITool Tool);
public record BuildingParameters(string TemplateName, PlaceableBlockObjectSpec PlaceableBlockObjectSpec, BlockObjectTool? Tool);
public record BuildingInstanceParameters(string TemplateName, PlaceableBlockObjectSpec PlaceableBlockObjectSpec, BlockObjectTool? Tool, BlockObject BlockObject)
    : BuildingParameters(TemplateName, PlaceableBlockObjectSpec, Tool);

public record CharacterInAreaParameters(CharacterParameters Character)
    : ITriggerParameterWith<CharacterParameters>
{
    CharacterParameters ITriggerParameterWith<CharacterParameters>.Parameter => Character;
}

public record CustomEventParameters(string Name, object? Data = null);
