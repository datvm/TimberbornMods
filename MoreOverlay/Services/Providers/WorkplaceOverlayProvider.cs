namespace MoreOverlay.Services.Providers;

[MultiBind(typeof(IMoreOverlayProvider))]
public class WorkplaceOverlayProvider(NamedIconProvider namedIconProvider) : ComponentOverlayProviderBase<Workplace, WorkplaceOverlayInstance>
{
    protected override WorkplaceOverlayInstance CreateInstance(MoreOverlayComponent overlayComp, Workplace comp)
        => new(overlayComp, comp, namedIconProvider);
}

public class WorkplaceOverlayInstance(MoreOverlayComponent overlayComp, Workplace comp, NamedIconProvider namedIconProvider) : ComponentOverlayInstanceBase<Workplace>(overlayComp, comp)
{

    readonly WorkplaceWorkerType? workerType = comp.GetComponent<WorkplaceWorkerType>();
    readonly WorkplacePriority? priorityComp = comp.GetComponent<WorkplacePriority>();

#nullable disable
    IconSpan worker;
    Image priority;
#nullable enable

    public override void Initialize(VisualElement container)
    {
        base.Initialize(container);

        worker = el.AddIconSpan().SetImageSize(IconSize).SetMarginRight(5);
        priority = el.AddImage().SetSize(IconSize);

        Component.DesiredWorkersChanged += (_, _) => UpdateData();
        Component.RelationsChanged += (_, _) => UpdateData();
        workerType?.WorkerTypeChanged += (_, _) => UpdateData();
        priorityComp?.PriorityChanged += (_, _) => UpdateData();
    }

    public override void UpdateData()
    {
        worker.SetImage((workerType && workerType!.WorkerType == "Bot")
            ? namedIconProvider.GetOrLoadGameIcon("ico-work-bot", "ico-work-bot")
            : namedIconProvider.GetOrLoadGameIcon("ico-work-beaver", "ico-work-beaver"),
            (int?) null
        );
        worker.SetPostfixText($"{Component.NumberOfAssignedWorkers}, {Component.DesiredWorkers}/{Component.MaxWorkers}");
        priority.sprite = namedIconProvider.GetPriority(priorityComp ? priorityComp!.Priority : Priority.Normal);
    }

}
