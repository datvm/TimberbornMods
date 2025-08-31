namespace BuildingHP.Components.Renovations;

public interface IActiveRenovationDescriber
{

    ActiveRenovationDescription? Describe(ILoc t, IDayNightCycle dayNightCycle);

}

public readonly record struct ActiveRenovationDescription(string Title, string Description, float? RemainingTime = null);