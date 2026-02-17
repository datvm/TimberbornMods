namespace PowerCopy.Models;

public record ObjectListingDetailedResult(
    ImmutableArray<ValueTuple<DistrictCenter?, ObjectListingDetailedEntry[]>> ObjectsByDistricts
)
{

    public IEnumerable<ObjectListingDetailedEntry> AllComponents => ObjectsByDistricts
        .SelectMany(kvp => kvp.Item2);

    public IEnumerable<EntityComponent> SelectedComponents => ObjectsByDistricts
        .SelectMany(kvp => kvp.Item2)
        .Where(e => e.Checked)
        .Select(e => e.Entity);

}

public record ObjectListingDetailedEntry(EntityComponent Entity, string Name, BuildingSettingsPair[] BuildingSettings)
{
    public bool Checked { get; set; } = true;
}
