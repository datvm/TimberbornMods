namespace ModdableTimberbornDemo.Features.EnterableBuff;

public class DemoEnterableBuffFragment : BaseEntityPanelFragment<DemoEnterableBuffComponent>
{
#nullable disable
    Toggle chkEnable;
#nullable enable

    protected override void InitializePanel()
    {
        chkEnable = panel.AddToggle("Enable buff", onValueChanged: OnBuffEnableChanged);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);

        if (!component) { return; }
        chkEnable.SetValueWithoutNotify(component.Active);
    }

    void OnBuffEnableChanged(bool v)
    {
        if (!component) { return; }
        component.Toggle(v);
    }
}
