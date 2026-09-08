namespace BeavlineLogistics.Services;

public class BuildingInventoryProvider
{
    readonly Dictionary<string, BeavlineInventoryList> inventoriesCache = [];

    public BeavlineInventoryList Get(BeavlineComponent comp)
    {
        var prefabName = comp.PrefabName;
        if (!inventoriesCache.TryGetValue(prefabName, out var list))
        {
            inventoriesCache[prefabName] = list = GetInventoryList(comp);
        }

        return list;
    }

    static BeavlineInventoryList GetInventoryList(BeavlineComponent comp)
    {
        Inventory? input = null, output = null;
        TryGetInventory? func = null;
        foreach (var searchFunc in InventoryTypes)
        {
            if (searchFunc(comp, out input, out output))
            {
                func = searchFunc;
                break;
            }
        }

        if (func is null)
        {
            return BeavlineInventoryList.Default;
        }

        var inputGoodIds = GetGoodList(input, false);
        var outputGoodIds = GetGoodList(output, true);        
        return new(inputGoodIds, outputGoodIds, func);
    }

    static FrozenSet<string> GetGoodList(Inventory? inventory, bool takable)
    {
        if (inventory == null) { return []; }

        return [.. inventory.AllowedGoods
            .Where(q => q.Amount > 0
                && ((takable && q.StorableGood.Takeable) || (!takable && q.StorableGood.Givable)))
            .Select(q => q.StorableGood.GoodId)];
    }

    static readonly ImmutableArray<TryGetInventory> InventoryTypes = [
        FindInventory<Stockpile>(c => c.Inventory, c => c.Inventory),
        FindInventory<Manufactory>(c => c.Inventory, c => c.Inventory),
        FindInventory<SimpleOutputInventory>(c => null, c => c.Inventory),
        FindInventory<GoodConsumingBuilding>(c => c.Inventory, c => null),
        FindInventory<BreedingPod>(c => c.Inventory, _ => null),
        FindInventory<WonderInventory>(c => c.Inventory, _ => null),
    ];

    static TryGetInventory FindInventory<T>(Func<T, Inventory?> input, Func<T, Inventory?> output)
        where T : BaseComponent =>
        (b, out inputInv, out outputInv) =>
        {
            var comp = b.GetComponentFast<T>();
            if (comp)
            {
                inputInv = input(comp);
                outputInv = output(comp);
                return true;
            }

            inputInv = null;
            outputInv = null;
            return false;
        };
}

public delegate bool TryGetInventory(BeavlineComponent comp, out Inventory? input, out Inventory? output);
public readonly record struct BeavlineInventoryList(FrozenSet<string> InputIds, FrozenSet<string> OutputIds, TryGetInventory? InventoryFunc)
{
    public static readonly BeavlineInventoryList Default = new([], [], null);
}