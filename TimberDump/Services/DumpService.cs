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
        Formatting = Formatting.Indented,
    };

    public void Dump()
    {
        if (!ConfirmDump()) { return; }

        Dictionary<string, Type> names = [];
        Directory.CreateDirectory(DumpFolder);

        foreach (var dumper in dumpers)
        {
            var outputFolder = dumper.Folder is null ? DumpFolder : Path.Combine(DumpFolder, dumper.Folder);
            Directory.CreateDirectory(outputFolder);

            foreach (var (n, dataFunc) in dumper.GetDumpData())
            {
                var name = dumper.Folder is null ? n : $"{dumper.Folder}/{n}";

                if (names.TryGetValue(name, out var type))
                {
                    throw new InvalidOperationException(
                        $"Duplicate dumper name found: {name}. " +
                        $"Types: {type.FullName} and {dumper.GetType().FullName}"
                    );
                }
                names[name] = dumper.GetType();

                Debug.Log($"Dumping {name}...");

                var data = dataFunc();
                if (data is null) { continue; }

                var filePath = Path.Combine(outputFolder, $"{n}.json");
                var json = JsonConvert.SerializeObject(data, jsonSettings);
                File.WriteAllText(filePath, json);
            }

            
        }

        OpenFolder();
    }

    bool ConfirmDump()
    {
        if (Directory.Exists(DumpFolder))
        {
            diag.Create()
                .SetMessage("The dump folder exists. It will be deleted if you proceed. Are you sure?")
                .SetConfirmButton(() =>
                {
                    Directory.Delete(DumpFolder, true);
                    Dump();
                })
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
