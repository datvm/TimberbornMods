namespace ModdableTimberborn.Helpers;

public static class PatchExtensions
{

    public static void PatchAwakePostfix<TComp, TModdedComponent>(this TComp comp)
        where TComp : BaseComponent
        where TModdedComponent : BaseModdableComponent<TComp>
    {
        var modded = comp.GetComponentFast<TModdedComponent>();
        modded.OriginalComponent = comp;

        if (modded is IModdableComponentAwake a)
        {
            a.AwakeAfter();
        }
    }

    public static void PatchStartPostfix<TComp, TModdedComponent>(this TComp comp)
        where TComp : BaseComponent
        where TModdedComponent : BaseModdableComponent<TComp>, IModdableComponentStart
    {
        var modded = comp.GetComponentFast<TModdedComponent>();
        modded.StartAfter();
    }

    public static IEnumerable<CodeInstruction> TranspileAndThrowIfNotFound(this IEnumerable<CodeInstruction> instructions, TranspilerWithFoundDelegate transpiler)
    {
        var found = new StrongBox<bool>();

        foreach (var ins in transpiler(found))
        {
            yield return ins;
        }

        if (!found.Value)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Transpiler did not find the target instruction. Instructions:");
            foreach (var ins in instructions)
            {
                sb.AppendLine($"{ins.opcode}: {ins.operand}");
            }

            throw new InvalidOperationException(sb.ToString());
        }
    }

    public delegate IEnumerable<CodeInstruction> TranspilerWithFoundDelegate(StrongBox<bool> found);
}
