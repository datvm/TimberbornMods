namespace BeaverChronicles.Services.Helpers;

[BindSingleton]
public class GoodsHelper(
    FindEntityHelper findEntityHelper,
    ScienceService scienceService,
    IGoodService goodsService
)
{

    public bool CanGiveToDistrictCenter([NotNullWhen(true)] out DistrictCenter? dc, DistrictCenter? preferred = null)
        => findEntityHelper.FindDistrictCenter(out dc, preferred);

    public void ModifyScience(int amount)
    {
        if (amount > 0)
        {
            scienceService.AddPoints(amount);
        }
        else if (amount < 0)
        {
            var subtracting = Math.Min(-amount, scienceService.SciencePoints);
            scienceService.SubtractPoints(subtracting);
        }        
    }

    public bool GiveGoodsAndScience(IEnumerable<GoodAmount> goods, DistrictCenter? preferred = null)
    {
        if (!findEntityHelper.FindDistrictCenter(out var dc, preferred))
        {
            Debug.LogWarning("No district center found to give goods to.");
            return false;
        }

        var inv = dc.GetComponent<SimpleOutputInventory>().Inventory;
        foreach (var g in goods)
        {
            if (g.GoodId.IsScience)
            {
                ModifyScience(g.Amount);
            }
            else if (goodsService.HasGood(g.GoodId))
            {
                inv.GiveIgnoringCapacity(g);
            }
            else
            {
                Debug.LogWarning($"Unknown Good {g.GoodId} to give. Skipping.");
            }
        }
        return true;
    }

}
