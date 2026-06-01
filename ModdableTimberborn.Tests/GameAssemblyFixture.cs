using System.Runtime.Loader;

namespace ModdableTimberborn.Tests;

public sealed class GameAssemblyFixture
{
    public GameAssemblyFixture()
    {
        var metadata = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>();
        var asmPath = metadata
            .First(x => x.Key == "GameAssemblyPath")
            .Value ?? throw new InvalidOperationException("GameAssemblyPath metadata attribute is missing");
        var probePaths = metadata
            .Where(x => x.Key == "AssemblyProbePath")
            .Select(x => x.Value)
            .OfType<string>()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Append(asmPath)
            .Append(AppContext.BaseDirectory)
            .Distinct()
            .ToArray();

        Log("Game assembly path: {0}", asmPath);
        foreach (var probePath in probePaths)
        {
            Log("Assembly probe path: {0}", probePath);
        }

        AssemblyLoadContext.Default.Resolving += (_, assemblyName) => ResolveFromProbePaths(probePaths, assemblyName);

        var loaded = 0;
        var alreadyLoaded = 0;
        foreach (var dll in Directory.GetFiles(asmPath, "Timberborn.*.dll"))
        {
            if (LoadAssembly(dll) is null)
            {
                alreadyLoaded++;
            }
            else
            {
                loaded++;
            }
        }

        Log("Loaded {0} Timberborn assemblies. Already loaded: {1}.", loaded, alreadyLoaded);
    }

    Assembly? ResolveFromProbePaths(string[] probePaths, AssemblyName assemblyName)
    {
        foreach (var probePath in probePaths)
        {
            if (!Directory.Exists(probePath))
            {
                continue;
            }

            var assembly = Directory
                .EnumerateFiles(probePath, assemblyName.Name + ".dll", SearchOption.AllDirectories)
                .Select(LoadAssembly)
                .FirstOrDefault(x => x is not null);

            if (assembly is not null)
            {
                return assembly;
            }
        }

        Log("Could not resolve {0}", assemblyName.Name);
        return null;
    }

    Assembly? LoadAssembly(string path)
    {
        try
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(path));
            return assembly;
        }
        catch (FileLoadException)
        {
            return null;
        }
    }

    void Log(string message, params object?[] args)
    {
        var text = "[assembly-load] " + string.Format(message, args);
        TestContext.Current.SendDiagnosticMessage(text);
    }
}
