namespace BeaverChronicles.Events;

public interface ICharacterParameters
{
    CharacterParameters Character { get; }
}

public record DayTimeParameters(int Day, int Cycle, int CycleDay, float PartialDays, float Hours, string WeatherId);
public record WeatherWarningParameters(string HazardousWeatherId, DayTimeParameters DayTime);

public record CharacterParameters(Character Character, bool IsBeaver, bool IsAdult)
{
    public CharacterType CharacterType { get; } = IsBeaver
        ? (IsAdult ? CharacterType.AdultBeaver : CharacterType.ChildBeaver)
        : CharacterType.Bot;
}
public record BeaverGrownUpParameters(Character Adult, Character Child);
public record NeedChangedParameters(NeedSpec Need, bool IsActive, CharacterParameters Character) : ICharacterParameters;

public record ToolParameters(ITool Tool);
public record BuildingParameters(string TemplateName, PlaceableBlockObjectSpec PlaceableBlockObjectSpec, BlockObjectTool? Tool);
public record BuildingInstanceParameters(string TemplateName, PlaceableBlockObjectSpec PlaceableBlockObjectSpec, BlockObjectTool? Tool, BlockObject BlockObject)
    : BuildingParameters(TemplateName, PlaceableBlockObjectSpec, Tool);

public record CharacterInAreaParameters(CharacterParameters Character) : ICharacterParameters;

public record CustomEventParameters(string Name, object? Data = null);