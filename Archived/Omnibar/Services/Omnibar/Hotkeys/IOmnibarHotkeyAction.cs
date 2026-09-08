
namespace Omnibar.Services.Omnibar.Hotkeys;

public interface IOmnibarHotkeyAction
{

    IReadOnlyList<string> HotkeyPrompts { get; }
    bool ProcessInput(InputModifiers modifiers);

}
