using Timberborn.Characters;

namespace TestMod;

[HarmonyPatch]
public static class TestPatch
{

    static readonly ImmutableArray<Color> Colors = [Color.red, Color.gray, Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta];

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterTint), nameof(CharacterTint.SetTint))]
    public static void Patch1(ref Color tintColor)
    {
        tintColor = Colors[UnityEngine.Random.Range(0, Colors.Length)];
    }

}
