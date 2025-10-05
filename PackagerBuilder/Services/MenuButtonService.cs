namespace PackagerBuilder.Services;

public class MenuButtonService(
    MainMenuPanel panel,
    PackagerModBuilder builder,
    DialogService diag,
    ModRepository modRepository,
    ILoc t
) : ILoadableSingleton
{

    public void Load()
    {
        AddBuildButton();
    }

    void AddBuildButton()
    {
        // Don't add the button if the Packager mod is already enabled
        if (modRepository.EnabledMods.Any(q => q.Manifest.Id == PackagerModBuilder.ModId)) { return; }

        var resumeBtn = panel._root.Q("ContinueButton");


        var btn = panel._root.AddMenuButton(t.T("LV.Pkg.BuildPackager"), onClick: Build, stretched: true);
        btn.InsertSelfBefore(resumeBtn);
    }

    async void Build()
    {
        builder.Build();

        var result = await diag.ConfirmAsync("LV.Pkg.BuiltDone", true, localizedOkText: "LV.Pkg.Restart");
        if (result)
        {
            TimberUiUtils.Restart();
        }
    }
}
