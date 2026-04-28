namespace TailsAndBannersModMaker.Models;

public class DecalGroupInfo
{
    public const string DefaultId = "$Default$";

    public string Id { get; set; } = "NewGroupId";
    public string Name { get; set; } = "New Group Name";
    public int Order { get; set; }

    public bool IsDefault => Id == DefaultId;
    public bool HasDecals => Decals.Count > 0;

    public List<DecalInfo> Decals { get; } = [];

}
