namespace PackagerBuilder.Services;

public class MenuButtonService(
    MainMenuPanel panel,
    PackagerModBuilder builder,
    DialogService diag,
    ILoc t,
    IContainer container
) : ILoadableSingleton
{

    public void Load()
    {
        AddBuildButton();
    }

    void AddBuildButton()
    {
        // Don't add the button if the Packager mod is already enabled
        if (MConfig.HasPackagerMod == true) { return; }

        var resumeBtn = panel._root.Q("ContinueButton");


        var btn = panel._root.AddMenuButton(t.T("LV.Pkg.BuildPackager"), onClick: Build, stretched: true);
        btn.InsertSelfBefore(resumeBtn);
    }

    async void Build()
    {
        var diagOpts = container.GetInstance<PackagerOptionDialog>();
        var options = await diagOpts.ShowAsync();
        if (options is null) { return; }

        builder.Build(options.Value);

        var result = await diag.ConfirmAsync("LV.Pkg.BuiltDone", true, localizedOkText: "LV.Pkg.Restart");
        if (result)
        {
            TimberUiUtils.Restart();
        }
    }
}
