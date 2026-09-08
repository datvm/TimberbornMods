namespace QuickBar.Models;

public interface IQuickBarItem
{
    string Title { get; }
    Sprite? Sprite { get; }
    Texture2D? Texture { get; }
    void Activate();

    bool IsStillValid();
}
