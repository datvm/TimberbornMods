namespace BeaverChronicles.Specs.NodeData;

public class GiveData
{
    public FormattableGoodItem[] Items { get; init; } = [];
    public string? Science { get; init; }
    public FormattableGoodItem[] Spawns { get; init; } = [];
    public string? PreferedDistrictCenter { get; init; }
}
