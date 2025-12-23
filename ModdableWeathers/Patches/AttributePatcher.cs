namespace ModdableWeathers.Patches;

public static class AttributePatcher
{

    public static void Patches()
    {
        var harmony = new Harmony(nameof(ModdableWeathers));

        foreach (var t in ModdableWeatherConfig.AssemblyTypes)
        {
            if (t.GetCustomAttribute<ReplaceSingletonAttribute>() is null
                && t.GetCustomAttribute<HasPatchAttribute>() is null) { continue; }

            ReplaceProperties(t, harmony);
            ReplaceMethods(t, harmony);
            ReplaceThrowMembers(t, harmony);
            BypassMethods(t, harmony);
        }

        target = null;
    }

    static void ReplaceProperties(Type t, Harmony harmony)
    {
        var properties = GetDeclaredProperties(t).ToDictionary(q => q.Name);
        foreach (var p in properties.Values)
        {
            var attr = p.GetCustomAttribute<ReplacePropertyAttribute>();
            if (attr is null) { continue; }

            var replacedName = attr.ReplacedName ?? p.Name[1..];
            var replacedProp = properties[replacedName].GetDeclaredMember();

            var srcGetter = replacedProp.GetGetMethod(true);
            if (srcGetter is not null)
            {
                var dstGetter = p.GetGetMethod(true);
                if (dstGetter is not null)
                {
                    target = dstGetter;
                    harmony.Patch(srcGetter, transpiler: new(RedirectGetterTranspiler));
                }
            }

            var srcSetter = replacedProp.GetSetMethod(true);
            if (srcSetter is not null)
            {
                var dstSetter = p.GetSetMethod(true);
                if (dstSetter is not null)
                {
                    target = dstSetter;
                    harmony.Patch(srcSetter, transpiler: new(RedirectSetterTranspiler));
                }
            }
        }
    }

    static void ReplaceMethods(Type t, Harmony harmony)
    {
        var methods = GetDeclaredMethods(t).ToDictionary(q => q.Name);

        foreach (var m in methods.Values)
        {
            var attr = m.GetCustomAttribute<ReplaceMethodAttribute>();
            if (attr is null) { continue; }

            var replacedName = attr.ReplacedName ?? m.Name[1..];
            var replacedMethod = methods[replacedName];

            target = m;
            harmony.Patch(replacedMethod, transpiler: new(RedirectMethod));
        }

    }

    static void BypassMethods(Type t, Harmony harmony)
    {
        var attrs = t.GetCustomAttributes<BypassMethodsAttribute>().ToArray();
        if (attrs.Length == 0) { return; }

        var allNames = attrs.SelectMany(q => q.MethodNames).ToHashSet();
        if (allNames.Count == 0) { return; }

        var methods = GetDeclaredMethods(t);
        foreach (var m in methods)
        {
            if (!allNames.Remove(m.Name)) { continue; }

            harmony.Patch(m, transpiler: new(BypassMethod));
        }

        if (allNames.Count > 0)
        {
            throw new InvalidOperationException($"[BypassMethods] could not find methods: {string.Join(", ", allNames)} in {t}");
        }
    }

    static void ReplaceThrowMembers(Type t, Harmony harmony)
    {
        var propertiesAttrs = t.GetCustomAttributes<ThrowPropertiesAttribute>();
        var methodsAttrs = t.GetCustomAttributes<ThrowMethodsAttribute>();

        var props = GetDeclaredProperties(t).ToArray();
        foreach (var propAttr in propertiesAttrs)
        {
            throwAttr = propAttr;
            var expectings = propAttr.Names;

            foreach (var p in props)
            {
                if (!expectings.Remove(p.Name)) { continue; }
                var getter = p.GetGetMethod(true);
                if (getter is not null)
                {
                    harmony.Patch(getter, transpiler: new(RedirectToThrow));
                }

                var setter = p.GetSetMethod(true);
                if (setter is not null)
                {
                    harmony.Patch(setter, transpiler: new(RedirectToThrow));
                }
            }

            if (expectings.Count > 0)
            {
                throw new InvalidOperationException($"[ThrowProperties] could not find properties: {string.Join(", ", expectings)} in {t}");
            }
        }

        var methods = GetDeclaredMethods(t).ToArray();
        foreach (var methodAttr in methodsAttrs)
        {
            throwAttr = methodAttr;
            var expectings = methodAttr.Names;

            foreach (var m in methods)
            {
                if (!expectings.Remove(m.Name)) { continue; }
                harmony.Patch(m, transpiler: new(RedirectToThrow));
            }

            if (expectings.Count > 0)
            {
                throw new InvalidOperationException($"[ThrowMethods] could not find methods: {string.Join(", ", expectings)} in {t}");
            }
        }

        throwAttr = null;
    }

    static MethodInfo? target;
    static IEnumerable<CodeInstruction> RedirectGetterTranspiler(IEnumerable<CodeInstruction> _, MethodBase __originalMethod)
    {
        var dstGetter = target;

        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Callvirt, dstGetter);
        yield return new CodeInstruction(OpCodes.Ret);
    }

    static IEnumerable<CodeInstruction> RedirectSetterTranspiler(IEnumerable<CodeInstruction> _, MethodBase __originalMethod)
    {
        var dstSetter = target;

        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return new CodeInstruction(OpCodes.Callvirt, dstSetter);
        yield return new CodeInstruction(OpCodes.Ret);
    }

    static IEnumerable<CodeInstruction> RedirectMethod(IEnumerable<CodeInstruction> _, MethodBase __originalMethod)
    {
        var dstMethod = target!;
        var paramCount = dstMethod.GetParameters().Length + 1;
        for (int i = 0; i < paramCount; i++)
        {
            yield return new(OpCodes.Ldarg, i);
        }
        yield return new(OpCodes.Callvirt, dstMethod);
        yield return new(OpCodes.Ret);
    }

    static IThrowAttribute? throwAttr;
    static readonly MethodInfo ThrowMethod = typeof(AttributePatcher).Method(nameof(ThrowException))!;
    static IEnumerable<CodeInstruction> RedirectToThrow(IEnumerable<CodeInstruction> _)
    {
        var attr = throwAttr!;
        // Push the parameters
        if (attr.ExceptionType is null)
        {
            yield return new(OpCodes.Ldnull);
        }
        else
        {
            yield return new(OpCodes.Ldtoken, attr.ExceptionType);
            yield return new(OpCodes.Call, typeof(Type).Method("GetTypeFromHandle"));
        }

        if (attr.ExceptionMember is null)
        {
            yield return new(OpCodes.Ldnull);
        }
        else
        {
            yield return new(OpCodes.Ldstr, attr.ExceptionMember);
        }

        yield return new(OpCodes.Call, ThrowMethod);
        yield return new(OpCodes.Throw);
    }

    static IEnumerable<CodeInstruction> BypassMethod(IEnumerable<CodeInstruction> _) => [new(OpCodes.Ret)];

    static Exception ThrowException(Type? exceptionType, string? exceptionMember) =>
        throw (exceptionType is null
            ? new ModdableWeatherNotSupportedException()
            : exceptionMember is null
                ? (Exception)Activator.CreateInstance(exceptionType)
                : (Exception)Activator.CreateInstance(exceptionType, exceptionMember));

    static IEnumerable<PropertyInfo> GetDeclaredProperties(Type t)
        => GetDeclaredMembers(t, t => t.GetProperties(AccessTools.all));
    static IEnumerable<MethodInfo> GetDeclaredMethods(Type t)
        => GetDeclaredMembers(t, t => t.GetMethods(AccessTools.all));

    static IEnumerable<T> GetDeclaredMembers<T>(Type t, Func<Type, IEnumerable<T>> func)
        where T : MemberInfo
    {
        // First, get the base type members
        var baseType = t.BaseType;

        HashSet<string> members = [];
        foreach (var member in func(baseType))
        {
            members.Add(member.Name);
            yield return member.GetDeclaredMember();
        }

        // Then, get the derived type members
        foreach (var member in func(t))
        {
            if (members.Add(member.Name))
            {
                yield return member.GetDeclaredMember();
            }
        }
    }

}
