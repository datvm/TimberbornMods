namespace PowerCopy.Services;

public class PowerCopyService
{

    static readonly Dictionary<Type, bool> ValidDuplicableTypeCache = [];
    static readonly Dictionary<Type, MethodInfo> DuplicateMethodCache = [];
    public static bool IsValidDuplicableType(Type type)
    {
        if (!ValidDuplicableTypeCache.TryGetValue(type, out bool result))
        {
            ValidDuplicableTypeCache[type] = result = PerformCheck(type);
        }
        return result;

        static bool PerformCheck(Type type)
        {
            if (!type.IsClass || type.IsAbstract)
            {
                return false;
            }

            foreach (var i in type.GetInterfaces())
            {
                if (i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IDuplicable<>)
                    && i.GetGenericArguments()[0] == type)
                {
                    return true;
                }
            }

            return false;
        }
    }

    static void ExecuteDuplicate(IDuplicable src, IDuplicable dst, Type type)
    {
        var method = DuplicateMethodCache.GetOrAdd(type, () =>
        {
            var interfaceType = typeof(IDuplicable<>).MakeGenericType(type);
            return interfaceType.GetMethod(nameof(IDuplicable<>.DuplicateFrom));
        });

        method.Invoke(dst, [src]);
    }

    readonly List<IDuplicable> internalDupsForCache = [];
    readonly Dictionary<string, FrozenSet<Type>> templateCache = [];
    FrozenSet<Type> GetDuplicableTypesOf(BaseComponent comp) => templateCache.GetOrAdd(
        comp.GetTemplateName(), () =>
        {
            comp.GetComponents(internalDupsForCache);

            FrozenSet<Type> types = [..internalDupsForCache
                .Select(d => d.GetType())
                .Where(t => t.IsValidDuplicableType())];

            internalDupsForCache.Clear();

            return types;
        });

    public void GetDuplicables(BaseComponent component, List<IDuplicable> result, HashSet<Type>? filtered = null)
    {
        var types = GetDuplicableTypesOf(component);
        component.GetComponents(result);

        for (int i = result.Count - 1; i >= 0; i--)
        {
            var d = result[i];
            var t = d.GetType();

            if (!types.Contains(t) || !d.IsDuplicable
                || (filtered is not null && !filtered.Contains(t)))
            {
                result.RemoveAt(i);
            }
        }
    }

    readonly List<IDuplicable> validDuplicables = [];
    public bool HasAnyValidDuplicables(BaseComponent component, HashSet<Type> expectedTypes)
    {
        GetDuplicables(component, validDuplicables, expectedTypes);

        var result = validDuplicables.Count > 0;
        validDuplicables.Clear();

        return result;
    }

    readonly List<IDuplicable> srcDuplicables = [];
    readonly List<IDuplicable> targetDuplicables = [];
    public void Duplicate(BaseComponent source, HashSet<Type> types, IEnumerable<EntityComponent> targets)
    {
        if (!source) { return; }

        GetDuplicables(source, srcDuplicables, types);
        var srcDict = srcDuplicables.ToDictionary(d => d.GetType());

        foreach (var target in targets)
        {
            GetDuplicables(target, targetDuplicables, types);

            foreach (var dstComp in targetDuplicables)
            {
                var t = dstComp.GetType();
                var srcComp = srcDict[t];

                ExecuteDuplicate(srcComp, dstComp, t);
            }

            targetDuplicables.Clear();
        }

        srcDuplicables.Clear();
    }

}