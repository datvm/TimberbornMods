namespace TailsAndBannersModMaker.Models;

public class ModMakerInfo
{
    public const string DecalTypeTails = "Tails";
    public const string DecalTypeBanners = "Banners";
    public static readonly ImmutableArray<string> DecalTypes = [DecalTypeTails, DecalTypeBanners];

    public string Id { get; set; } = "ModAuthor.ModName";
    public string Name { get; set; } = "New Mod Name";
    public string Version { get; set; } = "1.0.0";

    public ModMakerDecalInfo Tails { get; set; } = new(DecalTypeTails);
    public ModMakerDecalInfo Banners { get; set; } = new(DecalTypeBanners);

    public IEnumerable<ModMakerDecalInfo> DecalInfo => [Tails, Banners];
    public bool HasDecals => DecalInfo.Any(d => d.HasDecal);
    public bool HasCustomGroups => DecalInfo.Any(d => d.HasCustomGroups);

    public ModMakerDecalInfo GetDecalInfo(string type)
    {
        return type switch
        {
            "Tails" => Tails,
            "Banners" => Banners,
            _ => throw new ArgumentException($"Unknown decal type: {type}")
        };
    }

}
