namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneComponent))]
public class CraneTowerHead : BaseComponent, IAwakableComponent, IDeletableEntity
{
    CraneComponent crane = null!;

    public CraneTower Tower => crane.Tower;
    public CraneHeadComponent? Head { get; private set; }
    public bool HasHead => Head;

    public void Awake() => crane = GetComponent<CraneComponent>();

    public void DeleteEntity() => ClearHead();

    internal void SetHead(CraneHeadComponent head)
    {
        if (Head is not null)
        {
            throw new InvalidOperationException("This crane already has a head.");
        }

        Head = head;
        head.Crane = crane;
    }

    internal void ClearHead()
    {
        if (Head is not { } previous)
        {
            return;
        }

        Head = null;
        previous.Crane = null;
    }
}
