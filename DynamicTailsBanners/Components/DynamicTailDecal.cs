namespace DynamicTailsBanners.Components;

[AddTemplateModule2(typeof(TailDecalApplier))]
public class DynamicTailDecal(DynamicDecalService service)
    : DynamicDecalComponentBase<DynamicTailDecal, IDynamicTailDecalProvider>(service), IAwakableComponent
{

#nullable disable
    TailDecalApplier applier;
#nullable enable

    public override Material RendererMaterial => applier._tailDecalTextureSetter._characterMaterialModifier._meshRenderer.material;

    public override void Awake()
    {
        base.Awake();
        applier = GetComponent<TailDecalApplier>();
    }

    protected override void ShowTexture(Texture2D texture)
        => applier._tailDecalTextureSetter.SetTexture(texture);

}
