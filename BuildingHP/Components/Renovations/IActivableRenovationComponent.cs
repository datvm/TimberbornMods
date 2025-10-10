namespace BuildingHP.Components.Renovations;

public interface IActivableRenovationComponent
{
    bool RenovationActive { get; }
    Action<BuildingRenovation>? ActiveHandler { get; set; }
    void Activate();
}
