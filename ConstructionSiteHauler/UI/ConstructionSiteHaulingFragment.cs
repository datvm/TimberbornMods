namespace ConstructionSiteHauler.UI;

[BindSingleton]
public class ConstructionSiteHaulingFragment : IUnloadableSingleton
{
#nullable disable
    internal static ConstructionSiteHaulingFragment Instance;
    ConstructionSiteFragment frag;
    Toggle chkDisableHauling;
#nullable enable

    readonly ILoc t;

    public ConstructionSiteHaulingFragment(ILoc t)
    {
        Instance = this;
        this.t = t;
    }

    public void Unload()
    {
        Instance = null;
    }

    internal void InitializeFragment(ConstructionSiteFragment frag)
    {
        this.frag = frag;

        var panel = frag._root.AddChild().SetMargin(top: 10);
        var lastChild = frag._root.Q("ConstructionSiteInventoryFragment");
        panel.InsertSelfAfter(lastChild);

        chkDisableHauling = panel.AddGamePanelToggle(
            t.T("LV.CSH.DisableBuilderHaul"),
            onValueChanged: OnDisabledChanged);
    }

    internal void ShowFragment()
    {
        var comp = GetSettings();
        if (!comp)
        {
            return;
        }

        chkDisableHauling.SetValueWithoutNotify(comp!.DisableBuilderHauling);
    }

    void OnDisabledChanged(bool disabled)
    {
        var comp = GetSettings();
        if (!comp)
        {
            return;
        }

        comp!.DisableBuilderHauling = disabled;
    }

    ConstructionSiteBuilderHaulingSettings? GetSettings()
    {
        if (!frag._constructionSite)
        {
            return null;
        }

        var comp = frag._constructionSite.GetComponent<ConstructionSiteBuilderHaulingSettings>();
        return comp ? comp : null;
    }
}
