namespace Crane.UI;

[BindTransient]
public class CraneJobQueueItem(
    EntityBadgeService badgeService,
    BuilderPriorityToggleGroupFactory priorityFactory,
    IGoodService goods,
    ILoc t
) : VisualElement
{
    readonly List<IconSpan> materialIcons = [];

#nullable disable
    Label number, title, action, progress;
    Image icon;
    VisualElement materials;
    PriorityToggleGroup priority;
#nullable enable

    public ICraneJob? Job { get; private set; }

    public event EventHandler? Clicked;
    public event EventHandler<bool>? Hovered;

    public CraneJobQueueItem Init()
    {
        this.SetMarginBottom(10);

        var clickable = this.AddRow().AlignItems().SetMarginBottom(5);
        clickable.RegisterCallback<ClickEvent>(_ => Clicked?.Invoke(this, EventArgs.Empty));
        clickable.RegisterCallback<MouseEnterEvent>(_ => Hovered?.Invoke(this, true));
        clickable.RegisterCallback<MouseLeaveEvent>(_ => Hovered?.Invoke(this, false));

        number = clickable.AddLabel().SetMarginRight().SetFlexShrink(0);
        icon = clickable.AddImage().SetSize(32).SetMarginRight().SetFlexShrink(0);
        title = clickable.AddLabel().SetFlexGrow(1).SetMarginRight();
        action = clickable.AddLabel().SetFlexShrink(0);

        var secondRow = this.AddRow().AlignItems().SetMarginBottom(5);
        progress = secondRow.AddLabel().SetFlexShrink(0).SetMarginRight();
        secondRow.AddChild().SetFlexGrow();
        priority = priorityFactory.Create(secondRow, "LV.Cr.JobPriority");
        secondRow[secondRow.childCount - 1].Q<Label>("Label").SetDisplay(false);

        materials = this.AddRow().AlignItems().SetWrap();
        return this;
    }

    public void Bind(ICraneJob job, int index)
    {
        var subject = (BaseComponent)job;
        if (Job != job)
        {
            Job = job;
            icon.sprite = badgeService.GetEntityAvatar(subject);
            priority.Enable(job);
        }

        number.text = index.ToString();
        var displayName = subject.GetName(t);
        title.text = job.IsAvailable ? displayName : displayName.Color(TimberbornTextColor.Red);
        action.text = t.T(job.JobNameLoc);
        progress.text = $"{Mathf.FloorToInt(job.Progress * 100f)}%";
        priority.UpdateGroup();
        BindMaterials(job as IMaterialCraneJob);
        this.SetDisplay(true);
    }

    public void Unbind()
    {
        Job = null;
        priority.Disable();
        this.SetDisplay(false);
    }

    void BindMaterials(IMaterialCraneJob? material)
    {
        if (material is null)
        {
            materials.SetDisplay(false);
            return;
        }

        Dictionary<string, int> remaining = [];
        foreach (var need in material.GetRemainingMaterials())
        {
            remaining[need.GoodId] = need.Amount;
        }

        var i = 0;
        foreach (var total in material.GetTotalMaterials())
        {
            var left = remaining.GetValueOrDefault(total.GoodId);
            var delivered = Math.Max(0, total.Amount - left);
            var text = $"{delivered}/{total.Amount}";
            if (delivered >= total.Amount)
            {
                text = text.Color(TimberbornTextColor.Green);
            }

            while (i >= materialIcons.Count)
            {
                materialIcons.Add(materials.AddIconSpan().SetMarginRight());
            }

            materialIcons[i].SetGood(goods, total.GoodId, text);
            materialIcons[i].SetDisplay(true);
            i++;
        }

        for (var j = i; j < materialIcons.Count; j++)
        {
            materialIcons[j].SetDisplay(false);
        }

        materials.SetDisplay(i > 0);
    }

}
