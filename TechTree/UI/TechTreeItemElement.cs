namespace TechTree.UI;

[BindTransient]
public class TechTreeItemElement : VisualElement
{
    public const float ItemWidth = 140f;
    public const float ItemHeight = 160f;
    public const float GapX = 72f;
    public const float GapY = 36f;

    const float IconSize = 72f;
    const float CornerRadius = 8f;

    static readonly Color CardBackground = new(0.08f, 0.10f, 0.08f, 0.95f);
    static readonly Color CardBorder = new(0.45f, 0.40f, 0.28f, 1f);
    static readonly Color NameColor = new(0.92f, 0.90f, 0.82f, 1f);
    static readonly Color CostColor = new(0.85f, 0.72f, 0.25f, 1f);

#nullable disable
    public TechTreeGraphNode Node { get; private set; }
#nullable enable

    public void SetItem(TechTreeGraphNode node)
    {
        Node = node;
        Clear();
        BuildUI();
        ApplyPosition();
    }

    /// <summary>Pixel center of this card on the category canvas.</summary>
    public Vector2 CanvasCenter => new(
        Node.X * (ItemWidth + GapX) + ItemWidth * 0.5f,
        Node.Y * (ItemHeight + GapY) + ItemHeight * 0.5f);

    /// <summary>Right-edge midpoint (outgoing prereq edge).</summary>
    public Vector2 CanvasRightAnchor => new(
        Node.X * (ItemWidth + GapX) + ItemWidth,
        Node.Y * (ItemHeight + GapY) + ItemHeight * 0.5f);

    /// <summary>Left-edge midpoint (incoming prereq edge).</summary>
    public Vector2 CanvasLeftAnchor => new(
        Node.X * (ItemWidth + GapX),
        Node.Y * (ItemHeight + GapY) + ItemHeight * 0.5f);

    void BuildUI()
    {
        var tech = Node.TechItem;
        var catSpec = tech.Category.Spec;

        this.SetSize(ItemWidth, ItemHeight)
            .SetPosition()
            .SetPadding(10, 8)
            .AlignItems(Align.Center)
            .JustifyContent(Justify.Center);

        style.backgroundColor = catSpec.ItemBackgroundColor.a > 0f
            ? catSpec.ItemBackgroundColor
            : CardBackground;

        var border = catSpec.ItemBorderColor.a > 0f && catSpec.ItemBorderColor != Color.black
            ? catSpec.ItemBorderColor
            : CardBorder;
        this.SetBorder(border, 1.5f);

        style.borderTopLeftRadius = CornerRadius;
        style.borderTopRightRadius = CornerRadius;
        style.borderBottomLeftRadius = CornerRadius;
        style.borderBottomRightRadius = CornerRadius;

        if (tech.Spec.Icon is not null)
        {
            this.AddImage(tech.Spec.Icon)
                .SetSize(IconSize, IconSize)
                .SetMarginBottom(8)
                .SetFlexShrink(0);
        }

        var nameColor = catSpec.ItemTextColor != Color.black
            ? catSpec.ItemTextColor
            : NameColor;

        var nameLabel = this.AddGameLabel(
            tech.Name,
            size: UiBuilder.GameLabelSize.Normal,
            bold: true,
            centered: true);
        nameLabel.style.color = nameColor;
        nameLabel.style.whiteSpace = WhiteSpace.Normal;
        nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        nameLabel.SetMarginBottom(4);

        var costLabel = this.AddGameLabel(
            $"Science {tech.Spec.Cost}",
            color: UiBuilder.GameLabelColor.Yellow,
            centered: true);
        costLabel.style.color = CostColor;
        costLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
    }

    void ApplyPosition()
    {
        style.left = Node.X * (ItemWidth + GapX);
        style.top = Node.Y * (ItemHeight + GapY);
    }

}
