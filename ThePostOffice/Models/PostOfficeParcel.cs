namespace ThePostOffice.Models;

public readonly record struct PostOfficeParcel(IReadOnlyList<GoodAmount> Goods, bool Auto);
