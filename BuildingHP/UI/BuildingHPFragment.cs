namespace BuildingHP.UI;

public class BuildingHPFragment : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
    ProgressBar pgb;
    Label lblHp;
#nullable enable

    BuildingHPComponent? comp;

    public void ClearFragment()
    {
        panel.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new() { Visible = false };

        pgb = panel.AddProgressBar("HPBar");
        lblHp = pgb.AddProgressLabel("0 / 0", "HPBarLabel");

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<BuildingHPComponent>();
        if (!comp)
        {
            comp = null;
            return;
        }

        panel.Visible = true;
        UpdateFragment();
    }

    public void UpdateFragment()
    {
        if (!comp) { return; }

        pgb.SetProgress(comp.HPPercent,
            label: lblHp, text: $"{comp.HP} / {comp.Durability}");
    }

}
