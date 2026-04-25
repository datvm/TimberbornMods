namespace ConfigurableToolGroups.Models;

public record BottomBarButtonLookup(
    string Id,
    VisualElement VisualElement,
    string TitleLoc,
    string Title
)
{

    public Sprite? Sprite { get; init; }

    public Func<bool> IsLockedFunc { get; init; } = False;
    public Action Activate { get; init; } = NoAction;

    static bool False() => false;
    static void NoAction() { }
}

public record BottomBarButtonLookup<T>(
    string Id,
    VisualElement VisualElement,
    string TitleLoc,
    string Title,
    T Reference
) : BottomBarButtonLookup(Id, VisualElement, TitleLoc, Title);