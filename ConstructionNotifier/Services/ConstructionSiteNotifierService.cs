namespace ConstructionNotifier.Services;

public class ConstructionSiteNotifierService(
    DialogService diag,
    ILoc t,
    EntityBadgeService badgeService,
    EntitySelectionService selectionService,
    NotificationBus notfBus,
    GameUISoundController sounds
)
{

    public async void Notify(ConstructionSiteNotifier constructionSiteNotifier, bool nonblocking)
    {
        var name = badgeService.GetEntityName(constructionSiteNotifier);
        var msg = t.T("LV.CSN.NotifyMsg", name);

        sounds.PlayWellbeingHighscoreSound();
        if (nonblocking)
        {
            notfBus.Post(msg, constructionSiteNotifier);
        }
        else
        {
            var select = await diag.ConfirmAsync(msg,
                okText: t.T("LV.CSN.SelectFocus"),
                cancelText: t.T("Core.OK"));
            if (!select) { return; }

            selectionService.SelectAndFocusOn(constructionSiteNotifier);
        }
    }

}
