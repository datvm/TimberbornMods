namespace BeaverChronicles.Events;

public record DayTimeParameters(int Day, int Cycle, int CycleDay, float PartialDays, float Hours, string WeatherId);
public record WeatherWarningParameters(string HazardousWeatherId, DayTimeParameters DayTime);

public record CharacterParameters(Character Character, bool IsBeaver, bool IsAdult);
public record BeaverGrownUpParameters(Character Adult, Character Child);
public record NeedChangedParameters(NeedSpec Need, bool IsActive, CharacterParameters Character);

public record ToolParameters(ITool Tool);
public record BuildingParameters(string TemplateName, PlaceableBlockObjectSpec PlaceableBlockObjectSpec, BlockObjectTool? Tool);