namespace DistroStorage.Components.InventoryDistros;

[AddTemplateModule2(typeof(ConstructionSite))]
public class ConstructionSiteDistroReceiver(DistroService service) : InventoryDistroReceiverBase(service)
{

#nullable disable
    ConstructionSite constructionSite;
#nullable enable

    public override Inventory Inventory => constructionSite.Inventory;
    public override bool DisabledBySetting => service.DisableConstruction;
    public override bool EnabledByDefault => service.ConstructionEnableDefault;
    public override bool RequireFinishedBuilding => false;

    protected override bool CalculateActive() => base.CalculateActive() && !Inventory.IsFull;

    public override void Awake()
    {
        base.Awake();

        constructionSite = GetComponent<ConstructionSite>();
    }
}
