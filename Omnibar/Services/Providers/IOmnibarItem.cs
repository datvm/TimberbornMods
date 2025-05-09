namespace Omnibar.Services.Providers;

public interface IOmnibarItem
{
    string Title { get; }
    IOmnibarDescriptor? Description { get; }

    bool SetIcon(Image image);

    void Execute();
}