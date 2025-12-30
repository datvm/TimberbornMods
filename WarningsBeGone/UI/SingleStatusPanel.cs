namespace WarningsBeGone.UI;

public enum StatusHidingChangeType
{
    Self,
    Template,
    Global
}

public class SingleStatusPanel : VisualElement
{
    readonly Toggle chkSelf, chkTemplate, chkGlobal;

    public event Action<bool, StatusHidingChangeType> OnHidingRequested = null!;

    public SingleStatusPanel(string name, string status, ILoc t)
    {
        this.SetMarginBottom();

        this.AddGameLabel(status.Bold());
        chkSelf = this.AddToggle(t.T("LV.WBG.HideThis"), onValueChanged: v => OnCheckChanged(v, StatusHidingChangeType.Self));
        chkTemplate = this.AddToggle(t.T("LV.WBG.HideType", name), onValueChanged: v => OnCheckChanged(v, StatusHidingChangeType.Template));
        chkGlobal = this.AddToggle(t.T("LV.WBG.HideAll"), onValueChanged: v => OnCheckChanged(v, StatusHidingChangeType.Global));

        SetUI();
    }

    void OnCheckChanged(bool value, StatusHidingChangeType type)
    {
        OnHidingRequested(value, type);
        SetUI();
    }

    public void SetValuesWithoutNotifying(bool self, bool template, bool global)
    {
        chkSelf.SetValueWithoutNotify(self);
        chkTemplate.SetValueWithoutNotify(template);
        chkGlobal.SetValueWithoutNotify(global);

        SetUI();
    }

    void SetUI()
    {
        chkSelf.enabledSelf = !chkTemplate.value && !chkGlobal.value;
        chkTemplate.enabledSelf = !chkGlobal.value;
    }

}
