namespace TimberDump.Services;

public class DumpService(
    IEnumerable<IDumper> dumpers,
    DialogBoxShower diag,
    IExplorerOpener opener
)
{
    public static string DumpFolder = Path.Combine(UserDataFolder.Folder, nameof(TimberDump));

    static readonly JsonSerializerSettings jsonSettings = new()
    {
        ContractResolver = new JsonIgnoreUnityObjectContract(),
        Formatting = Newtonsoft.Json.Formatting.Indented,
    };

    public void Dump() => Dump(false);
    public void Dump(bool force)
    {
        if (!force)
        {
            if (!ConfirmDump()) { return; }
        }

        Directory.CreateDirectory(DumpFolder);
        foreach (var dumper in dumpers.OrderBy(q => q.Order))
        {
            Debug.Log($"Dumping {dumper.GetType().FullName}");

            var outputFolder = dumper.Folder is null ? DumpFolder : Path.Combine(DumpFolder, dumper.Folder);
            Directory.CreateDirectory(outputFolder);

            if (dumper is IJsonDumper jsonDumper)
            {
                DumpJson(jsonDumper, outputFolder);
            }
            else
            {
                dumper.Dump(outputFolder);
            }
        }

        OpenFolder();
    }

    void DumpJson(IJsonDumper dumper, string outputFolder)
    {
        foreach (var (n, dataFunc) in dumper.GetDumpData())
        {
            var name = dumper.Folder is null ? n : $"{dumper.Folder}/{n}";

            Debug.Log($"  Dumping {name}...");

            var data = dataFunc();
            if (data is null) { continue; }

            var filePath = Path.Combine(outputFolder, $"{n}.json");
            var json = JsonConvert.SerializeObject(data, jsonSettings);
            File.WriteAllText(filePath, json);
        }
    }

    bool ConfirmDump()
    {
        if (Directory.Exists(DumpFolder))
        {
            diag.Create()
                .SetMessage("The dump folder exists. Certain data may be overwritten and old data that should not be there anymore may persist. You can delete the folder yourself before continue. Do you want to continue?")
                .SetConfirmButton(() => Dump(true))
                .SetDefaultCancelButton()
                .SetInfoButton(OpenFolder, "Open folder")
                .Show();

            return false;
        }

        return true;
    }

    void OpenFolder()
    {
        opener.OpenDirectory(DumpFolder);
    }

}
