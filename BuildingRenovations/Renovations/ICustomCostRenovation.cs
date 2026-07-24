namespace BuildingRenovations.Renovations;

public interface ICustomCostRenovation
{
    IEnumerable<GoodAmount> GetCost();
}
