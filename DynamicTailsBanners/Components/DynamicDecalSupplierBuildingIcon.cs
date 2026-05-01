namespace DynamicTailsBanners.Components;

[AddTemplateModule2(typeof(DecalSupplierBuildingIcon))]
public class DynamicDecalSupplierBuildingIcon(DynamicDecalService service)
    : DynamicDecalComponentBase<DynamicDecalSupplierBuildingIcon, IDynamicBannerDecalProvider>(service), IAwakableComponent
{

    DecalSupplierBuildingIcon? buildingIcon;

    public void Awake()
    {
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
