namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltCarrierSpec))]
public class BeltVisual(IBlockService blocks, MSettings settings)
    : BaseComponent, IAwakableComponent, IInitializablePreview, IPostPlacementChangeListener,
        IFinishedStateListener, IPreviewSelectionListener, IUpdatableComponent
{
    const int Pool = BeltRates.Capacity;

    readonly List<GameObject> goods = [];
    readonly List<Transform> rollers = [];

    BlockObject block = null!;
    BeltCarrier carrier = null!;
    GameObject? straight;
    GameObject? building;
    bool finished;
    bool linksBuildings;
    BeltShape shape;

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        carrier = GetComponent<BeltCarrier>();
        var spec = GetComponent<BeltCarrierSpec>();
        shape = spec.Shape;
        linksBuildings = BeltShapeInfo.LinksBuildings(spec.Shape);

        var finishedModel = Transform.Find("#Finished");
        if (!finishedModel)
        {
            DisableComponent();
            return;
        }

        straight = Child(finishedModel, "#Straight");
        building = Child(finishedModel, "#Building");
        var template = Named(finishedModel, "GoodTemplate");
        if (template)
        {
            template.SetActive(false);
            for (var i = 0; i < Pool; i++)
            {
                var copy = Object.Instantiate(template, finishedModel);
                copy.name = "Good";
                copy.SetActive(false);
                goods.Add(copy);
            }
        }

        foreach (var child in finishedModel.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Roller")
            {
                rollers.Add(child);
            }
        }
    }

    public void InitializePreview() => RefreshMesh();

    public void OnPostPlacementChanged() => RefreshMesh();

    public void OnPreviewSelect() => RefreshMesh();

    public void OnPreviewUnselect() { }

    public void OnEnterFinishedState()
    {
        finished = true;
        RefreshMesh();
    }

    public void OnExitFinishedState()
    {
        finished = false;
        HideGoods();
    }

    public void Update()
    {
        if (!finished)
        {
            return;
        }

        var animate = settings.Animation.Value && carrier.Running;
        if (animate)
        {
            var spin = carrier.ItemsPerHour * 36f * Time.deltaTime;
            foreach (var roller in rollers)
            {
                if (roller.gameObject.activeInHierarchy)
                {
                    roller.Rotate(spin, 0f, 0f, Space.Self);
                }
            }
        }

        ShowGoods(settings.Animation.Value);
    }

    void RefreshMesh()
    {
        if (!straight || !building)
        {
            return;
        }

        var facesBuilding = linksBuildings && (Faces(Direction3D.Up) || Faces(Direction3D.Down));
        straight.SetActive(!facesBuilding);
        building.SetActive(facesBuilding);
    }

    bool Faces(Direction3D local)
    {
        var world = block.TransformDirection(local);
        var cell = block.Coordinates + world.ToOffset();
        foreach (var obj in blocks.GetObjectsAt(cell))
        {
            if (!obj || obj == block)
            {
                continue;
            }

            if (obj.GetComponent<BeltCarrierSpec>() is not null || obj.GetComponent<BeltTeleporterSpec>() is not null)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    void ShowGoods(bool visible)
    {
        var items = carrier.Items;
        for (var i = 0; i < goods.Count; i++)
        {
            var show = visible && i < items.Count;
            goods[i].SetActive(show);
            if (show)
            {
                goods[i].transform.localPosition = GoodPosition(items[i].Position);
            }
        }
    }

    void HideGoods()
    {
        foreach (var good in goods)
        {
            good.SetActive(false);
        }
    }

    Vector3 GoodPosition(float position)
    {
        const float height = 0.13f;
        var grid = shape switch
        {
            BeltShape.RiserUp => new Vector3(0.5f, 0.5f, Mathf.Lerp(0.18f, 0.82f, position)),
            BeltShape.RiserDown => new Vector3(0.5f, 0.5f, Mathf.Lerp(0.82f, 0.18f, position)),
            BeltShape.Corner => Corner(position, height),
            _ => new Vector3(0.5f, Mathf.Lerp(0.82f, 0.18f, position), height),
        };
        return CoordinateSystem.GridToWorld(grid);
    }

    Vector3 Corner(float t, float height)
    {
        var flipped = block.FlipMode.IsFlipped;
        var start = new Vector3(0.5f, 0.82f, height);
        var end = new Vector3(flipped ? 0.18f : 0.82f, 0.5f, height);
        var bend = new Vector3(flipped ? 0.28f : 0.72f, 0.72f, height);
        return Vector3.Lerp(Vector3.Lerp(start, bend, t), Vector3.Lerp(bend, end, t), t);
    }

    static GameObject? Child(Transform root, string name)
    {
        var found = root.Find(name);
        return found ? found.gameObject : null;
    }

    static GameObject? Named(Transform root, string name)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
