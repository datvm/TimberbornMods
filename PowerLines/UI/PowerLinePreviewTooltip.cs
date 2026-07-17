namespace PowerLines.UI;

[BindSingleton]
public class PowerLinePreviewTooltip(
    VisualElementLoader visualElementLoader,
    ITooltipRegistrar tooltipRegistrar,
    ILoc loc
) : ILoadableSingleton
{
    const string CrossClass = "cross-red";

    readonly Phrase distancePhrase = Phrase.New("Zipline.Distance").FormatDistance<int>().FormatDistance<int>();

#nullable disable
    VisualElement tooltipRoot;
    Label distanceLabel;
    VisualElement distanceWarning;
    VisualElement distanceIcon;
    VisualElement warnings;
    VisualElement tooManyConnectionsWarning;
#nullable enable

    public void Load()
    {
        tooltipRoot = visualElementLoader.LoadVisualElement("Game/ZiplineConnectionTooltip");
        tooltipRoot.pickingMode = PickingMode.Ignore;

        tooltipRoot.Q("InclinationWrapper").ToggleDisplayStyle(false);
        tooltipRoot.Q("DistrictsWarning").ToggleDisplayStyle(false);
        tooltipRoot.Q("BlockedWarning").ToggleDisplayStyle(false);

        distanceLabel = tooltipRoot.Q<Label>("Distance");
        distanceWarning = tooltipRoot.Q("DistanceWarning");
        distanceIcon = tooltipRoot.Q("DistanceIcon");
        warnings = tooltipRoot.Q("WarningsWrapper");
        tooManyConnectionsWarning = tooltipRoot.Q("TooManyConnectionsWarning");
    }

    public void Show(PowerLineConnectionCheck check)
    {
        distanceLabel.text = loc.T(distancePhrase, Mathf.CeilToInt(check.Distance), Mathf.CeilToInt(check.MaxDistance));
        distanceWarning.ToggleDisplayStyle(!check.DistanceOk);
        distanceIcon.EnableInClassList(CrossClass, !check.DistanceOk);

        var showNoSlot = check.DistanceOk && !check.HasSlot;
        warnings.ToggleDisplayStyle(showNoSlot);
        tooManyConnectionsWarning.ToggleDisplayStyle(showNoSlot);

        tooltipRegistrar.ShowPriority(tooltipRoot);
    }

    public void Hide()
    {
        tooltipRegistrar.HidePriority();
    }
}
