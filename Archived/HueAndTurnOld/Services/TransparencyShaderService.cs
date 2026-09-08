namespace HueAndTurn.Services;

public class TransparencyShaderService(
    IAssetLoader assets
) : ILoadableSingleton
{

#nullable disable
    Shader transparentShader;
#nullable enable

    public void Load()
    {
        transparentShader = assets.Load<Shader>("Shader/TransparentEnvironmentURP");
    }

    public Material CreateMaterial(Material original)
    {
        // Remove emission if needed
        original.SetColor(MaterialColorer.EmissionColorProperty, MaterialColorer.UnhighlightedColor);

        return new(original)
        {
            shader = transparentShader
        };
    }

    public IEnumerable<Material> ReplaceMaterials(GameObject gameObj)
    {
        var renderers = gameObj.GetComponentsInChildren<Renderer>(true);
        var entityMaterials = gameObj.GetComponentInParent<EntityMaterials>(true);

        foreach (var r in renderers)
        {
            if (r.name.Contains("GoodVisualization")) // Prevent losing good boxes.
            {
                continue;
            }

            var transform = r.transform;
            var materials = r.materials.ToArray();
            var needReplacement = false;

            for (int i = 0; i < materials.Length; i++)
            {
                var m = materials[i];
                if (m.shader == transparentShader)
                {
                    yield return m;
                    continue;
                }

                if (!m.shader.name.Contains(HueAndTurnComponent.EnvironmentURPName)) { continue; }
                needReplacement = true;

                var replacement = materials[i] = CreateMaterial(m);

                if (entityMaterials)
                {
                    entityMaterials.AddMaterial(transform, replacement);
                    entityMaterials.DestroyMaterial(m);
                }
                else
                {
                    UnityEngine.Object.Destroy(m);
                }


                yield return replacement;
            }

            if (needReplacement)
            {
                r.materials = materials;
            }
        }

    }

}
