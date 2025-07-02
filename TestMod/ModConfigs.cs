using System.Reflection.Emit;

namespace TestMod;

[Context("Game")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {

    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TestMod));
        harmony.PatchAll();

        var t = AccessTools.TypeByName("TimberApi.SpecificationSystem.GeneratedSpecLoader");
        var method = t.Method("PostLoad");

        harmony.Patch(method,
            transpiler: typeof(ModStarter).Method("Patch"));
    }


    static FieldInfo FindCacheField(Type type)
    {
        var field = type.GetField("_cachedBlueprints", AccessTools.all);
        return field is null
            ? type.BaseType == typeof(object) ? throw new FieldAccessException("Field not found") : FindCacheField(type.BaseType)
            : field;
    }

    public static void ClearCache(object __instance)
    {
        var fields = __instance.GetType().GetFields(AccessTools.all);
        var specServiceField = fields.First(q => q.Name.Contains("specService"));
        var specService = (ISpecService) specServiceField.GetValue(__instance);

        var cacheField = FindCacheField(specService.GetType());        
        var cache = cacheField.GetValue(specService);
        var clear = cache.GetType().GetMethod("Clear", AccessTools.all);
        clear.Invoke(cache, null);
    }

    public static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> instructions)
    {
        var list = instructions.ToList();

        var index = list.FindIndex(q => q.opcode == OpCodes.Callvirt && q.operand is MethodInfo method && method.Name == "Clear");

        // Go back two `ldarg.0` instructions
        var startIndex = list.FindLastIndex(index - 1, q => q.opcode == OpCodes.Ldarg_0);
        startIndex = list.FindLastIndex(startIndex - 1, q => q.opcode == OpCodes.Ldarg_0);

        // Clear that part
        // Don't remove the first ldarg.0 as it's a label for a branch
        list.RemoveRange(startIndex + 1, index - startIndex);

        // Insert our ClearCache method
        list.Insert(startIndex + 1, new(OpCodes.Call, typeof(ModStarter).Method(nameof(ClearCache))));

        return list;
    }

}
