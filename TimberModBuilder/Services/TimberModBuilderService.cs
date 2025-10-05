namespace TimberModBuilder.Services;

public class TimberModBuilderService()
{

    public static readonly string UserModFolder = Path.Combine(UserDataFolder.Folder, UserFolderModsProvider.ModsDirectoryName);

    public TimberModBuilderService(ModManifest manifest) : this()
    {
        Manifest = manifest;
    }

    public bool ClearIfExists { get; set; } = true;
    public ModManifest? Manifest { get; set; }
    public ModManifest ManifestOrThrow => Manifest ?? throw new NullReferenceException($"{nameof(Manifest)} is not set yet.");

    public string ModId => ManifestOrThrow.Id;
    public string ModFolder => Path.Combine(UserModFolder, ModId);

    readonly Dictionary<string, ModBuilderLocalization> localizations = [];
    public IReadOnlyDictionary<string, ModBuilderLocalization> Localizations => localizations;

    public List<ModBuilderBlueprint> Blueprints { get; } = [];

    public bool OutputExists => Directory.Exists(ModFolder);

    public ModBuilderArtifact Build()
    {
        var manifest = ManifestOrThrow;

        var folder = PrepareFolder();
        var manifestPath = SaveManifest(folder, manifest);
        var locFolder = SaveLocalizations(folder);
        var bpFolder = SaveBlueprints(folder);

        return new ModBuilderArtifact(folder, manifestPath, locFolder, bpFolder);
    }

    public string PrepareFolderAndDontClearOnBuild()
    {
        var folder = PrepareFolder();
        ClearIfExists = false;

        return folder;
    }

    string PrepareFolder()
    {
        var folder = ModFolder;

        if (ClearIfExists && OutputExists)
        {
            Directory.Delete(folder, true);
        }

        Directory.CreateDirectory(folder);

        return folder;
    }

    string SaveManifest(string folder, ModManifest manifest)
    {
        var path = Path.Combine(folder, "manifest.json");

        var values = TimberModBuilderHelper.ToDictionary(manifest);
        values[nameof(manifest.Version)] = manifest.Version.ToString();
        values[nameof(manifest.MinimumGameVersion)] = manifest.MinimumGameVersion.ToString();

        if (!manifest.RequiredMods.IsDefaultOrEmpty)
        {
            values[nameof(manifest.RequiredMods)] = manifest.RequiredMods
                .Select(SerializeVersionedMod)
                .ToList();
        }

        if (!manifest.OptionalMods.IsDefaultOrEmpty)
        {
            values[nameof(manifest.OptionalMods)] = manifest.OptionalMods
                .Select(SerializeVersionedMod)
                .ToList();
        }

        var json = JsonConvert.SerializeObject(values, Formatting.Indented);
        File.WriteAllText(path, json);

        return path;
    }

    Dictionary<string, object> SerializeVersionedMod(VersionedMod m) => new()
    {
        { nameof(m.Id), m.Id },
        { nameof(m.MinimumVersion), m.MinimumVersion.ToString() }
    };

    public ModBuilderLocalization AddLocalization(string loc = "enUS")
    {
        var localization = new ModBuilderLocalization(loc);
        localizations.Add(loc, localization);
        return localization;
    }

    string? SaveLocalizations(string folder)
    {
        if (localizations.Count == 0) { return null; }

        var locFolder = Path.Combine(folder, "Localizations");
        Directory.CreateDirectory(locFolder);

        foreach (var (key, localization) in localizations)
        {
            var locPath = Path.Combine(locFolder, $"{key}.csv");
            var content = localization.ToCsv();
            File.WriteAllText(locPath, content);
        }

        return locFolder;
    }

    public ModBuilderBlueprint AddBlueprints(ModBuilderBlueprint bp)
    {
        Blueprints.Add(bp);
        return bp;
    }

    string? SaveBlueprints(string folder)
    {
        if (Blueprints.Count == 0) { return null; }

        var bpFolder = Path.Combine(folder, "Blueprints");

        foreach (var bp in Blueprints)
        {
            var bpTypeFolder = Path.Combine(bpFolder, bp.TypeForFileName);
            Directory.CreateDirectory(bpTypeFolder);

            var bpPath = Path.Combine(bpTypeFolder, bp.FileName);
            var content = bp.ToJson();
            File.WriteAllText(bpPath, content);
        }

        return bpFolder;
    }

}
