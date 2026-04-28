namespace TailsAndBannersModMaker.UI;

[BindMenuSingleton]
public class MenuButtonAdder(
    MainMenuPanel mainMenuPanel,
    ILoc t,
    IContainer container
) : ILoadableSingleton
{

    public void Load()
    {
        var btnContinue = mainMenuPanel._root.Q("ContinueButton");
        var btn = btnContinue.AddMenuButton(t.T("LV.TBMM.OpenDialog"), onClick: OpenDialog, stretched: true);
        btn.InsertSelfBefore(btnContinue);
    }

    void OpenDialog() => container.GetInstance<CreateModDialog>().Show();

}
