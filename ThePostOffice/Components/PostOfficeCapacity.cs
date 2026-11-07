namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeCapacitySpec))]
public class PostOfficeCapacity(PostOfficeService service) : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IFinishedStateListener, IEntityDescriber
{
    PostOfficeCapacitySpec spec = null!;
    BlockObject block = null!;
    DistrictBuilding? district;
    bool contributing;
    bool office;

    public int Contribution => spec.Contribution;
    public Vector3Int Coordinates => block.Coordinates;
    public bool IsFinished => block.IsFinished;
    public bool IsOffice => office;

    public void Awake()
    {
        spec = GetComponent<PostOfficeCapacitySpec>();
        block = GetComponent<BlockObject>();
        district = GetComponent<DistrictBuilding>();
        office = GetComponent<PostOfficeInventory>();
    }

    public void InitializeEntity()
    {
        var link = district;
        if (link is not null && link)
        {
            link.ReassignedInstantDistrict += OnDistrict;
        }

        Refresh();
    }

    public void DeleteEntity()
    {
        var link = district;
        if (link is not null && link)
        {
            link.ReassignedInstantDistrict -= OnDistrict;
        }

        Release();
    }

    public void OnEnterFinishedState() => Refresh();

    public void OnExitFinishedState() => Release();

    void OnDistrict(object sender, EventArgs e) => Refresh();

    void Refresh()
    {
        if (Connected())
        {
            if (contributing)
            {
                return;
            }

            contributing = true;
            service.AddContribution(this);
            return;
        }

        Release();
    }

    void Release()
    {
        if (!contributing)
        {
            return;
        }

        contributing = false;
        service.RemoveContribution(this);
    }

    bool Connected()
    {
        var body = block;
        var link = district;
        return body && body.IsFinished && link is not null && link && link.InstantDistrict;
    }

    public IEnumerable<EntityDescription> DescribeEntity() => [
        EntityDescription.CreateTextSection(service.t.T("LV.TPO.GlobalSpace", spec.Contribution), 200),
    ];
}
