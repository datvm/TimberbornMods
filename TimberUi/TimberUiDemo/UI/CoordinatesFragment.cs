namespace TimberUiDemo.UI;

public class CoordinatesFragment : IEntityPanelFragment
{
    EntityPanelFragmentElement panel = null!;
    Label lbl = null!;

    BlockObject? curr;
    

    public VisualElement InitializeFragment()
    {
        panel = new EntityPanelFragmentElement()
        {
            Background = EntityPanelFragmentBackground.Green
        };

        panel.AddLabelGame("<b>Coordinates</b>");

        var container = panel.AddChild()
            .SetAsRow();

        container.AddLabelGame("(X, Y, Z): ");
        lbl = container.AddLabelGame("N/A");

        return panel;
    }

    public void ClearFragment()
    {
        panel.Visible = false;
    }

    public void ShowFragment(BaseComponent entity)
    {
        curr = entity.GetComponentFast<BlockObject>();
        panel.Visible = curr is not null;

        UpdateFragment();
    }

    public void UpdateFragment()
    {
        if (curr is null) { return; }

        var coords = curr.Coordinates;
        lbl.text = $"({coords.x}, {coords.y}, {coords.z})";
    }
}
