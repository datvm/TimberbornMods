namespace RadialToolbar.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolbarNavigator(
    ToolbarSegmentItemProvider itemProvider
)
{

    readonly Stack<ToolbarSegmentItem> currPath = [];
    public IReadOnlyCollection<ToolbarSegmentItem> CurrentPath => currPath;
    public ToolbarSegmentItem CurrentItem => currPath.Peek();

    public int CurrentPathCount => currPath.Count;

    public void NavigateTo(ToolbarSegmentItem item)
    {
        currPath.Push(item);
    }

    public bool Back()
    {
        if (currPath.Count > 1)
        {
            currPath.Pop();
            return true;
        }

        return false;
    }

    public void Reset()
    {
        currPath.Clear();
        currPath.Push(itemProvider.GetRootItem());
    }

}
