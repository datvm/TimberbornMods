namespace DynamicTailsBanners.Services.Implementations;

public class RandomBannerProviderOptions
{

    public List<string> BannerIds { get; } = [];
    public int CurrentDay { get; set; } = -1;
    public string? CurrentBannerId { get; set; }


}
