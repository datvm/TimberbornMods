using TimberUi.CommonProviders;

namespace TimberUiDemo.UI;

public class ColorfulFragments(EntityPanelFragmentBackground background) : IEntityPanelFragment
{

    EntityPanelFragmentElement panel = null!;

    public void ClearFragment()
    {
        panel.Visible = false;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = background,
        };

        panel.AddGameLabel($"{background} colorful fragment");

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        panel.Visible = true;
    }

    public void UpdateFragment() { }
}

public class GreenColorfulFragment() : ColorfulFragments(EntityPanelFragmentBackground.Green);
public class BlueColorfulFragment() : ColorfulFragments(EntityPanelFragmentBackground.Blue);
public class PurpleStripedColorfulFragment() : ColorfulFragments(EntityPanelFragmentBackground.PurpleStriped);
public class PalePurpleColorfulFragment() : ColorfulFragments(EntityPanelFragmentBackground.PalePurple);
public class RedStripedColorfulFragment() : ColorfulFragments(EntityPanelFragmentBackground.RedStriped);
public class FrameColorfulFragment() : ColorfulFragments(EntityPanelFragmentBackground.Frame);

public class ColorfulFragmentsProvider(
    GreenColorfulFragment green,
    BlueColorfulFragment blue,
    PurpleStripedColorfulFragment purpleStriped,
    PalePurpleColorfulFragment palePurple,
    RedStripedColorfulFragment redStriped,
    FrameColorfulFragment frame
) : EntityPanelFragmentProvider([green, blue, purpleStriped, palePurple, redStriped, frame]);
