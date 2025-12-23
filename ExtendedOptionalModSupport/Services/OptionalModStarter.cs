
namespace ExtendedOptionalModSupport.Services;

public class OptionalModStarter(ModRepository repo)
{
    public const string OptionalAssemblyFolderName = "OptionalDlls";

    public void StartOptionalMods()
    {
        var asms = LoadAssemblies();
    }

    IEnumerable<Assembly> LoadAssemblies()
    {
        var enabledMods = repo.Mods.Where(q => q.IsEnabled).ToArray();
        var enabledIds = enabledMods.Select(q => q.Manifest.Id).ToHashSet();

        foreach (var m in enabledMods)
        {
            var optionalFolder = Path.Combine(m.ModDirectory.Directory.FullName, OptionalAssemblyFolderName);
            if (!Directory.Exists(optionalFolder)) { continue; }

            foreach (var folder in Directory.GetDirectories(optionalFolder))
            {

            }
        }
    }

}
