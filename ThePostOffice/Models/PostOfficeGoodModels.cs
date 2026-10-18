namespace ThePostOffice.Models;

public record PostOfficeGood(GoodSpec GoodSpec)
{
    public int Amount { get; internal set; }

    public int ItemWeight => GoodSpec.Weight;
    public virtual int TotalWeight => Amount * ItemWeight;
}

public record PostOfficeInventoryGood(GoodSpec GoodSpec, GlobalGoodHandle GlobalGoodHandle)
{
    public int Amount => (int)GlobalGoodHandle.Amount;

    public int ItemWeight => GoodSpec.Weight;
    public int TotalWeight => (int)GlobalGoodHandle.Amount * ItemWeight;
}