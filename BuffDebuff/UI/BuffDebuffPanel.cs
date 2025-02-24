namespace BuffDebuff.UI;

public class BuffPanel(ITooltipRegistrar tooltip) : BuffDebuffPanel(true, tooltip);
public class DebuffPanel(ITooltipRegistrar tooltip) : BuffDebuffPanel(false, tooltip);

public class BuffDebuffPanel(bool isBuff, ITooltipRegistrar tooltip) : IEntityPanelFragment
{
    BuffPanelFragment fragment = null!;
    BuffableComponent? comp;
    
    public VisualElement InitializeFragment()
    {
        return fragment = new BuffPanelFragment(isBuff, tooltip);
    }
    
    public void ClearFragment()
    {
        comp = null;
        fragment.Visible = false;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetBuffable();
    }

    public void UpdateFragment()
    {
        if (comp is null)
        {
            fragment.Visible = false;
            return;
        }

        var buffs = comp.GetBuffs()
            .Where(q => q.IsBuff == isBuff)
            .ToList();

        fragment.Visible = buffs.Count > 0;
        fragment.Buffs = buffs;
    }

}
