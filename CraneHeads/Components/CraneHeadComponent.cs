namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadSpec))]
public class CraneHeadComponent(
    CraneHeadStructureService heads
) : BaseComponent, ICranePartComponent, IAwakableComponent, IFinishedStateListener, IDeletableEntity
{
    BlockObject bo = null!;
    CraneHeadSpec spec = null!;
    CraneComponent? crane;

    public Vector3Int Coordinates => bo.Coordinates;
    public bool IsFinished => bo.IsFinished;

    public CraneComponent? Crane
    {
        get => crane;
        internal set
        {
            if (value is not null && crane is not null)
            {
                throw new InvalidOperationException("This crane head is already attached to a crane.");
            }

            crane = value;
            CraneChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CraneChanged;

    public Vector3Int AttachmentCoordinates => bo.TransformCoordinates(spec.AttachmentCoordinates);

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
        spec = GetComponent<CraneHeadSpec>();
    }

    public CraneComponent? GetCrane() => Crane ?? heads.FindCrane(this);

    public void OnEnterFinishedState() => heads.RefreshHead(this);

    public void OnExitFinishedState() => heads.RefreshHead(this);

    public void DeleteEntity() => heads.RefreshHead(this, this);
}
