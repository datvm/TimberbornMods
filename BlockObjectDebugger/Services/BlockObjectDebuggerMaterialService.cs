namespace BlockObjectDebugger.Services;

public class BlockObjectDebuggerMaterialService : ILoadableSingleton, IUnloadableSingleton
{
    static readonly Shader DefaultShader = Shader.Find("Universal Render Pipeline/Lit");
    ImmutableArray<Material> materials = [];

    public Material GetMaterial(BlockOccupations occupation)
    {
        var index = ModUtils.AllOccupations.IndexOf(occupation);
        return materials[index % materials.Length];
    }

    public void Load()
    {
        materials = [..ModUtils.Colors
            .Select(static c =>  new Material(DefaultShader) { color = c })];
    }

    public void Unload()
    {
        foreach (var m in materials)
        {
            UnityEngine.Object.Destroy(m);
        }
    }
}
