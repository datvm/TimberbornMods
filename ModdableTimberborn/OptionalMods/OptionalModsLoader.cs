namespace ModdableTimberborn.OptionalMods;

public static class OptionalModsLoader
{

    public const string Prefix = ".dll.";
    public const string Suffix = ".optional";

    public static void Load(ModRepository modRepository)
    {
        LoadAndStartMods(modRepository);
    }

    static void LoadAndStartMods(ModRepository modRepository)
    {
        Debug.Log($"[{OptionalModStarter.OptionalModCategory}] Loading optional DLLs:");
        var counter = 0;

        Dictionary<Mod, List<Assembly>> loadedAssemblies = [];

        var enabledMods = modRepository.EnabledMods.ToArray();
        var enabledIds = enabledMods.Select(m => m.Manifest.Id).ToHashSet();

        foreach (var m in enabledMods)
        {
            List<Assembly> list = [];

            var files = m.ModDirectory.Directory.GetFiles($"*{Prefix}*{Suffix}", SearchOption.AllDirectories);
            if (files.Length == 0) { continue; }

            TimberUiUtils.LogVerbose(() => $"- {m.Manifest.Name} has {files.Length} optional DLLs in {m.ModDirectory.Directory.FullName}:");

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var modId = GetModId(file.Name);
                var hasId = enabledIds.Contains(modId);

                TimberUiUtils.LogVerbose(() => $"  - {file.Name} only loads if mod with id '{modId}' is enabled, which is: {hasId}");

                if (!hasId) { continue; }

                var bytes = File.ReadAllBytes(file.FullName);
                var assembly = Assembly.Load(bytes);
                list.Add(assembly);
                counter++;
            }

            if (list.Count > 0)
            {
                loadedAssemblies[m] = list;
            }
        }

        Debug.Log($"[{OptionalModStarter.OptionalModCategory}] Loaded {counter} optional DLL(s). Starting mods:");
        StartMods(loadedAssemblies, enabledMods);
    }

    static string GetModId(string name)
    {
        name = name[..^Suffix.Length];
        var start = name.IndexOf(Prefix) + Prefix.Length;

        return name[start..];
    }

    static void StartMods(Dictionary<Mod, List<Assembly>> assemblies, Mod[] enabledMods)
    {
        foreach (var m in enabledMods)
        {
            if (!assemblies.TryGetValue(m, out var list)) { continue; }

            var env = ModEnvironment.Create(m);
            foreach (var assembly in list)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!typeof(IModStarter).IsAssignableFrom(type)) { continue; }
                    if (type.IsInterface || type.IsAbstract) { continue; }

                    TimberUiUtils.LogVerbose(() => $"- Starting {assembly.FullName}");

                    var starter = (IModStarter)Activator.CreateInstance(type);
                    starter.StartMod(env);
                }
            }
        }
    }

}
