namespace TestMod;

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {

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
        Bind<TestComponent>().AsTransient();
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<Building, TestComponent>();
            return b.Build();
        }).AsSingleton();
    }
}

class TestComponent : BaseComponent, IInitializableEntity
{
    TransparentShaderService transparentShaderService = null!;

    [Inject]
    public void Inject(TransparentShaderService transparentShaderService)
    {
        this.transparentShaderService = transparentShaderService;
    }

    public void InitializeEntity()
    {
        var template = GetComponent<TemplateSpec>();
        if (!template.TemplateName.Contains("Levee")) { return; }

        var renderers = GameObject.GetComponentsInChildren<Renderer>(true);
        var materials = transparentShaderService.ReplaceRenderersMaterials(renderers);
        foreach (var m in materials)
        {
            Debug.Log($"TEST: replacing {m}'s shader to {m.shader.name}");
            m.SetEnvironmentAlpha(.5f);

            Debug.Log($"TEST: alpha: {m.GetFloat("_EnvironmentAlpha")}");
        }

        var litMaterial = new Material(transparentShaderService.PlainColorTransparentShader);
        var unlitMaterial = new Material(transparentShaderService.UnlitPlainColorTransparentShader);
        litMaterial.color = unlitMaterial.color = Color.cornflowerBlue;
        litMaterial.SetColorAlpha(.5f);
        unlitMaterial.SetColorAlpha(.5f);

        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = Transform;
        sphere.transform.localPosition = new(2.5f, .5f, .5f);

        {
            var renderer = sphere.GetComponent<Renderer>();
            renderer.material = litMaterial;
        }

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = Transform;
        sphere.transform.localPosition = new(5.5f, .5f, .5f);
        {
            var renderer = sphere.GetComponent<Renderer>();
            renderer.material = unlitMaterial;
        }

    }
}
