namespace ThePostOffice.Components;

public class PostOfficeLodgeVisuals(PostOfficeBoxVisuals visuals) : BaseComponent, IAwakableComponent, IInitializablePreview, IInitializableEntity, IDeletableEntity, IFinishedStateListener
{
    bool lodge;
    BuildingModel model = null!;
    BlockObject block = null!;
    DistrictBuilding? district;
    PostOfficeBoxPile boxes = null!;
    bool ready;
    bool bannerApplied;
    readonly List<Material> bannerMaterials = [];

    public void Awake()
    {
        lodge = !GetComponent<PostOfficeInventory>();
        model = GetComponent<BuildingModel>();
        block = GetComponent<BlockObject>();
        district = GetComponent<DistrictBuilding>();
        boxes = new();
    }

    public void InitializePreview()
    {
        if (!lodge)
        {
            return;
        }

        ApplyBanner();
    }

    public void InitializeEntity()
    {
        if (!lodge)
        {
            return;
        }

        visuals.LodgeFillChanged += Refresh;
        var link = district;
        if (link is not null && link)
        {
            link.ReassignedInstantDistrict += OnDistrict;
        }

        ApplyBanner();
    }

    public void DeleteEntity()
    {
        visuals.LodgeFillChanged -= Refresh;
        var link = district;
        if (link is not null && link)
        {
            link.ReassignedInstantDistrict -= OnDistrict;
        }
        boxes.Clear();
        foreach (var material in bannerMaterials)
        {
            if (material)
            {
                Object.Destroy(material);
            }
        }

        bannerMaterials.Clear();
    }

    public void OnEnterFinishedState()
    {
        ready = true;
        Refresh();
    }

    public void OnExitFinishedState() => boxes.Clear();

    void Refresh()
    {
        if (!lodge)
        {
            return;
        }

        var finished = model.FinishedModel;
        if (!finished)
        {
            return;
        }

        ApplyBanner();
        if (!Contributing())
        {
            boxes.Clear();
            return;
        }

        visuals.Show(boxes, finished.transform, visuals.LodgeShares(), PostOfficeBoxPlan.LodgeSlots, PostOfficeBoxPlan.LodgeLayers);
    }

    void OnDistrict(object sender, EventArgs e) => Refresh();

    bool Contributing()
    {
        var link = district;
        return ready && block.IsFinished && link is not null && link && link.InstantDistrict;
    }

    void ApplyBanner()
    {
        if (bannerApplied)
        {
            return;
        }

        var finished = model.FinishedModel;
        if (!finished)
        {
            return;
        }

        var renderer = finished.GetComponentInChildren<MeshRenderer>(true);
        if (!renderer)
        {
            return;
        }

        bannerApplied = true;
        visuals.ApplyBanner(renderer, block.FlipMode.IsFlipped, bannerMaterials);
    }
}
