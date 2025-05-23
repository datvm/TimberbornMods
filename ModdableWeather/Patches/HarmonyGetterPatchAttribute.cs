namespace ModdableWeather.Patches;

[AttributeUsage(AttributeTargets.Method)]
public class HarmonyGetterPatchAttribute(string name) : Attribute
{

    public string Name { get; } = name;

}
