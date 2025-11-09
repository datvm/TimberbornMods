namespace TestMod;

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {
        Bind<ShaderService>().AsSingleton().AsExported();
    }
}

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {

    }
}

public class ShaderService(IAssetLoader assets) : IUnloadableSingleton, ILoadableSingleton
{
    Shader? terrainShader;
    
    public static ShaderService Instance { get; private set; } = null!;



    public Shader Shader
    {
        get
        {
            if (!terrainShader)
            {
                terrainShader = assets.Load<Shader>("Shaders/TerrainTransparentURP");
            }

            return terrainShader;
        }
    }

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null!;
    }
}
