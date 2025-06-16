namespace Omnibar.Services.Omnibar.Hotkeys;

public interface IOmnibarItemWithTodoList : IOmnibarItem
{
    public bool CanAddToTodoList { get; }
    void AddToTodoList(bool append);
}