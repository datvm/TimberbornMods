namespace ModdableTimberbornAchievements.Specs;

#nullable disable
public record ModdableAchievementSpec : ComponentSpec
{
    [Serialize] public string Id { get; init; }
    [Serialize] public string GroupId { get; init; }
    [Serialize] public int Order { get; init; }
    [Serialize] public string NameLoc { get; init; }
    [Serialize(nameof(NameLoc))] public LocalizedText Name { get; init; }
    [Serialize] public string DescLoc { get; init; }    
    [Serialize(nameof(DescLoc))] public LocalizedText Description { get; init; }
    [Serialize] public bool IsSecret { get; init; }
    [Serialize] public AssetRef<Sprite> Icon { get; init; }    
}
#nullable enable