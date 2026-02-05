namespace PowerCopy.Services;

public readonly record struct ObjectListingQuery(
    EntityComponent Source,
    string? TemplateName,
    HashSet<Type> Components,
    DistrictCenter? InDistrict
);