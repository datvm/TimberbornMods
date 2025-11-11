namespace TransparentShaders.Services;

public class TransparentShaderServiceUnloader(TransparentShaderService service) : IUnloadableSingleton
{
    public void Unload() => service.OnSceneUnload();
}
