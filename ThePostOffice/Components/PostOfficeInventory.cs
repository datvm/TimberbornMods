namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeComponent))]
public class PostOfficeInventory : BaseComponent, IFinishedStateListener, IGoodDisallower
{
    readonly Dictionary<string, int> limits = [];

    public Inventory Inventory { get; private set; } = null!;
    public IReadOnlyDictionary<string, int> Limits => limits;

    public event EventHandler<DisallowedGoodsChangedEventArgs>? DisallowedGoodsChanged;

    public void InitializeInventory(Inventory inventory)
    {
        Inventory = inventory;
    }

    public void OnEnterFinishedState()
    {
        if (Inventory)
        {
            Inventory.Enable();
        }
    }

    public void OnExitFinishedState()
    {
        if (Inventory)
        {
            Inventory.Disable();
        }
    }

    public int AllowedAmount(string goodId) => limits.GetValueOrDefault(goodId);

    public void SetAllowed(IReadOnlyList<GoodAmount> goods)
    {
        Dictionary<string, int> next = [];
        foreach (var good in goods)
        {
            if (good.Amount <= 0)
            {
                continue;
            }

            next[good.GoodId] = next.GetValueOrDefault(good.GoodId) + good.Amount;
        }

        HashSet<string> ids = [..limits.Keys];
        foreach (var id in next.Keys)
        {
            ids.Add(id);
        }

        foreach (var id in ids)
        {
            var amount = next.GetValueOrDefault(id);
            if (limits.GetValueOrDefault(id) == amount)
            {
                continue;
            }

            if (amount == 0)
            {
                limits.Remove(id);
            }
            else
            {
                limits[id] = amount;
            }

            DisallowedGoodsChanged?.Invoke(this, new(id));
        }
    }
}
