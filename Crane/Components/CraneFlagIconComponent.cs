namespace Crane.Components;

[AddTemplateModule2(typeof(CraneComponent))]
public class CraneFlagIconComponent(
    NamedIconProvider namedIconProvider
) : BaseComponent, IAwakableComponent, IFinishedStateListener, IInitializablePreview
{
    static readonly int TextureProperty = Shader.PropertyToID("_DetailAlbedoMap2");
    static readonly int IconColorProperty = Shader.PropertyToID("_DetailAlbedoUV2Color");
    static readonly Color BannerIconColor = new(0.12f, 0.12f, 0.12f);

    static Texture2D? stampTexture;

    BuildingModel buildingModel = null!;

    public void Awake()
    {
        buildingModel = GetComponent<BuildingModel>();
    }

    public void InitializePreview()
    {
        ApplyIcon();
    }

    public void OnEnterFinishedState()
    {
        ApplyIcon();
    }

    public void OnExitFinishedState() { }

    void ApplyIcon()
    {
        var finished = buildingModel.FinishedModel;
        if (!finished)
        {
            return;
        }

        MeshRenderer? renderer = null;
        foreach (var candidate in finished.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (candidate.name == "Pile")
            {
                renderer = candidate;
                break;
            }
        }

        renderer ??= finished.GetComponentInChildren<MeshRenderer>(true);
        if (!renderer)
        {
            return;
        }

        var material = renderer.material;
        material.SetTexture(TextureProperty, GetStampTexture());
        material.SetColor(IconColorProperty, BannerIconColor);
    }

    Texture2D GetStampTexture()
    {
        if (stampTexture)
        {
            return stampTexture;
        }

        var source = namedIconProvider
            .GetOrLoad("CraneFlagIcon", "Buildings/DistrictManagement/Crane/CraneFlagIcon")
            .texture;

        // Banner UV2 treats light pixels as the print. The PNG is a dark silhouette,
        // so invert it to a white stamp and tint it dark.
        var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);
        var previous = RenderTexture.active;
        RenderTexture.active = rt;
        var stamp = new Texture2D(source.width, source.height, TextureFormat.RGBA32, mipChain: false)
        {
            name = "CraneFlagStamp",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        stamp.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        var pixels = stamp.GetPixels();
        for (var i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            var luminance = (pixel.r * 0.299f) + (pixel.g * 0.587f) + (pixel.b * 0.114f);
            var mask = pixel.a * (1f - luminance);
            pixels[i] = new Color(mask, mask, mask, mask);
        }

        stamp.SetPixels(pixels);
        stamp.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        stampTexture = stamp;
        return stamp;
    }
}
