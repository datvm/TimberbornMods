namespace TransparentTerrain.Services;

public class TransparentTerrainTopMeshService(
    TransparentTerrainService transparentTerrainService,
    TerrainTopMeshService terrainTopMeshService,
    TransparentShaderService transparentShaderService
) : ILoadableSingleton
{

    Material material = null!;

    public void Load()
    {
        transparentTerrainService.OnChanged += OnChanged;

        var renderer = terrainTopMeshService._topLayerObject.GetComponentInChildren<Renderer>();
        material = renderer.material;

        OnChanged(); // In case always enabled is on
    }

    void OnChanged()
    {
        var alpha = (transparentTerrainService.Enabled || transparentTerrainService.AlwaysEnableTopLayer)
            ? transparentTerrainService.Alpha
            : 1f;

        if (alpha == 1f)
        {
            transparentShaderService.RestoreTerrainLayerToolTopShader(material, true);
        }
        else
        {
            transparentShaderService.ReplaceTerrainLayerToolTopShader(material);
            material.SetTerrainLayerToolTopAlpha(alpha);
        }
    }

}
