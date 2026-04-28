namespace TailsAndBannersModMaker.Models;

public class ModMakerDecalInfo(string type)
{

    public bool HasDecal => DefaultGroup.HasDecals || Groups.Any(g => g.HasDecals);
    public string Type => type;

    public DecalGroupInfo DefaultGroup { get; } = new()
    {
        Id = DecalGroupInfo.DefaultId,
    };
    public List<DecalGroupInfo> Groups { get; } = [];

    public IEnumerable<DecalGroupInfo> AllGroups => Groups.Prepend(DefaultGroup);

    public bool HasCustomGroups => Groups.Count > 0;

}
