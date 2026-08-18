namespace Crane.UI;

[BindFragment]
public class CraneJobQueueFragment(
    EntityBadgeService badgeService,
    BuilderPriorityToggleGroupFactory priorityFactory,
    EntitySelectionService selectionService,
    ILoc t
) : BaseEntityPanelFragment<CraneComponent>
{
    readonly List<JobQueueItem> cache = [];
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
            Bind(item, jobs[i]);
        }

        for (var i = jobs.Count; i < cache.Count; i++)
        {
            Hide(cache[i]);
        }
    }

    public override void ClearFragment()
    {
        foreach (var item in cache)
        {
            Hide(item);
        }

        base.ClearFragment();
    }

    JobQueueItem AddItem()
    {
        var row = list.AddRow().AlignItems().SetMarginBottom(5);
        var clickable = row.AddRow().AlignItems().SetFlexGrow(1);
        clickable.RegisterCallback<ClickEvent>(OnItemClicked);

        var icon = clickable.AddImage().SetSize(32).SetMarginRight().SetFlexShrink(0);
        var name = clickable.AddLabel().SetFlexGrow(1);

        var priority = priorityFactory.Create(row, "LV.Cr.JobPriority");
        row[row.childCount - 1].Q<Label>("Label").SetDisplay(false);

        var item = new JobQueueItem(row, clickable, icon, name, priority);
        cache.Add(item);
        return item;
    }

    void Bind(JobQueueItem item, ICraneJob job)
    {
        var subject = (BaseComponent)job;
        if (item.Current != subject)
        {
            item.Current = subject;
            item.Icon.sprite = badgeService.GetEntityAvatar(subject);
            item.Priority.Enable(job);
        }

        var name = subject.GetName(t);
        item.Name.text = job.IsAvailable ? name : name.Color(TimberbornTextColor.Red);
        item.Priority.UpdateGroup();
        item.Row.SetDisplay(true);
    }

    void Hide(JobQueueItem item)
    {
        item.Current = null;
        item.Priority.Disable();
        item.Row.SetDisplay(false);
    }

    void OnItemClicked(ClickEvent evt)
    {
        if (evt.currentTarget is not VisualElement target)
        {
            return;
        }

        foreach (var item in cache)
        {
            if (item.Clickable == target && item.Current)
            {
                selectionService.SelectAndFocusOn(item.Current);
                return;
            }
        }
    }

    record JobQueueItem(
        VisualElement Row,
        VisualElement Clickable,
        Image Icon,
        Label Name,
        PriorityToggleGroup Priority
    )
    {
        public BaseComponent? Current { get; set; }
    }

}
