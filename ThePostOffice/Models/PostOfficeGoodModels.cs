namespace ThePostOffice.Models;

public record PostOfficeInventoryGood(GoodSpec GoodSpec, GlobalGoodHandle GlobalGoodHandle)
{
    public int Amount => (int)GlobalGoodHandle.Amount;

    public int ItemWeight => GoodSpec.Weight;
    public int TotalWeight => (int)GlobalGoodHandle.Amount * ItemWeight;
}
