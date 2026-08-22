namespace CraneHeads.UI;

[BindSingleton]
public class CraneHeadUIAppender(
    CraneFragment fragment,
    EntityBadgeService badgeService,
    EntitySelectionService selectionService,
    RollingHighlighter highlighter,
    ILoc t
) : ILoadableSingleton
{
    VisualElement row = null!;
    Image icon = null!;
    Label name = null!;
    CraneHeadComponent? head;

    public void Load()
    {
        fragment.AppendInitializePanel(panel =>
        {
            row = panel.AddRow().AlignItems().SetMargin(top: 10);
            row.AddLabel(t.T("LV.CrH.HeadLabel")).SetMarginRight(5);

            row.RegisterCallback<ClickEvent>(_ => OnClicked());
            row.RegisterCallback<MouseEnterEvent>(_ => OnHovered(true));
            row.RegisterCallback<MouseLeaveEvent>(_ => OnHovered(false));

            icon = row.AddImage().SetSize(32).SetMarginRight().SetFlexShrink(0);
            name = row.AddLabel().SetFlexGrow(1);
            row.SetDisplay(false);
        });

        fragment.OnUpdateFragment += OnUpdateFragment;
        fragment.OnClearFragment += OnClearFragment;
    }

    void OnUpdateFragment(object sender, CraneComponent e)
    {
        var next = e.GetComponent<CraneTowerHead>()?.Head;
        if (next is not { } attached || !attached)
        {
            ClearHead();
            return;
        }

        if (head != attached)
        {
            head = attached;
            icon.sprite = badgeService.GetEntityAvatar(attached);
            name.text = attached.GetName(t);
        }

        row.SetDisplay(true);
    }

    void OnClearFragment(object sender, EventArgs e)
    {
        highlighter.UnhighlightAllPrimary();
        ClearHead();
    }

    void ClearHead()
    {
        head = null;
        row.SetDisplay(false);
    }

    void OnClicked()
    {
        if (head)
        {
            selectionService.SelectAndFocusOn(head);
        }
    }

    void OnHovered(bool hovered)
    {
        if (hovered && head)
        {
            highlighter.HighlightPrimary(head, Color.yellow);
        }
        else
        {
            highlighter.UnhighlightAllPrimary();
        }
    }

}
