namespace TimberUi.CommonProviders;

public class AlertFragmentProvider<T>(T fragment) : IProvider<AlertPanelModule>
    where T : IAlertFragmentWithOrder
{

    public AlertPanelModule Get()
    {
        AlertPanelModule.Builder builder = new();
        builder.AddAlertFragment(fragment, fragment.Order);
        return builder.Build();
    }
}
