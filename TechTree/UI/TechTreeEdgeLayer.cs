namespace TechTree.UI;

/// <summary>
/// Draws orthogonal gold prerequisite arrows under tech nodes (Painter2D).
/// </summary>
public class TechTreeEdgeLayer : VisualElement
{
    static readonly Color LineColor = new(0.85f, 0.72f, 0.25f, 1f);
    const float LineWidth = 3f;
    const float ArrowLength = 10f;
    const float ArrowWidth = 8f;

    readonly List<(Vector2 From, Vector2 To)> edges = [];

    public TechTreeEdgeLayer()
    {
        pickingMode = PickingMode.Ignore;
        generateVisualContent += OnGenerateVisualContent;
        style.position = Position.Absolute;
        style.left = 0;
        style.top = 0;
        style.right = 0;
        style.bottom = 0;
    }

    public void SetEdges(IEnumerable<(Vector2 From, Vector2 To)> newEdges)
    {
        edges.Clear();
        edges.AddRange(newEdges);
        MarkDirtyRepaint();
    }

    void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        if (edges.Count == 0)
        {
            return;
        }

        var painter = ctx.painter2D;
        painter.strokeColor = LineColor;
        painter.fillColor = LineColor;
        painter.lineWidth = LineWidth;
        painter.lineCap = LineCap.Round;
        painter.lineJoin = LineJoin.Round;

        foreach (var (from, to) in edges)
        {
            DrawOrthogonalEdge(painter, from, to);
        }
    }

    static void DrawOrthogonalEdge(Painter2D painter, Vector2 from, Vector2 to)
    {
        // Approach the target slightly so the arrowhead sits on the card edge.
        var tip = to;
        var dirIntoTarget = to.x >= from.x ? Vector2.right : Vector2.left;
        var lineEnd = tip - dirIntoTarget * ArrowLength;

        painter.BeginPath();
        painter.MoveTo(from);

        if (Mathf.Abs(from.y - to.y) < 0.5f)
        {
            // Straight horizontal.
            painter.LineTo(lineEnd);
        }
        else
        {
            // Orthogonal: out → mid column → align Y → into target.
            float midX = (from.x + to.x) * 0.5f;
            painter.LineTo(new Vector2(midX, from.y));
            painter.LineTo(new Vector2(midX, to.y));
            painter.LineTo(lineEnd);
        }

        painter.Stroke();
        DrawArrowHead(painter, tip, dirIntoTarget);
    }

    static void DrawArrowHead(Painter2D painter, Vector2 tip, Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }
        direction.Normalize();

        var back = tip - direction * ArrowLength;
        var perp = new Vector2(-direction.y, direction.x) * (ArrowWidth * 0.5f);

        painter.BeginPath();
        painter.MoveTo(tip);
        painter.LineTo(back + perp);
        painter.LineTo(back - perp);
        painter.ClosePath();
        painter.Fill();
    }

}
