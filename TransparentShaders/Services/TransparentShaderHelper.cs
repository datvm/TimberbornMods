namespace TransparentShaders.Services;

public static class TransparentShaderHelper
{

    public static void SetEnvironmentAlpha(this Material material, float alpha)
    {
        if (material.HasProperty(TransparentShaderService.ShaderEnvironmentAlphaId))
        {
            material.SetFloat(TransparentShaderService.ShaderEnvironmentAlphaId, alpha);
        }
    }

    public static void SetTerrainAlpha(this Material material, float alpha)
    {
        if (material.HasProperty(TransparentShaderService.ShaderTerrainAlphaId))
        {
            material.SetFloat(TransparentShaderService.ShaderTerrainAlphaId, alpha);
        }
    }

    public static void SetColorAlpha(this Material material, float alpha)
    {
        if (material.HasProperty(TransparentShaderService.ShaderColorAlphaId))
        {
            material.SetFloat(TransparentShaderService.ShaderColorAlphaId, alpha);
        }
    }

    public static void SetTerrainLayerToolTopAlpha(this Material material, float alpha)
    {
        if (material.HasProperty(TransparentShaderService.ShaderTerrainLayerToolTopId))
        {
            material.SetFloat(TransparentShaderService.ShaderTerrainLayerToolTopId, alpha);
        }
    }

}
