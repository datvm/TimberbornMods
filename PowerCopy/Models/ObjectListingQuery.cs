namespace PowerCopy.Models;

public readonly record struct ObjectListingQuery(
    EntityComponent Source,
    string? TemplateName,
    HashSet<IBuildingSettings> Settings,
    DistrictCenter? InDistrict
);