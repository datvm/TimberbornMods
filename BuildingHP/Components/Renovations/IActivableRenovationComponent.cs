namespace BuildingHP.Components.Renovations;

public interface IActivableRenovationComponent
{
    bool Active { get; }
    Action<BuildingRenovation>? ActiveHandler { get; set; }
    void Activate();
}
