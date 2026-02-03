namespace PowerCopy.Services;

public readonly record struct ObjectListingQuery(
    EntityComponent Source,
    string? TemplateName,
    IReadOnlyList<Type>? Components,
    DistrictCenter? InDistrict,
    HashSet<EntityComponent>? SelectedBuildings
);