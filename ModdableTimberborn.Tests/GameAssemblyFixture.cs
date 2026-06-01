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

        foreach (var dll in Directory.GetFiles(asmPath, "Timberborn.*.dll"))
        {
            LoadAssembly(dll);
        }
    }

    Assembly? ResolveFromProbePaths(string[] probePaths, AssemblyName assemblyName)
    {
        Log("Resolving {0}", assemblyName.Name);

        foreach (var probePath in probePaths)
        {
            if (!Directory.Exists(probePath))
            {
                Log("Skipped missing probe path: {0}", probePath);
                continue;
            }

            var assembly = Directory
                .EnumerateFiles(probePath, assemblyName.Name + ".dll", SearchOption.AllDirectories)
                .Select(LoadAssembly)
                .FirstOrDefault(x => x is not null);

            if (assembly is not null)
            {
                Log("Resolved {0} from {1}", assemblyName.Name, assembly.Location);
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
            Log("Loaded {0}", assembly.Location);
            return assembly;
        }
        catch (FileLoadException)
        {
            Log("Already loaded {0}", path);
            return null;
        }
    }

    void Log(string message, params object?[] args)
    {
        Console.Error.WriteLine("[assembly-load] " + message, args);
    }
}
