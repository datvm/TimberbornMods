namespace ConveyorBelt.Components;

class DefaultIgnoredBeltInventory : BaseComponent, IIgnoredBeltInventory, IAwakableComponent
{
    ConstructionSite site = null!;
    RecoveredGoodStack? rgs;

    public void Awake()
    {
        site = GetComponent<ConstructionSite>();
        rgs = this.GetComponentOrNull<RecoveredGoodStack>();
    }

    public IEnumerable<Inventory> GetIgnoredInventories()
    {
        if (site.Inventory is var inv && inv)
        {
            yield return inv;
        }

        if (rgs && rgs!.Inventory is var rgsInv && rgsInv)
        {
            yield return rgsInv;
        }
    }
}
