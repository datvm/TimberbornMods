namespace DynamicTailsBanners.Components;

[AddTemplateModule2(typeof(TailDecalApplier))]
public class DynamicTailDecalApplier(DynamicDecalService service)
    : DynamicDecalComponentBase<DynamicTailDecalApplier, IDynamicTailDecalProvider>(service), IAwakableComponent
{

#nullable disable
    TailDecalApplier applier;
#nullable enable

    public void Awake()
    {
        applier = GetComponent<TailDecalApplier>();
    }

    protected override void ShowTexture(Texture2D texture)
        => applier._tailDecalTextureSetter.SetTexture(texture);

}
