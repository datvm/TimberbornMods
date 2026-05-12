namespace BeaverChronicles.Services;

[BindSingleton]
public class GoodsHelper(
    FindEntityHelper findEntityHelper    
)
{

    public bool CanGiveToDistrictCenter([NotNullWhen(true)] out DistrictCenter? dc, DistrictCenter? preferred = null)
        => findEntityHelper.FindDistrictCenter(out dc, preferred);

    public bool GiveToDistrictCenter(IEnumerable<GoodAmount> goods, DistrictCenter? preferred = null)
    {
        if (!findEntityHelper.FindDistrictCenter(out var dc, preferred))
        {
            Debug.LogWarning("No district center found to give goods to.");
            return false;
        }

        var inv = dc.GetComponent<SimpleOutputInventory>().Inventory;
        foreach (var g in goods)
        {
            inv.GiveIgnoringCapacity(g);
        }
        return true;
    }

}
