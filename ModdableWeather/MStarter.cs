namespace ModdableWeather;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(ModdableWeather));
        harmony.PatchAll();

        ScanForAttributes(harmony);
    }

    public static void ScanForAttributes(Harmony harmony)
    {
        var types = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Select(q => (q, q.GetCustomAttribute<HarmonyPatch>()))
            .Where(q => q.Item2?.info.declaringType is not null);

        foreach (var (type, attr) in types)
        {
            if (!(type.IsAbstract && type.IsSealed))
            {
                throw new InvalidOperationException(
                    $"Type {type} is not static. Harmony patching requires static classes.");
            }

            var originalType = attr.info.declaringType;
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods)
            {
                PatchGetter(harmony, method, originalType);
            }
        }
    }

    static void PatchGetter(Harmony harmony, MethodInfo method, Type original)
    {
        var getterAttr = method.GetCustomAttribute<HarmonyGetterPatchAttribute>();
        if (getterAttr is null) { return; }

        var propGetter = original.PropertyGetter(getterAttr.Name)
            ?? throw new InvalidOperationException(
                $"Property {getterAttr.Name} not found on {original}");

        harmony.Patch(propGetter, prefix: method);
    }

}
