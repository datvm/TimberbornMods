namespace TImprove4Achievements.UI;

public class AchievementHelperDialogShower(
    IOptionsBox optionsBox,
    ILoc t
) : ILoadableSingleton
{

    readonly GameOptionsBox optionsBox = (GameOptionsBox)optionsBox;

    public void Load()
    {
        var settingsBtn = optionsBox._root.Q("SettingsButton");

        var btn = settingsBtn.AddMenuButton(t.T("LV.T4A.AchievementsHelper"), onClick: ShowDialog);
        btn.InsertSelfBefore(settingsBtn);
    }

    void ShowDialog()
    {

    }

}
