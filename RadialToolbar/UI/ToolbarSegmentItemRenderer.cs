namespace RadialToolbar.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolbarSegmentItemRenderer(
    ToolbarSegmentProvider segmentProvider,
    KeyBindingDescriber keyBindingDescriber,
    NamedIconProvider namedIconProvider
) : VisualElement, ILoadableSingleton
{
    const int HotkeyFontSize = 24;
    const float BoxSize = 50;
    const float HotkeyTranslate = ToolbarFrame.CenterRadius * 2 + BoxSize;
    const int ItemSize = 60;

    const float HalfBoxSize = BoxSize * 0.5f;
    const float Sqrt2Rev = 0.7071f; // 1/sqrt(2)    
    static readonly Color BackgroundColor = ToolbarFrame.BorderColor;
    const float ItemDistanceFactor = .6f;

    public void Load()
    {
        this.SetFullscreen();
        pickingMode = PickingMode.Ignore;
    }

    public void Render(ToolbarSegmentItem item)
    {
        Clear();

        if (item.Children is null)
        {
            throw new ArgumentException("ToolbarSegmentItem must have children to be rendered by SegmentItemRenderer.");
        }

        var children = item.Children;
        var count = children.Length;

        var scale = panel.scaledPixelsPerPoint;
        var rect = new Rect(0, 0, Screen.width / scale, Screen.height / scale);
        var center = rect.center;

        for (int i = 0; i < count; i++)
        {
            var s = children[i];
            if (s is null)
            {
                continue;
            }

            var itemLocation = GetItemLocation(rect, center, i);
            RenderHotkey(itemLocation);
            RenderItem(s, itemLocation);
        }

        ClearPickingMode(this);
    }

    void ClearPickingMode(VisualElement el)
    {
        el.pickingMode = PickingMode.Ignore;
        foreach (var c in el.Children())
        {
            ClearPickingMode(c);
        }
    }

    void RenderHotkey(ItemSegmentLocation itemLocation)
    {
        var (d, center, _) = itemLocation;

        var hotkeyText = d.DescribeHotkey(keyBindingDescriber);

        var lbl = this.AddLabel(hotkeyText.Bold().Size(HotkeyFontSize)).SetSize(BoxSize).SetBorderRadius(BoxSize * 0.25f);

        var s = lbl.style;
        s.backgroundColor = BackgroundColor;
        s.color = Color.white;
        s.unityTextAlign = TextAnchor.MiddleCenter;

        s.position = Position.Absolute;
        s.left = center.x;
        s.top = center.y;
    }

    static readonly Translate TranslateHalf = new(Length.Percent(-50f), Length.Percent(-50f));
    void RenderItem(ToolbarSegmentItem item, ItemSegmentLocation location)
    {
        var el = CreateItemElement(item);

        var s = el.style;
        s.position = Position.Absolute;
        s.left = location.ItemBoxCenter.x;
        s.top = location.ItemBoxCenter.y;
        s.translate = TranslateHalf;
        Add(el);
    }

    VisualElement CreateItemElement(ToolbarSegmentItem item)
    {
        if (item.Name is not null)
        {
            var col = new VisualElement();
            var original = item.OriginalElement;
            var sprite = item.Sprite ?? namedIconProvider.GetOrLoadGameIcon("Question", "question-mark");

            var hasContent = false;
            switch (original?.name)
            {
                case "ToolGroup":
                    col.AddChild(() => CreateToolButton(original, sprite));
                    hasContent = true;
                    break;
                case "ToolButton":
                    col.AddChild(() => CreateToolButton(original, sprite));
                    hasContent = true;
                    break;
            }

            if (hasContent)
            {
                var lbl = col.AddLabel(item.Name).SetMaxWidth(ItemSize);
                lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
            else
            {
                col.AddIconSpan(sprite, item.Name, size: ItemSize);
            }

            return col;
        }
        else // Currently it's a group
        {
            if (!item.HasChildren)
            {
                throw new InvalidOperationException("ToolbarSegmentItem with null Name must have children.");
            }

            return CreateGroup(item);
        }
    }

    VisualElement CreateGroup(ToolbarSegmentItem item)
    {
        const int rowSize = 3;

        var ve = new VisualElement().JustifyContent(Justify.Center);
        var currRowSize = rowSize;
        VisualElement currRow = null!;

        foreach (var c in item.RepresentativeChildren!)
        {
            if (c is null) { continue; }

            var childEl = CreateItemElement(c).SetMarginRight(5);

            if (currRowSize >= rowSize)
            {
                currRowSize = 0;
                currRow = ve.AddRow().SetMarginBottom(5);
            }

            currRow.Add(childEl);
            currRowSize++;
        }

        return ve;
    }

    VisualElement CreateToolButton(VisualElement original, Sprite sprite) => new StandaloneToolButton(namedIconProvider, original, sprite);

    ItemSegmentLocation GetItemLocation(Rect rect, Vector2 center, int segIndex)
    {
        var direction = segmentProvider.GetSegment(segIndex).Direction;
        var (deltaX, deltaY) = direction.GetDirectionDeltas();

        var factor = (deltaX == 0 || deltaY == 0) ? 1f : Sqrt2Rev;

        var radius = HotkeyTranslate * factor;
        var hkX = center.x + deltaX * radius - HalfBoxSize;
        var hkY = center.y + deltaY * radius - HalfBoxSize;

        var distanceToXEdge = deltaX switch
        {
            > 0 => rect.xMax - center.x,
            < 0 => center.x - rect.xMin,
            _ => float.PositiveInfinity
        };
        var distanceToYEdge = deltaY switch
        {
            > 0 => rect.yMax - center.y,
            < 0 => center.y - rect.yMin,
            _ => float.PositiveInfinity
        };

        var distanceToEdge = Math.Min(distanceToXEdge, distanceToYEdge);

        var itemX = center.x + deltaX * distanceToEdge * ItemDistanceFactor;
        var itemY = center.y + deltaY * distanceToEdge * ItemDistanceFactor;

        return new(direction, new(hkX, hkY), new(itemX, itemY));
    }

    readonly record struct ItemSegmentLocation(Direction Direction, Vector2 HotkeyBox, Vector2 ItemBoxCenter);
}
