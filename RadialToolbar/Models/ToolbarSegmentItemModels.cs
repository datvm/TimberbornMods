namespace RadialToolbar.Models;

public class ToolbarSegmentItem
{
    public string? ButtonId { get; set; }

    public string? Name { get; set; }
    public Sprite? Sprite { get; set; }
    public bool IsLocked { get; set; }
    public Action? Action { get; set; }
    public VisualElement? OriginalElement { get; set; }

    public ToolbarSegmentItem?[]? Children { get; set; }
    public int ChildCount => Children?.Count(c => c is not null) ?? 0;

    ToolbarSegmentItem?[]? representativeChildren;
    public ToolbarSegmentItem?[]? RepresentativeChildren
    {
        get => representativeChildren ?? Children;
        set => representativeChildren = value;
    }

    public void MarkRepresentativeIfNot()
    {
        if (representativeChildren is not null) { return; }

        if (Children is null) { throw new InvalidOperationException("Cannot make representative item for an item without children."); }
        representativeChildren = [.. Children];
    }

    public bool HasChildren => Children is not null;

}
