namespace TimberDump.Services.Dumpers;

public class SpecDumper(ISpecService specs) : IJsonDumper
{

    static readonly MethodInfo GetAllMethod = typeof(ISpecService)
        .GetMethod(nameof(ISpecService.GetSpecs));

    public string? Folder { get; } = "Specs";
    public int Order { get; }

    public void Dump(string folder)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(string Name, Func<object?> Data)> GetDumpData()
    {
        HashSet<string> names = [];
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var asm in assemblies)
        {
            var types = asm.GetTypes()
                .Where(t =>
                    t.IsClass 
                    && !t.IsAbstract 
                    && typeof(ComponentSpec).IsAssignableFrom(t));

            foreach (var t in types)
            {
                var name = t.Name;
                if (names.Contains(name))
                {
                    if (names.Contains(t.FullName))
                    {
                        throw new InvalidOperationException(
                            $"The {t.FullName} was declared more than once. This one is in assembly {asm.FullName}."
                        );
                    }
                    else
                    {
                        Debug.LogWarning($"{name} is already used before, using the full name {t.FullName}");
                        name = t.FullName;
                    }
                }
                names.Add(name);

                yield return (name, () => GetAllSpecs(t));
            }
        }
    }

    object? GetAllSpecs(Type t)
    {
        try
        {
            var result = (IEnumerable<object>) GetAllMethod.MakeGenericMethod(t).Invoke(specs, []);
            return result.Any() ? result : null;
        }
        catch (NullReferenceException)
        {
            Debug.LogWarning($"Skipping {t.FullName} because apparently there is no registered spec for this type.");

            return null;
        }
    }

}
