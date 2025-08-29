namespace TimberUi.CommonUi;

public class EntityPanelFragmentElement : NineSliceVisualElement
{

    public EntityPanelFragmentBackground Background
    {
        get;
        set
        {
            classList.Remove(GetBackgroundClass(field));
            field = value;
            classList.Add(GetBackgroundClass(value));
        }
    }

    public bool Visible
    {
        get => this.IsDisplayed();
        set => this.ToggleDisplayStyle(value);
    }

    public EntityPanelFragmentElement()
    {
        classList.Add(UiCssClasses.FragmentClass);
        Background = EntityPanelFragmentBackground.Green;
        Visible = false;
    }

    public static string GetBackgroundClass(EntityPanelFragmentBackground background)
    {
        return UiCssClasses.FragmentBgPrefix + background switch
        {
            EntityPanelFragmentBackground.Green => UiCssClasses.Green,
            EntityPanelFragmentBackground.Blue => UiCssClasses.Blue,
            EntityPanelFragmentBackground.PurpleStriped => UiCssClasses.PurpleStriped,
            EntityPanelFragmentBackground.PalePurple => UiCssClasses.PalePurple,
            EntityPanelFragmentBackground.RedStriped => UiCssClasses.RedStriped,
            EntityPanelFragmentBackground.Frame => UiCssClasses.Frame,
            _ => throw new ArgumentOutOfRangeException(nameof(background), background, null)
        };
    }

}

public enum EntityPanelFragmentBackground
{
    Green,
    Blue,
    PurpleStriped,
    PalePurple,
    RedStriped,
    Frame
}
