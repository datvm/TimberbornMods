namespace Omnibar.Services.Providers;

public interface IOmnibarItem
{
    string Title { get; }

    bool SetDescription(VisualElement container);  
    bool SetIcon(Image image);

    void Execute();
}