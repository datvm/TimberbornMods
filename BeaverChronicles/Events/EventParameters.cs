namespace BeaverChronicles.Events;

public record DayTimeParameters(int Day, int Cycle, int CycleDay, float PartialDays, float Hours, string WeatherId);

public record CharacterParameters(Character Character, bool IsBeaver, bool IsAdult);
public record BeaverGrownUpParameters(Character Adult, Character Child);

public record ToolParameters(ITool Tool);
public record BuildingParameters(string TemplateName, PlaceableBlockObjectSpec PlaceableBlockObjectSpec, BlockObjectTool? Tool);