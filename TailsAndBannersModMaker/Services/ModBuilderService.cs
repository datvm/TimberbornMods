namespace TailsAndBannersModMaker.Services;

[BindMenuSingleton]
public class ModBuilderService(ILoc t)
{

    public string? Validate(ModMakerInfo mod)
    {
        if (IsEmpty(mod.Id, mod.Name, mod.Version)) { return t.T("LV.TBMM.ErrModManifest"); }
        if (!IsValidFileName(mod.Id)) { return GetFileNameError("LV.TBMM.ErrModId", mod.Id); }

        if (!mod.HasDecals) { return t.T("LV.TBMM.ErrNoDecalsMod"); }

        foreach (var type in mod.DecalInfo)
        {
            foreach (var grp in type.Groups)
            {
                if (IsEmpty(grp.Id, grp.Name))
                {
                    return t.T("LV.TBMM.ErrGroupInfo", t.TDecal(type.Type));
                }

                if (!IsValidFileName(grp.Id))
                {
                    return GetFileNameError("LV.TBMM.ErrGroupId", grp.Id);
                }

                if (!grp.HasDecals)
                {
                    return t.T("LV.TBMM.ErrNoDecalsGroup", grp.Name, grp.Id, t.TDecal(type.Type));
                }
            }

            if (type.Groups.Count > 1)
            {
                HashSet<string> groupIds = [];
                foreach (var grp in type.Groups)
                {
                    if (!groupIds.Add(grp.Id))
                    {
                        return t.T("LV.TBMM.ErrDuplicateId", t.TDecal(type.Type), grp.Id);
                    }
                }
            }
        }

        return null;
    }

    public IEnumerable<string> ValidateWarnings(ModMakerInfo mod)
    {
        var modFolder = GetModFolder(mod.Id);
        if (Directory.Exists(modFolder))
        {
            yield return t.T("LV.TBMM.WarnFolderExists", modFolder);
        }

        if (mod.HasCustomGroups)
        {
            foreach (var type in mod.DecalInfo)
            {
                HashSet<string> names = [];

                foreach (var grp in type.Groups)
                {
                    if (!names.Add(grp.Name))
                    {
                        yield return t.T("LV.TBMM.WarnGroupNameDup", t.TDecal(type.Type), grp.Name);
                    }
                }
            }
        }
    }

    public async Task<string> BuildAsync(ModMakerInfo mod)
    {
        SanitizeValues(mod);

        var id = mod.Id;
        var modFolder = GetModFolder(id);
        Directory.CreateDirectory(modFolder);

        await WriteManifestAsync(mod, modFolder);

        var namespacedFolder = Path.Combine(modFolder, id);
        var fileList = CopyFiles(mod, namespacedFolder, id);
        await WriteDecalBlueprintsAsync(fileList, namespacedFolder);

        if (mod.HasCustomGroups)
        {
            var locs = await WriteCustomGroupsAsync(mod, namespacedFolder);

            if (locs.Count > 0)
            {
                await WriteLocalizationAsync(locs, modFolder);
            }
        }

        return modFolder;
    }

    static void SanitizeValues(ModMakerInfo mod)
    {
        mod.Id = mod.Id.Trim();
        mod.Name = mod.Name.Trim();
        mod.Version = mod.Version.Trim();

        foreach (var t in mod.DecalInfo)
        {
            foreach (var grp in t.Groups)
            {
                grp.Id = grp.Id.Trim();
                grp.Name = grp.Name.Trim();
            }
        }
    }

    static async Task WriteManifestAsync(ModMakerInfo mod, string folder)
    {
        var path = Path.Combine(folder, "manifest.json");

        await File.WriteAllTextAsync(path, $$"""
{
  "Id": "{{mod.Id}}",  
  "Name": "{{SimpleEscapeJson(mod.Name)}}",
  "Version": "{{mod.Version}}",  
  "MinimumGameVersion": "1.0.0",
  "Description": "",
  "OptionalMods": [
    { "Id": "ModdableDecalGroups" }
  ]
}
""");
    }

    static List<DecalFileList> CopyFiles(ModMakerInfo mod, string folder, string relativePath)
    {
        List<DecalFileList> result = [];

        foreach (var t in mod.DecalInfo)
        {
            if (!t.HasDecal) { continue; }

            List<DecalFileListItem> files = [];

            var srcFolder = Path.Combine(UserDataFolder.Folder, t.Type);

            var dstFolder = Path.Combine(folder, "Images", t.Type);
            var typeRelativeFolder = relativePath + $"/Images/{t.Type}/";
            Directory.CreateDirectory(dstFolder);

            foreach (var grp in t.AllGroups)
            {
                if (!grp.HasDecals) { continue; } // Empty default group

                foreach (var d in grp.Decals)
                {
                    var fileName = d.Id;
                    var srcPath = Path.Combine(srcFolder, fileName);
                    var dstPath = Path.Combine(dstFolder, fileName);

                    File.Copy(srcPath, dstPath, true);

                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    files.Add(new(typeRelativeFolder + fileNameWithoutExt, fileName, d));
                }
            }

            result.Add(new(t, files));
        }

        return result;
    }

    static async Task WriteDecalBlueprintsAsync(List<DecalFileList> fileList, string folder)
    {
        foreach (var (t, files) in fileList)
        {
            var dstFolder = Path.Combine(folder, "Blueprints", "DecalSpecs", t.Type);
            Directory.CreateDirectory(dstFolder);

            foreach (var f in files)
            {
                var path = Path.Combine(dstFolder, $"{f.FileName}.blueprint.json");
                await File.WriteAllTextAsync(path, $$"""
{
  "DecalSpec": {
    "FactionId": "{{f.DecalInfo.FactionId}}",
    "Category": "{{t.Type}}",
    "Texture": "{{f.AssetPath}}"
  }
}
""");
            }
        }
    }

    static async Task<List<(string, string)>> WriteCustomGroupsAsync(ModMakerInfo mod, string namespacedFolder)
    {
        List<(string, string)> locEntries = [];

        var id = mod.Id;

        foreach (var t in mod.DecalInfo)
        {
            if (!t.HasCustomGroups) { continue; }

            var dstFolder = Path.Combine(namespacedFolder, "Blueprints", "DecalGroupSpec", t.Type);
            Directory.CreateDirectory(dstFolder);

            foreach (var grp in t.Groups)
            {
                var locKey = $"{id}.Group.{t.Type}.{grp.Id}";
                locEntries.Add((locKey, grp.Name));

                var path = Path.Combine(dstFolder, $"{grp.Id}.blueprint.json");
                var idList = string.Join(", ", grp.Decals.Select(d => $"\"{d.Id}\""));

                await File.WriteAllTextAsync(path, $$"""
{
    "DecalGroupSpec": {
        "Id": "{{grp.Id}}.{{t.Type}}",
        "TitleLoc": "{{locKey}}",
        "Category": "{{t.Type}}",
        "Order": {{grp.Order}},
        "DecalIdExacts": [{{idList}}]
    }
}
""");
            }
        }

        return locEntries;
    }

    static async Task WriteLocalizationAsync(List<(string, string)> locs, string modFolder)
    {
        var folder = Path.Combine(modFolder, "Localizations");
        Directory.CreateDirectory(folder);

        var dst = Path.Combine(folder, "enUS.csv");
        
        var content = string.Join(Environment.NewLine, locs.Select(l => $"{EscapeCsv(l.Item1)},{EscapeCsv(l.Item2)},"));
        content = $"ID,Text,Comment" + Environment.NewLine + content;

        await File.WriteAllTextAsync(dst, content);
        static string EscapeCsv(string v) => $"\"{v.Replace("\"", "\"\"")}\"";
    }

    static string GetModFolder(string id)
        => Path.Combine(UserDataFolder.Folder, UserFolderModsProvider.ModsDirectoryName, id);

    string GetFileNameError(string fieldKey, string value)
        => t.T("LV.TBMM.ErrFileName", t.T(fieldKey), value, string.Join(" ", Path.GetInvalidFileNameChars()));

    static bool IsEmpty(params string[] values)
        => values.Any(v => string.IsNullOrWhiteSpace(v));

    static bool IsValidFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        if (name.Any(c => invalidChars.Contains(c)))
        {
            return false;
        }
        return true;
    }
    
    static string SimpleEscapeJson(string v) => v.Replace("\"", "\\\"");

    readonly record struct DecalFileList(ModMakerDecalInfo Type, List<DecalFileListItem> Files);
    readonly record struct DecalFileListItem(string AssetPath, string FileName, DecalInfo DecalInfo);
}
