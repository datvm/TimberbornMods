namespace GlobalInventory.Specs;

public record GlobalGoodSpec : ComponentSpec
{
    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string? CopiedGoodId { get; init; }

    [Serialize]
    public string DisplayNameLocKey { get; init; } = null!;
    
    [Serialize(nameof(DisplayNameLocKey))]
    public LocalizedText DisplayName { get; init; } = null!;

    [Serialize]
    public string PluralDisplayNameLocKey { get; init; } = null!;
    [Serialize(nameof(PluralDisplayNameLocKey))]
    public LocalizedText PluralDisplayName { get; init; } = null!;

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public AssetRef<Sprite>? Icon { get; init; }

    [Serialize("Icon")]
    public FlippedSprite? IconFlipped { get; init; }

    [Serialize("Icon")]
    public UISprite? IconSmall { get; init; }

    [Serialize]
    public float MinCapacity { get; init; }

    [Serialize]
    public float? MaxCapacity { get; init; }

    [Serialize]
    public float InitialAmount { get; init; }

    [Serialize]
    public bool HideOnZero { get; init; }

}
