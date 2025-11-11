namespace TransparentTerrain.Services;

public class TransparentTerrainService : ILoadableSingleton, IUnloadableSingleton, IInputProcessor
{
    public const string HoldKeyId = "EnableTerrainTransparency";
    public const string ToggleKeyId = "ToggleTerrainTransparency";
    public const string ConfigureKeyId = "ConfigureTerrainTransparency";

    public static TransparentTerrainService? Instance { get; private set; }

    readonly HashSet<Material> materials = [];
    readonly TransparentShaderService transparentShaderService;
    readonly InputService inputService;
    readonly IContainer container;
    Shader terrainShader = null!;

    public event EventHandler<bool> OnToggled = null!;

    public TransparentTerrainService(
        TransparentShaderService transparentShaderService,
        InputService inputService,
        IContainer container
)
    {
        this.transparentShaderService = transparentShaderService;
        this.inputService = inputService;
        this.container = container;

        Instance = this;
    }

    public bool Enabled { get; private set; }
    public float Alpha { get; private set; } = .25f;

    public void Load()
    {
        terrainShader = Shader.Find("Shader Graphs/TerrainURP")
            ?? throw new InvalidOperationException("Could not find TerrainURP shader!");

        inputService.AddInputProcessor(this);
    }

    public void Toggle(bool? enabled)
    {
        if (enabled == Enabled) { return; }
        var v = enabled ?? !Enabled;
        Enabled = v;

        if (v)
        {
            foreach (var m in materials)
            {
                m.shader = transparentShaderService.TerrainTransparentShader;
                m.SetTerrainAlpha(Alpha);
            }
        }
        else
        {
            foreach (var m in materials)
            {
                m.shader = terrainShader;
            }
        }

        OnToggled?.Invoke(this, Enabled);
    }

    public void SetAlpha(float alpha)
    {
        Alpha = alpha;
        if (!Enabled) { return; }

        foreach (var m in materials)
        {
            m.SetTerrainAlpha(Alpha);
        }
    }

    internal void RegisterMaterial(Material material) => materials.Add(material);

    public void Unload()
    {
        Instance = null;
    }

    public bool ProcessInput()
    {
        if (inputService.IsKeyDown(ConfigureKeyId))
        {
            ShowConfigureDialog();
            return true;
        }

        if (inputService.IsKeyDown(HoldKeyId))
        {
            Toggle(true);
        }
        else if (inputService.IsKeyUp(HoldKeyId))
        {
            Toggle(false);
        }

        return false;
    }

    public void ShowConfigureDialog()
    {
        container.GetInstance<TransparentTerrainDialog>().Show();
    }

}
