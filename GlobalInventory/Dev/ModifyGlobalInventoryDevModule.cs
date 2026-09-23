namespace GlobalInventory.Dev;

[MultiBind(typeof(IDevModule))]
public class ModifyGlobalInventoryDevModule(IContainer container) : IDevModule
{
    public DevModuleDefinition GetDefinition()
        => new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Global Inventory: Modify", Open))
            .Build();

    void Open() => container.GetInstance<ModifyGlobalInventoryDialog>().Show();
}
