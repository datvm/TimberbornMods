namespace ModdableTimberbornAchievements.UI;

public abstract class BaseAchievementDialogShower(
    ILoc t,
    PanelStack panelStack,
    IContainer container,
    InputService inputService
) : ILoadableSingleton, IInputProcessor
{
    protected abstract VisualElement Root { get; }

    public void Load()
    {
        var settingsBtn = Root.Q("SettingsButton");

        var btn = settingsBtn.parent.AddMenuButton(t.T("LV.MTA.Achievements"), onClick: ShowDialog, stretched: true);
        btn.InsertSelfBefore(settingsBtn);

        inputService.AddInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (inputService.IsKeyDown("ShowAchievementDialog"))
        {
            ShowDialog();
            return true;
        }

        return false;
    }

    public void ShowDialog()
    {
        var diag = container.GetInstance<AchievementDialog>();
        diag.Show(initializer: null, panelStack);
    }
}

public class MainMenuDialogShower(MainMenuPanel mainMenuPanel, ILoc t, PanelStack panelStack, IContainer container, InputService inputService) : BaseAchievementDialogShower(t, panelStack, container, inputService)
{
    protected override VisualElement Root => mainMenuPanel._root;
}

public class GameAchievementDialogShower(IOptionsBox optionsBox, ILoc t, PanelStack panelStack, IContainer container, InputService inputService) : BaseAchievementDialogShower(t, panelStack, container, inputService)
{

    readonly GameOptionsBox optionsBox = (GameOptionsBox)optionsBox;
    protected override VisualElement Root => optionsBox._root;

}