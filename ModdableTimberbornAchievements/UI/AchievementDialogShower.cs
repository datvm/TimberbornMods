namespace ModdableTimberbornAchievements.UI;

public abstract class BaseAchievementDialogShower(
    ILoc t,
    PanelStack panelStack,
    IContainer container
) : ILoadableSingleton
{
    protected abstract VisualElement Root { get; }

    public void Load()
    {
        var settingsBtn = Root.Q("SettingsButton");

        var btn = settingsBtn.parent.AddMenuButton(t.T("LV.MTA.Achievements"), onClick: ShowDialog, stretched: true);
        btn.InsertSelfBefore(settingsBtn);
    }

    void ShowDialog()
    {
        var diag = container.GetInstance<AchievementDialog>();
        diag.Show(initializer: null, panelStack);
    }
}

public class MainMenuDialogShower(MainMenuPanel mainMenuPanel, ILoc t, PanelStack panelStack, IContainer container) : BaseAchievementDialogShower(t, panelStack, container)
{
    protected override VisualElement Root => mainMenuPanel._root;
}

public class GameAchievementDialogShower(IOptionsBox optionsBox, ILoc t, PanelStack panelStack, IContainer container) : BaseAchievementDialogShower(t, panelStack, container)
{

    readonly GameOptionsBox optionsBox = (GameOptionsBox)optionsBox;
    protected override VisualElement Root => optionsBox._root;

}