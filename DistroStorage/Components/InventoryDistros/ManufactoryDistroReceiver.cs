namespace DistroStorage.Components.InventoryDistros;

[AddTemplateModule2(typeof(Manufactory))]
public class ManufactoryDistroReceiver(DistroService service) : InventoryDistroReceiverBase(service), IDuplicable<ManufactoryDistroReceiver>
{

#nullable disable
    Manufactory manufactory;
#nullable enable

    public override Inventory Inventory => manufactory.Inventory;
    public override bool SystemDisabled => base.SystemDisabled || service.DisableManufactory;
    public override bool EnabledByDefault => service.ManufactoryEnableDefault;

    public override void Awake()
    {
        manufactory = GetComponent<Manufactory>();
        base.Awake();
    }

    public void DuplicateFrom(ManufactoryDistroReceiver source) => Deserialize(source.Serialize());
}
