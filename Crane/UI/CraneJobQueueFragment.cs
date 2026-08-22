namespace Crane.UI;

[BindFragment]
public class CraneJobQueueFragment(
    IContainer container,
    EntitySelectionService selectionService,
    RollingHighlighter highlighter,
    ILoc t
) : BaseEntityPanelFragment<CraneComponent>
{
    readonly List<CraneJobQueueItem> cache = [];
    VisualElement list = null!;
    Label empty = null!;

    protected override void InitializePanel()
    {
        list = panel.AddChild();
        empty = panel.AddGameLabel(t.T("LV.Cr.NoJobs"));
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null || !component.IsFinished)
        {
            panel.Visible = false;
        }
    }

    public override void UpdateFragment()
    {
        base.UpdateFragment();
        if (component is null || !component.IsFinished)
        {
            return;
        }

        var jobs = component.Tower.Jobs;
        empty.SetDisplay(jobs.Count == 0);

        for (var i = 0; i < jobs.Count; i++)
        {
            var item = i < cache.Count ? cache[i] : AddItem();
            item.Bind(jobs[i], i + 1);
        }

        for (var i = jobs.Count; i < cache.Count; i++)
        {
            cache[i].Unbind();
        }
    }

    public override void ClearFragment()
    {
        highlighter.UnhighlightAllPrimary();
        foreach (var item in cache)
        {
            item.Unbind();
        }

        base.ClearFragment();
    }

    CraneJobQueueItem AddItem()
    {
        var item = list.AddChild(container.GetInstance<CraneJobQueueItem>).Init();
        item.Clicked += OnItemClicked;
        item.Hovered += OnItemHovered;
        cache.Add(item);
        return item;
    }

    void OnItemClicked(object sender, EventArgs e)
    {
        if (sender is CraneJobQueueItem { Job: BaseComponent subject } && subject)
        {
            selectionService.SelectAndFocusOn(subject);
        }
    }

    void OnItemHovered(object sender, bool hovered)
    {
        if (hovered && sender is CraneJobQueueItem { Job: BaseComponent subject } && subject)
        {
            highlighter.HighlightPrimary(subject, Color.yellow);
        }
        else
        {
            highlighter.UnhighlightAllPrimary();
        }
    }

}
