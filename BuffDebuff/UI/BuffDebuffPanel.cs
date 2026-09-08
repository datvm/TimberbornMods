namespace BuffDebuff.UI;

public class BuffPanel(ITooltipRegistrar tooltip, VisualElementInitializer veInit) : BuffDebuffPanel(true, tooltip, veInit);
public class DebuffPanel(ITooltipRegistrar tooltip, VisualElementInitializer veInit) : BuffDebuffPanel(false, tooltip, veInit);

public class BuffDebuffPanel(bool isBuff, ITooltipRegistrar tooltip, VisualElementInitializer veInit) : IEntityPanelFragment
{
    BuffPanelFragment fragment = null!;
    BuffableComponent? comp;
    
    public VisualElement InitializeFragment()
    {
        return fragment = new BuffPanelFragment(isBuff, tooltip)
            .Initialize(veInit);
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
