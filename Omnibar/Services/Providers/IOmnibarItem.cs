namespace Omnibar.Services.Providers;

public interface IOmnibarItem
{
    string Title { get; }
    string? Description { get; }
    
    bool SetIcon(Image image);

    void Execute();
}
