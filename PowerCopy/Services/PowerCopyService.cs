namespace PowerCopy.Services;

public static class PowerCopyService
{

    static readonly List<IDuplicable> duplicables = [];
    static readonly Dictionary<Type, bool> validTypeCache = [];
    public static void GetDuplicables(BaseComponent component, List<IDuplicable> result)
    {
        component.GetComponents(duplicables);

        result.Clear();
        foreach (var d in duplicables)
        {
            var type = d.GetType();
            
            if (!validTypeCache.TryGetValue(type, out var isValid))
            {
                isValid = IsValidType(type);
                validTypeCache[type] = isValid;
            }

            if (isValid)
            {
                result.Add(d);
            }
        }

        duplicables.Clear();

        static bool IsValidType(Type type)
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

    static readonly MethodInfo GetComponentMethod = typeof(BaseComponent).GetMethod(nameof(BaseComponent.GetComponent));
    // types is assumed to be only valid types got from GetDuplicables
    public static void Duplicate(BaseComponent source, IEnumerable<Type> types, IEnumerable<EntityComponent> targets)
    {
        if (!source) { return; }

        List<(Type T, BaseComponent Source, MethodInfo Method)> sourceComps = [];

        foreach (var t in types)
        {
            var src = (BaseComponent)GetComponentMethod.MakeGenericMethod(t).Invoke(source, []);
            if (!src || !((IDuplicable)src).IsDuplicable) { continue; }

            var method = typeof(IDuplicable<>).MakeGenericType(t).GetMethod(nameof(IDuplicable<>.DuplicateFrom));
            sourceComps.Add((t, src, method));
        }

        foreach (var dst in targets)
        {
            if (!dst) { continue; }

            foreach (var (t, src, method) in sourceComps)
            {
                var dstComp = (BaseComponent)GetComponentMethod.MakeGenericMethod(t).Invoke(dst, []);
                if (!dstComp) { continue; }
                
                method.Invoke(src, [dstComp]);
            }
        }
    }

}