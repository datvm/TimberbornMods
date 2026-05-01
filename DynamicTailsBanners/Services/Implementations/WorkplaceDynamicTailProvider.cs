namespace DynamicTailsBanners.Services.Implementations;

[MultiBind(typeof(IDynamicDecalProvider))]
public class WorkplaceDynamicTailProvider(IAssetLoader assets) : IDynamicTailDecalProvider, IUnloadableSingleton
{
    public string Id => "dynamic-tail-workplaces";
    const float PaddingPercent = .3f;

    Texture2D? unemployed;

    Texture2D UnemployedTexture => unemployed ??= assets.Load<Texture2D>("Sprites/StatusIcons/NothingToDo");
    readonly Dictionary<string, Texture2D> cached = [];

    public Texture2D GetTexture(DynamicTailDecalApplier comp)
    {
        var worker = comp.GetComponent<Worker>();
        if (!worker) { return UnemployedTexture; }

        var workplace = worker.Workplace;
        if (!workplace) { return UnemployedTexture; }

        return GetTexture(workplace);
    }

    Texture2D GetTexture(Workplace workplace)
    {
        var templateName = workplace.GetTemplateName();

        if (cached.TryGetValue(templateName, out var cachedTexture))
        {
            return cachedTexture;
        }

        var original = workplace.GetComponent<LabeledEntity>().Image.texture;
        var w = original.width;
        var h = original.height;
        
        var paddingW = (int)(w * PaddingPercent);
        var paddingH = (int)(h * PaddingPercent);

        var newW = w + paddingW * 2;
        var newH = h + paddingH * 2;

        var paddedTexture = new Texture2D(
            newW,
            newH,
            TextureFormat.RGBA32,
            mipChain: false);

        // Fill background with transparent pixels in one bulk write.
        var clearPixels = new Color32[newW * newH];
        paddedTexture.SetPixels32(clearPixels);
        paddedTexture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

        // Fast GPU copy into the padded texture.
        Graphics.CopyTexture(
            original,
            srcElement: 0,
            srcMip: 0,
            srcX: 0,
            srcY: 0,
            srcWidth: w,
            srcHeight: h,
            paddedTexture,
            dstElement: 0,
            dstMip: 0,
            dstX: paddingW,
            dstY: paddingH);

        return cached[templateName] = paddedTexture;
    }

    public void Register(DynamicTailDecalApplier comp)
    {
        var worker = comp.GetComponent<Worker>();
        if (!worker) { return; }

        worker.RelationsChanged += UpdateDecal;        
    }

    public void Unregister(DynamicTailDecalApplier comp)
    {
        var worker = comp.GetComponent<Worker>();
        if (!worker) { return; }

        worker.RelationsChanged -= UpdateDecal;
    }

    void UpdateDecal(object sender, EventArgs e) => ((Worker)sender).GetComponent<DynamicTailDecalApplier>().ShowTexture();

    public void Unload()
    {
        foreach (var t in cached.Values)
        {
            UnityEngine.Object.Destroy(t);
        }
        cached.Clear();
    }
}
