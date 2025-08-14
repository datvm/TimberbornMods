namespace TimberModBuilder.Services;

public record ModBuilderArtifact(
    string ModFolder,
    string ManifestPath,
    string? LocalizationsFolder,
    string? BlueprintFolder
);
