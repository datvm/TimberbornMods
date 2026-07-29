namespace TechTree.UI;

/// <summary>
/// Draws orthogonal gold prerequisite arrows under tech nodes (Painter2D).
/// Edges always leave a card to the right, then branch vertically in the gutter —
/// never exit downward from a card face.
/// </summary>
public class TechTreeEdgeLayer : VisualElement
{
    static readonly Color LineColor = new(0.85f, 0.72f, 0.25f, 1f);
    const float LineWidth = 3f;
    const float ArrowLength = 10f;
    const float ArrowWidth = 8f;
    /// <summary>Minimum travel to the right before any vertical branch.</summary>
    const float MinRightStub = 16f;

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

    /// <param name="from">Parent right-edge anchor.</param>
    /// <param name="to">Child left-edge anchor.</param>
    static void DrawOrthogonalEdge(Painter2D painter, Vector2 from, Vector2 to)
    {
        // Gutter just to the right of the parent — vertical runs only here, never on the card.
        float gutterX = from.x + Math.Max(MinRightStub, TechTreeItemElement.GapX * 0.5f);

        // Child left is at/after the gutter → enter its left edge (arrow points right).
        // Child is left of the gutter (same/earlier column) → enter its right edge (arrow points left).
        bool enterFromLeft = to.x >= gutterX - 0.5f;

        Vector2 tip;
        Vector2 approachDir;
        Vector2 lineEnd;

        if (enterFromLeft)
        {
            tip = to;
            approachDir = Vector2.right;
            lineEnd = tip - approachDir * ArrowLength;
        }
        else
        {
            tip = new Vector2(to.x + TechTreeItemElement.ItemWidth, to.y);
            approachDir = Vector2.left;
            lineEnd = tip - approachDir * ArrowLength;
        }

        painter.BeginPath();
        painter.MoveTo(from);

        bool aligned = Math.Abs(from.y - to.y) < 0.5f;
        if (aligned && enterFromLeft)
        {
            // Same row, target to the right: straight horizontal.
            painter.LineTo(lineEnd);
        }
        else
        {
            // Always step right first, then vertical branch, then into the target.
            painter.LineTo(new Vector2(gutterX, from.y));
            painter.LineTo(new Vector2(gutterX, to.y));
            painter.LineTo(lineEnd);
        }

        painter.Stroke();
        DrawArrowHead(painter, tip, approachDir);
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
