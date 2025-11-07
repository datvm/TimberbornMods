namespace DaisugiForestry.Services;

public class TreeYieldMaterialService(
    IAssetLoader assets,
    EventBus eb
) : ILoadableSingleton
{
    static readonly Shader VegetationShader = Shader.Find("Shader Graphs/VegetationURP");
    
    Texture2D yieldDetailTexture = null!;

    public void Load()
    {
        yieldDetailTexture = assets.Load<Texture2D>("Resources/Materials/YieldDetail");
        eb.Register(this);
    }

    public void UpdateMaterials(IEnumerable<Renderer> renderers)
    {
        foreach (var r in renderers)
        {
            var materials = r.materials.ToArray();
            var changed = false;

            foreach (var m in r.materials)
            {
                if (m.shader != VegetationShader) { continue; }

                m.SetTexture("_DetailMap", yieldDetailTexture);
                changed = true;
            }

            if (changed)
            {
                r.materials = materials;
            }
        }
    }

    [OnEvent]
    public void OnEntityInitialized(EntityInitializedEvent e)
    {
        if (!e.Entity.HasComponent<CustomTreeYieldSpec>()) { return; }

        var renderers = e.Entity.Transform.GetComponentsInChildren<Renderer>(true);
        UpdateMaterials(renderers);
    }

}
