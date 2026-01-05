namespace ModdableRecipes.Specs;

public record ModdableRecipeLockSpec : ComponentSpec
{

    [Serialize]
    public string? DescriptionLoc { get; init; } = null!;

    [Serialize(nameof(DescriptionLoc))]
    public LocalizedText? Description { get; init; } = null!;

    [Serialize]
    public bool UnlockByDefault { get; init; }

    [Serialize]
    public ModdableRecipeLockTitle LockTitle { get; init; } = ModdableRecipeLockTitle.Hidden;

    [Serialize]
    public ImmutableArray<string> VisibleFactions { get; init; } = [];

}

public enum ModdableRecipeLockTitle
{
    Hidden,
    Censored,
    Visible,
}
