namespace RadialToolbar.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolbarFrame(ToolbarSegmentProvider segmentProvider) : VisualElement, ILoadableSingleton
{
    public const float CenterRadius = 50f;
    const float LineWidth = 2f;

    public static readonly Color BorderColor = new(.61f, .53f, .36f);
    static readonly Color BackgroundColor = new(0f, 0f, 0f, 0.7f);
    static readonly Color HighlightColor = BorderColor with { a = .7f };
    static readonly Color CenterColor = Color.black;

    public int? HighlightedSegment { get; set; }

    public void Load()
    {
        this.SetFullscreen();
        generateVisualContent += OnGenerateVisualContent;
    }

    public void Repaint()
    {
        MarkDirtyRepaint();
    }

    void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var rect = contentRect;
        if (rect.width <= 0 || rect.height <= 0)
        {
            return;
        }

        var painter = ctx.painter2D;
        var segments = segmentProvider.GetSegments(rect);

        // Fill the wedges first.
        foreach (var segment in segments)
        {
            painter.fillColor = HighlightedSegment == segment.Index
                ? HighlightColor
                : BackgroundColor;

            FillSegment(painter, rect, segment);
        }

        // Draw all boundary rays.
        painter.strokeColor = BorderColor;
        painter.lineWidth = LineWidth;

        foreach (var segment in segments)
        {
            DrawRay(painter, segment.StartBoundary);
        }

        // Overwrite the center with a circle.
        var center = rect.center;

        painter.fillColor = CenterColor;
        painter.BeginPath();
        painter.Arc(center, CenterRadius, 0f, 360f);
        painter.Fill();

        painter.BeginPath();
        painter.Arc(center, CenterRadius, 0f, 360f);
        painter.Stroke();
    }

    static void FillSegment(
        Painter2D painter,
        Rect rect,
        ToolbarSegment segment)
    {
        painter.BeginPath();

        painter.MoveTo(segment.Center);
        painter.LineTo(segment.StartBoundary.End);

        AddClockwiseRectBoundary(
            painter,
            segment.StartBoundary.Side,
            segment.EndBoundary.Side,
            rect
        );

        painter.LineTo(segment.EndBoundary.End);
        painter.ClosePath();
        painter.Fill();
    }

    static void DrawRay(
        Painter2D painter,
        ToolbarRay ray)
    {
        painter.BeginPath();
        painter.MoveTo(ray.Start);
        painter.LineTo(ray.End);
        painter.Stroke();
    }

    static void AddClockwiseRectBoundary(
        Painter2D painter,
        Direction fromSide,
        Direction toSide,
        Rect rect)
    {
        var side = fromSide;

        while (side != toSide)
        {
            painter.LineTo(GetCornerAfterSide(side, rect));
            side = NextSideClockwise(side);
        }
    }

    static Vector2 GetCornerAfterSide(Direction side, Rect rect) => side switch
    {
        Direction.Up => new Vector2(rect.xMax, rect.yMin),
        Direction.Right => new Vector2(rect.xMax, rect.yMax),
        Direction.Down => new Vector2(rect.xMin, rect.yMax),
        Direction.Left => new Vector2(rect.xMin, rect.yMin),
        _ => default
    };

    static Direction NextSideClockwise(Direction side) => side switch
    {
        Direction.Up => Direction.Right,
        Direction.Right => Direction.Down,
        Direction.Down => Direction.Left,
        Direction.Left => Direction.Up,
        _ => Direction.Up
    };
}