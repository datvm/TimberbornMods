namespace RealStars.Services;

public class RealStarMaterialService(IAssetLoader assets)
{

    Material? realStarSkyboxMaterial;
    Material? realStarNamedSkyboxMaterial;

    public Material RealStarSkyboxMaterial => GetOrLoad(ref realStarSkyboxMaterial, "StarSkybox04/StarSkybox04");
    public Material RealStarNamedSkyboxMaterial => GetOrLoad(ref realStarNamedSkyboxMaterial, "StarSkybox04Named/StarSkybox04Named");

    Material GetOrLoad(ref Material? material, string path)
    {
        if (!material)
        {
            material = assets.Load<Material>(path);

            if (!material)
            {
                throw new Exception($"Failed to load material at path: {path}");
            }
        }

        return material;
    }

}
