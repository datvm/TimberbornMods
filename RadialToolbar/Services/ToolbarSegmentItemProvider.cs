namespace RadialToolbar.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolbarSegmentItemProvider(
    MSettings s,
    BottomBarPanel bottomBarPanel,
    BottomBarButtonLookupService lookupService
)
{

    public ToolbarSegmentItem GetRootItem()
    {
        var segmentCount = s.SegmentCount;

        var root = new ToolbarSegmentItem()
        {
            Name = "$ROOT",
            Children = new ToolbarSegmentItem[segmentCount]
        };

        var main = bottomBarPanel._mainElements;

        var rootButtons = CreateItemsFromElements(
            [
                ..main.Q<VisualElement>("LeftSection").Children(),
                ..main.Q<VisualElement>("MiddleSection").Children(),
                ..main.Q<VisualElement>("RightSection").Children(),
            ],
            segmentCount);


        Distribute(rootButtons, root.Children, segmentCount);

        if (s.FlattenSubmenusValue)
        {
            FlattenSubmenus(root);
        }

        return root;
    }

    void FlattenSubmenus(ToolbarSegmentItem item)
    {
        if (!item.HasChildren) { return; }

        var children = item.Children!;
        foreach (var c in children)
        {
            if (c is null)
            {
                // With our distribution logic, all nulls should be at the end, so we can stop at the first null.
                break;
            }

            FlattenSubmenus(c);
        }

        var emptySlotCount = item.Children!.Count(c => c is null);
        for (int i = 0; i < children.Length && emptySlotCount > 0; i++)
        {
            var c = children[i];
            if (c is null) { break; } // Same early break logic as above.

            var nonEmptyChildCount = c.ChildCount;
            if (nonEmptyChildCount == 0) { continue; } // Already non-children items

            var needSpace = nonEmptyChildCount - 1;
            if (needSpace == 0 || needSpace > emptySlotCount) { continue; }

            item.MarkRepresentativeIfNot();

            // Move items up to make space for the children
            for (int j = children.Length - needSpace - 1; j > i; j--)
            {
                var targetIndex = j + needSpace;
                children[targetIndex] = children[j];
            }

            // Move the children up
            for (int j = 0; j < nonEmptyChildCount; j++)
            {
                children[i + j] = c.Children![j];
            }

            emptySlotCount -= needSpace;
            i += needSpace;
        }
    }

    ToolbarSegmentItem[] CreateItemsFromElements(IEnumerable<VisualElement> ves, int segmentCount) => ves
        .Select(el => CreateItemFromElement(el, segmentCount))
        .Where(i => i is not null)!
        .ToArray()!;

    ToolbarSegmentItem? CreateItemFromElement(VisualElement el, int segmentCount)
    {
        if (!lookupService.TryGet(el, out var info)) { return null; }

        return info switch
        {
            BottomBarButtonLookup<ToolGroupButton> grp => CreateItemFromGroup(grp, segmentCount),
            BottomBarButtonLookup<ToolButton> btn => !btn.Reference.ToolEnabled ? null : CreateItemFromToolButton(btn, el),
            _ => null,
        };
    }

    ToolbarSegmentItem? CreateItemFromGroup(BottomBarButtonLookup<ToolGroupButton> grp, int segmentCount)
    {
        var r = grp.Reference;
        if (!r.IsVisible) { return null; }

        var item = new ToolbarSegmentItem()
        {
            Name = grp.Title,
            Sprite = grp.Sprite,
            Children = new ToolbarSegmentItem[segmentCount],
            OriginalElement = r.Root,
        };

        var childButtons = CreateItemsFromElements(r.ToolButtonsElement.Children(), segmentCount);
        Distribute(childButtons, item.Children, segmentCount);

        return item;
    }

    public static ToolbarSegmentItem CreateItemFromToolButton(BottomBarButtonLookup<ToolButton> btn, VisualElement ve) => new()
    {
        ButtonId = btn.Id,
        Name = btn.Title,
        Sprite = btn.Sprite,
        IsLocked = btn.IsLockedFunc?.Invoke() == true,
        Action = btn.Activate,
        OriginalElement = ve
    };

    void Distribute(ReadOnlySpan<ToolbarSegmentItem> allChildren, ToolbarSegmentItem?[] items, int segmentCount)
    {
        if (allChildren.Length == 0) { return; }

        var count = allChildren.Length;
        var minimumPerSegment = count / segmentCount;
        var extra = count % segmentCount;

        var childIndex = 0;
        for (int i = 0; i < segmentCount; i++)
        {
            var countForThisSegment = minimumPerSegment + (i < extra ? 1 : 0);

            // Does nothing when it's 0
            if (countForThisSegment == 1)
            {
                items[i] = allChildren[childIndex++];
            }
            else if (countForThisSegment > 1)
            {
                var segmentRoot = items[i] = new ToolbarSegmentItem()
                {
                    Children = new ToolbarSegmentItem[segmentCount]
                };

                var segmentChildren = allChildren.Slice(childIndex, countForThisSegment);
                Distribute(segmentChildren, segmentRoot.Children!, segmentCount);
                childIndex += countForThisSegment;
            }
        }
    }
}
