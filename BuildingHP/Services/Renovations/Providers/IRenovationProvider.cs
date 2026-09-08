namespace BuildingHP.Services.Renovations;

public interface IRenovationProvider
{
    public string Id { get; }
    RenovationSpec RenovationSpec { get; set; }

    string? CanRenovate(BuildingRenovationComponent building);
    VisualElement CreateUI(BuildingRenovationComponent building);
    void ClearUI(VisualElement? currentUI);

    void Save(BuildingRenovationComponent comp, BuildingRenovation renovation, IEntitySaver saver);
    void Load(BuildingRenovationComponent comp, IEntityLoader loader);

}
