namespace BuildingHP.Helpers;

public static class BuildingHPHelpers
{

    public static BuildingHPComponent GetHPComponent<T>(this T comp) where T : BaseComponent
        => comp.GetComponentFast<BuildingHPComponent>();

    public static BuildingRenovationComponent GetRenovationComponent<T>(this T comp) where T : BaseComponent
        => comp.GetComponentFast<BuildingRenovationComponent>();

    public static void ActivateIfAvailable<T>(this T comp, string id)
        where T : BaseComponent, IActivableRenovationComponent
        => ActivateIfAvailable(comp, id, true);

    static void ActivateIfAvailable<T>(this T comp, string id, bool listenIfNotActive)
        where T : BaseComponent, IActivableRenovationComponent
    {
        if (comp.RenovationActive) { return; }

        var reno = comp.GetRenovationComponent();
        var active = reno.HasRenovation(id);

        if (active)
        {
            if (comp.ActiveHandler is not null)
            {
                reno.RenovationCompleted -= comp.ActiveHandler;
                comp.ActiveHandler = null;
            }

            comp.Activate();
        }
        else if (listenIfNotActive)
        {
            void ActiveHandler(BuildingRenovation reno) => comp.ActivateIfAvailable(id, false);

            reno.RenovationCompleted += ActiveHandler;
            comp.ActiveHandler = ActiveHandler;
        }
    }

}
