namespace BuildingHP.Helpers;

public static class BuildingHPHelpers
{

    public static BuildingHPComponent GetHPComponent<T>(this T comp) where T : BaseComponent
        => comp.GetComponentFast<BuildingHPComponent>();

    public static BuildingRenovationComponent GetRenovationComponent<T>(this T comp) where T : BaseComponent
        => comp.GetComponentFast<BuildingRenovationComponent>();

}
