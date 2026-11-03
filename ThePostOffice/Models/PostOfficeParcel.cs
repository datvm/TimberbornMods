namespace ThePostOffice.Models;

public readonly record struct PostOfficeParcel(ImmutableArray<GoodAmount> Goods, bool Auto);

public readonly record struct PostOfficeAutoRule(string GoodId, int Target, int Margin);

public readonly record struct PostOfficeHold(int Id, bool Incoming, int Weight, ImmutableArray<GoodAmount> Goods);
