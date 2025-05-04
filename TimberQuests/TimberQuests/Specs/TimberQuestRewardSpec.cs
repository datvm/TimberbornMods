namespace TimberQuests;

public record TimberQuestRewardSpec : ComponentSpec
{

    [Serialize]
    public int Amount { get; init; }

    [Serialize]
    public string? GoodId { get; init; }
    public string? GoodName { get; set; }

    [Serialize]
    public string? CustomTextKey { get; init; }
    public string? CustomText { get; set; }

    [Serialize]
    public Sprite? Icon { get; set; }

}
