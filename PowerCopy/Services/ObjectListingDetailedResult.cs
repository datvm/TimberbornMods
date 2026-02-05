namespace PowerCopy.Services;

public record ObjectListingDetailedResult(
    ImmutableArray<KeyValuePair<DistrictCenter?, ImmutableArray<ObjectListingDetailedEntry>>> ObjectsByDistricts
)
{

    public IEnumerable<ObjectListingDetailedEntry> AllComponents => ObjectsByDistricts
        .SelectMany(kvp => kvp.Value);

    public IEnumerable<EntityComponent> SelectedComponents => ObjectsByDistricts
        .SelectMany(kvp => kvp.Value)
        .Where(e => e.Checked)
        .Select(e => e.Entity);

}

public record ObjectListingDetailedEntry(EntityComponent Entity, string Name, ImmutableArray<IDuplicable> Duplicables)
{
    public bool Checked { get; set; } = true;
}
