namespace CraneHeads.UI;

[BindSingleton]
public class TrebuchetPreviewTooltip(
    VisualElementLoader visualElementLoader,
    ITooltipRegistrar tooltipRegistrar,
    ILoc t
) : ILoadableSingleton
{
    const string CrossClass = "cross-red";

#nullable disable
    VisualElement tooltipRoot;
    Label distanceLabel;
    VisualElement distanceWarning;
    VisualElement distanceIcon;
    VisualElement warnings;
    VisualElement blockedWarning;
    Label reason;
#nullable enable

    public void Load()
    {
        tooltipRoot = visualElementLoader.LoadVisualElement("Game/ZiplineConnectionTooltip");
        tooltipRoot.pickingMode = PickingMode.Ignore;
        tooltipRoot.Q("InclinationWrapper").ToggleDisplayStyle(false);
        tooltipRoot.Q("DistrictsWarning").ToggleDisplayStyle(false);
        tooltipRoot.Q("TooManyConnectionsWarning").ToggleDisplayStyle(false);

        distanceLabel = tooltipRoot.Q<Label>("Distance");
        distanceWarning = tooltipRoot.Q("DistanceWarning");
        distanceIcon = tooltipRoot.Q("DistanceIcon");
        warnings = tooltipRoot.Q("WarningsWrapper");
        blockedWarning = tooltipRoot.Q("BlockedWarning");
        blockedWarning.ToggleDisplayStyle(false);
        reason = tooltipRoot.AddGameLabel().SetMargin(top: 5);
    }

    public void Show(TrebuchetShotCheck check)
    {
        distanceLabel.text = t.T("LV.CrH.PreviewRange", check.Distance, check.MaxRange);
        var rangeBad = check.Status is TrebuchetShotStatus.OutOfRange or TrebuchetShotStatus.SameTile;
        distanceWarning.ToggleDisplayStyle(rangeBad);
        distanceIcon.EnableInClassList(CrossClass, rangeBad);

        warnings.ToggleDisplayStyle(!check.IsValid);
        reason.text = check.Status switch
        {
            TrebuchetShotStatus.Valid => "",
            TrebuchetShotStatus.SameTile => t.T("LV.CrH.TrebuchetSameTile"),
            TrebuchetShotStatus.OutOfRange => t.T("LV.CrH.TrebuchetOutOfRange"),
            TrebuchetShotStatus.TooHigh => t.T("LV.CrH.TrebuchetTooHigh"),
            TrebuchetShotStatus.InvalidLanding => t.T("LV.CrH.TrebuchetBadLanding"),
            TrebuchetShotStatus.Blocked => t.T("LV.CrH.TrebuchetBlocked"),
            _ => t.T("LV.CrH.TrebuchetBlocked"),
        };
        reason.ToggleDisplayStyle(!check.IsValid);
        tooltipRegistrar.ShowPriority(tooltipRoot);
    }

    public void Hide() => tooltipRegistrar.HidePriority();
}
