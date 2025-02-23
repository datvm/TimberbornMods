namespace BuffDebuff.UI;

public class BuffPanel() : BuffDebuffPanel(true);
public class DebuffPanel() : BuffDebuffPanel(false);

public class BuffDebuffPanel(bool isBuff) : IEntityPanelFragment
{
    BuffPanelFragment fragment = null!;
    BuffableComponent? comp;
    
    public VisualElement InitializeFragment()
    {
        return fragment = new BuffPanelFragment(isBuff);
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
