namespace DecorativePlants.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class DecorativePlantsSettingPanel : VisualElement
{
    readonly BlueprintBuilder builder;
    readonly ILoc t;
    readonly Toggle chkAllFactions, chkFree, chkNoOccupation;

    public DecorativePlantsSettingPanel(BlueprintBuilder builder, ILoc t)
    {
        this.builder = builder;
        this.t = t;

        this.AddGameLabel(t.T("LV.DP.SettingDesc"));
        this.AddGameLabel(t.T("LV.DP.RestartNotice")).SetMarginBottom();

        chkAllFactions = CreateOption("LV.DP.AllFactions");
        chkFree = CreateOption("LV.DP.Free");
        chkNoOccupation = CreateOption("LV.DP.NoOccupation");

        chkFree.RegisterValueChangedCallback(v => chkNoOccupation.SetEnabled(v.newValue));
        chkNoOccupation.enabledSelf = false;

        this.AddMenuButton(t.T("LV.DP.Generate"), Build, stretched: true).SetMarginBottom(10);
        this.AddButton(t.T("LV.DP.Clear"), onClick: builder.Clear, stretched: true, style: UiBuilder.GameButtonStyle.Text);
    }

    Toggle CreateOption(string key)
    {
        var grp = this.AddChild().SetMarginBottom(10);
        var chk = grp.AddToggle(t.T(key));
        grp.AddGameLabel(t.T(key + "Desc"));

        return chk;
    }

    void Build()
    {
        builder.Build(new(chkAllFactions.value, chkFree.value, chkNoOccupation.value));
    }


}
