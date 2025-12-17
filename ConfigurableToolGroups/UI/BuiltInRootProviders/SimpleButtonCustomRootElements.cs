namespace ConfigurableToolGroups.UI.BuiltInRootProviders;


public class CursorButtonCustomRootElement(CursorButton button)
    : BuiltInButtonCustomRootElement<CursorButton>(button)
{
    public override IEnumerable<IToolHotkeyDefinition> GetHotkeys() => [];
}


public class ShowOptionsButtonCustomRootElement(ShowOptionsButton button) : BuiltInButtonCustomRootElement<ShowOptionsButton>(button)
{
    public override IEnumerable<IToolHotkeyDefinition> GetHotkeys() => [];
}
