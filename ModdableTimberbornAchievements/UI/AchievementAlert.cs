
namespace ModdableTimberbornAchievements.UI;

public class AchievementAlert(
    AlertPanelRowFactory alertPanelRowFactory,
    EventBus eb,
    ModdableAchievementSpecService specs,
    ILoc t,
    GameAchievementDialogShower diagShower
) : IAlertFragmentWithOrder
{
    public int Order { get; } = 10;

#nullable disable
    VisualElement list;
    VisualElement container;
    ClosableAlertElement header;
#nullable enable

    public void InitializeAlertFragment(VisualElement root)
    {
        container = root;

        header = container.AddClosableAlert(
            alertPanelRowFactory,
            "AchievementUnlocked",
            text: t.T("LV.MTA.UnlockNotifHeader"),
            buttonAction: diagShower.ShowDialog,
            closeButtonAction: OnCloseButtonClicked,
            name: "AchievementUnlockHeader");

        list = container.AddChild<NineSliceVisualElement>(classes: [UiCssClasses.FragmentBgPrefix + UiCssClasses.Green]);
        SetVisible(false);

        eb.Register(this);        
    }

    [OnEvent]
    public void OnAchievementUnlocked(ModdableAchievementUnlockedEvent e)
    {
        if (e.AchievementIds.Length == 0) { return; }

        foreach (var id in e.AchievementIds)
        {
            var spec = specs.AchievementsByIds[id];

            list.AddChild(() => new AchievementElement(spec, t, true, false));
        }

        SetVisible(true);
    }

    void OnCloseButtonClicked() => SetVisible(false);

    void SetVisible(bool visible)
    {
        container.SetDisplay(visible);
        header.Visible = visible;

        if (!visible)
        {
            list.Clear();
        }
    }

    public void UpdateAlertFragment() { }
}
