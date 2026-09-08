namespace Omnibar.Services.Omnibar.Hotkeys;

public interface IOmnibarHotkeyProvider
{

    public IOmnibarHotkeyAction? GetAction(IOmnibarItem item);

}
