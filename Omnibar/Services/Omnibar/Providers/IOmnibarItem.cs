namespace Omnibar.Services.Omnibar.Providers;

public interface IOmnibarItem
{
    string Title { get; }
    IOmnibarDescriptor? Description { get; }

    bool SetIcon(Image image);

    void Execute();
}

public interface IInplaceExecutionOmnibarItem : IOmnibarItem
{

    void Execute(OmnibarBox box);

}

public interface IOmnibarItemWithTodoList : IOmnibarItem
{
    public bool CanAddToTodoList { get; }
    void AddToTodoList();
}