namespace ConstructionNotifier.Services;

public class ConstructionSiteNotifierService(
    DialogService diag,
    ILoc t,
    EntityBadgeService badgeService,
    EntitySelectionService selectionService
)
{

    public async void Notify(ConstructionSiteNotifier constructionSiteNotifier)
    {
        var name = badgeService.GetEntityName(constructionSiteNotifier);

        var select = await diag.ConfirmAsync(t.T("LV.CSN.NotifyMsg", name),
            okText: t.T("LV.CSN.SelectFocus"),
            cancelText: t.T("Core.OK"));
        if (!select) { return; }

        selectionService.SelectAndFocusOn(constructionSiteNotifier);
    }

}
