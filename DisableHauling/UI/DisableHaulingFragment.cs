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

    Toggle chkDisableHauling = null!;

    public void Load()
    {
        instance = this;
        var other = fragment!._root.Q("Toggle");

        chkDisableHauling = AddCheckBox("LV.DH.ToggleHaulingLabel", OnDisablingChanged);

        Toggle AddCheckBox(string textKey, Action<bool> onValueChanged)
        {
            var toggle = other.parent.AddChild<Toggle>(classes: ["game-toggle", "entity-panel__text", "entity-panel__toggle"])
                .SetMargin(top: 10);
            toggle.text = t.T(textKey);
            toggle.RegisterValueChangedCallback(e => onValueChanged(e.newValue));
            toggle.InsertSelfAfter(other);
            return toggle;
        }
    }

    internal void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponent<DisableHaulingComponent>();
        if (!comp)
        {
            ClearFragment();
            return;
        }

        chkDisableHauling.SetValueWithoutNotify(comp.DisableHauling);
    }

    internal void ClearFragment()
    {
        comp = null;
    }

    void OnDisablingChanged(bool e)
    {
        if (!comp) { return; }
        comp!.DisableHauling = e;
    }

    public void Unload()
    {
        fragment = null;
        instance = null;
    }
}
