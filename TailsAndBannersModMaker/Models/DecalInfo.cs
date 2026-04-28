namespace TailsAndBannersModMaker.Models;

public class DecalInfo(DecalSpec DecalSpec)
{

    public DecalSpec DecalSpec { get; } = DecalSpec;

    public string Id => DecalSpec.Id;
    public Texture2D Texture => DecalSpec.Texture.Asset;

    public string? FactionId { get; set; }

}
