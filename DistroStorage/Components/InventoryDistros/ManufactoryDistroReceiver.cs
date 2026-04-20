namespace DistroStorage.Components.InventoryDistros;

[AddTemplateModule2(typeof(Manufactory))]
public class ManufactoryDistroReceiver(DistroService service) : InventoryDistroReceiverBase(service)
{

#nullable disable
    Manufactory manufactory;
#nullable enable

    public override Inventory Inventory => manufactory.Inventory;
    public override bool DisabledBySetting => service.DisableManufactory;
    public override bool EnabledByDefault => service.ManufactoryEnableDefault;

    public override void Awake()
    {
        manufactory = GetComponent<Manufactory>();
        base.Awake();
    }

}
