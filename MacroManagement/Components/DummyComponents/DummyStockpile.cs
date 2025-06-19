namespace MacroManagement.Components.DummyComponents;

public class DummyStockpile : Stockpile, IDummyComponent<DummyStockpile, Stockpile>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public void Init(Stockpile original)
    {
        Inventory = original.Inventory;
        _stockpileSpec = original._stockpileSpec;
        enabled = true;
    }
}
