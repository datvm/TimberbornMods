namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeCapacitySpec))]
public class PostOfficeCapacity(PostOfficeService service) : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IFinishedStateListener
{
    PostOfficeCapacitySpec spec = null!;
    BlockObject block = null!;
    DistrictBuilding? district;
    bool contributing;

    public int Contribution => spec.Contribution;
    public Vector3Int Coordinates => block.Coordinates;

    public void Awake()
    {
        spec = GetComponent<PostOfficeCapacitySpec>();
        block = GetComponent<BlockObject>();
        district = GetComponent<DistrictBuilding>();
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
}
