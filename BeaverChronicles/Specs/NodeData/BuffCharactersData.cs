namespace BeaverChronicles.Specs.NodeData;

public record BuffCharactersData(
    string BuffId,
    string TitleLoc,
    string DescLoc,
    CharacterType CharacterTypes,
    ImmutableArray<FormattableGoodItem> Bonuses,
    string? Days,
    EntityBuffCategory Category = EntityBuffCategory.LimitedTime
);
