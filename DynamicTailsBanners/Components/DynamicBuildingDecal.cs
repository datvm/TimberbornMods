namespace DynamicTailsBanners.Components;

[AddTemplateModule2(typeof(DecalSupplierBuildingIcon))]
public class DynamicBuildingDecal(DynamicDecalService service)
    : DynamicDecalComponentBase<DynamicBuildingDecal, IDynamicBannerDecalProvider>(service), IAwakableComponent
{

    DecalSupplierBuildingIcon? buildingIcon;

    public override Material RendererMaterial => buildingIcon!._iconRenderer.material;

    public override void Awake()
    {
        base.Awake();

        if (HasComponent<EnterableTailDecalApplierSpec>()) // Disable on Detailer building
        {
            DisableComponent();
            return;
        }

        buildingIcon = GetComponent<DecalSupplierBuildingIcon>();
    }

    public bool RevalidateAndShowTexture()
    {
        if (!Enabled || !buildingIcon) { return false; }

        var decal = buildingIcon!._decalService.GetValidatedDecal(buildingIcon._decalSupplier.ActiveDecal);
        OnDecalApplied(decal);
        ShowTexture();

        return provider is not null;
    }

    protected override void ShowTexture(Texture2D texture)
        => buildingIcon?._iconRenderer.material.SetTexture(DecalSupplierBuildingIcon.IconPropertyId, texture);
}
