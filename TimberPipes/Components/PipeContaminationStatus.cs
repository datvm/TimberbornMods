namespace TimberPipes.Components;

[AddTemplateModule2(typeof(TransportPipeSpec))]
public class PipeContaminationStatus(ILoc t) : BaseComponent, IAwakableComponent, IInitializableEntity, IFinishedStateListener
{
    const string Sprite = "PipeContamination";

    BuildingPipe pipe = null!;
    StatusToggle status = null!;

    public void Awake()
    {
        pipe = GetComponent<BuildingPipe>();
        status = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            Sprite,
            t.T("LV.TPi.ContaminatedStatus"),
            t.T("LV.TPi.ContaminatedStatusShort"));
    }

    public void InitializeEntity()
    {
        GetComponent<StatusSubject>().RegisterStatus(status);
    }

    public void OnEnterFinishedState() => Refresh();

    public void OnExitFinishedState() => status.Deactivate();

    public void Refresh()
    {
        var show = pipe.IsFinished
            && pipe.Graph is { Contaminated: true } graph
            && graph.IsStatusPipe(pipe);
        status.Toggle(show);
    }
}
