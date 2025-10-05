namespace PackagerBuilder.Services;

public class PackagerModBuilder(
    ModRepository modRepository,
    GoodBuilder goodBuilder
)
{

    public const string BuilderModId = nameof(PackagerBuilder);
    public const string ModId = "Packager";
    public const string ModName = "Packager";

    public void Build()
    {
        var builder = new TimberModBuilderService(GenerateManifest());
        builder.PrepareFolderAndDontClearOnBuild();

        var loc = builder.AddLocalization();

        goodBuilder.Build(builder.ModFolder, loc.Values);

        builder.Build();
    }

    ModManifest GenerateManifest()
    {
        var currManifest = modRepository.EnabledMods
            .First(q => q.Manifest.Id == BuilderModId)
            .Manifest;

        return new(
            ModName,
            "Pack and unpack items",
            currManifest.Version,
            ModId,
            currManifest.MinimumGameVersion,
            [new(BuilderModId, currManifest.Version)],
            []);
    }

}
