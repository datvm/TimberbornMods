namespace DisableHauling.UI;

[BindSingleton]
public class DisableHaulingFragment(
    ILoc t,
#pragma warning disable CS9113 // For DI
    IEntityPanel _
#pragma warning restore CS9113 // Parameter is unread.
) : ILoadableSingleton, IUnloadableSingleton
{
    internal static DisableHaulingFragment? instance;

    internal static HaulCandidateFragment? fragment;
    DisableHaulingComponent? comp;

    Toggle chkDisable = null!;

    public void Load()
    {
        instance = this;
        var other = fragment!._root.Q("Toggle");

        chkDisable = other.parent.AddChild<Toggle>(classes: ["game-toggle", "entity-panel__text", "entity-panel__toggle"])
            .SetMargin(top: 10);
        chkDisable.text = t.T("LV.DH.ToggleLabel");
        chkDisable.RegisterValueChangedCallback(OnDisablingChanged);
        chkDisable.InsertSelfAfter(other);
    }

    internal void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponent<DisableHaulingComponent>();
        if (!comp)
        {
            ClearFragment();
            return;
        }

        chkDisable.SetValueWithoutNotify(comp.DisableHauling);
    }

    internal void ClearFragment()
    {
        comp = null;
    }

    void OnDisablingChanged(ChangeEvent<bool> e)
    {
        if (!comp) { return; }
        comp!.DisableHauling = e.newValue;
    }

    public void Unload()
    {
        fragment = null;
        instance = null;
    }
}
