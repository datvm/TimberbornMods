namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.NeedSpecs.NeedSpec")]
public record ParsedNeedSpec(
    string Id,
    string[] BackwardCompatibleIds,
    Int32 Order,
    string NeedGroupId,
    string CharacterType,
    string DisplayName,
    Single StartingValue,
    Single MinimumValue,
    Single MaximumValue,
    Single DailyDelta,
    Single ImportanceMultiplier,
    Single Effectiveness,
    Boolean Wastable,
    Single HoursWarningThreshold,
    string DisplayNameLocKey,
    Int32 FavorableWellbeing,
    Int32 UnfavorableWellbeing
) : ParsedComponentSpec, IComponentSpecWithId;