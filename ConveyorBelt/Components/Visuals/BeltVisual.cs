namespace ConveyorBelt.Components.Visuals;

[AddTemplateModule2(typeof(BeltCarrierSpec))]
public class BeltVisual(BeltVisualService visuals)
    : BaseComponent, IAwakableComponent, IInitializablePreview, IPostPlacementChangeListener,
        IFinishedStateListener, IPreviewSelectionListener, IUpdatableComponent, IDeletableEntity
{
    const float Pad = 0.001f;
    const float Deck = 0.11f;
    const float Cube = 0.04f;
    const float BoxGap = 0.015f;
    const float PlankMin = 0.35525f;
    const float PlankMax = 0.64475f;

    readonly BeltGoodsView goods = new(visuals);

    bool ownsMaterials;
    bool materialsReady;
    BlockObject block = null!;
    BeltCarrier carrier = null!;
    GameObject? straight;
    GameObject? building;
    bool finished;
    bool swapsMesh;
    BeltShape shape;
    GameObject? leftRunner;
    GameObject? rightRunner;
    float runnerShown;
    float deckMin = PlankMin;
    float deckMax = PlankMax;
    float deckTop = Deck;
    Transform? finishedModel;
    Func<float, Vector3> place = null!;

    public void Awake()
    {
        place = Pose;
        block = GetComponent<BlockObject>();
        carrier = GetComponent<BeltCarrier>();
        var spec = GetComponent<BeltCarrierSpec>();
        shape = spec.Shape;
        swapsMesh = shape is BeltShape.Straight or BeltShape.Impermeable;

        var model = FinishedModel();
        if (!model)
        {
            DisableComponent();
            return;
        }

        straight = Child(model, "#Straight");
        building = Child(model, "#Building");
        foreach (var child in model.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "GoodTemplate")
            {
                child.gameObject.SetActive(false);
            }
        }

        var entityMaterials = GetComponent<EntityMaterials>();
        ownsMaterials = !entityMaterials;
        goods.Bind(entityMaterials);
        EnsureRunners();
        if (!visuals.Ready)
        {
            return;
        }

        goods.Attach(model, active: false);
        goods.EnsureCount(BeltRates.Capacity);
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
    }

    public void DeleteEntity()
    {
        if (!ownsMaterials)
        {
            return;
        }

        goods.DestroyMaterials();
    }

    public void Update()
    {
        EnsureRunners();
        if (!visuals.Animation || !RunsOnDeck || (!block.IsPreview && !finished))
        {
            HideRunners();
            goods.Hide();
            return;
        }

        if (block.IsPreview)
        {
            MoveRunners(follow: true);
            goods.Hide();
            return;
        }

        MoveRunners(carrier.Running);
        if (goods.Count == 0)
        {
            return;
        }

        UseRendererMaterials();
        ShowGoods();
    }

    bool RunsOnDeck => shape is BeltShape.Straight or BeltShape.Impermeable or BeltShape.Corner;

    Transform? FinishedModel()
    {
        if (finishedModel)
        {
            return finishedModel;
        }

        var found = Transform.Find("#Finished");
        if (!found)
        {
            return null;
        }

        finishedModel = found;
        return finishedModel;
    }

    void EnsureRunners()
    {
        var wood = visuals.RunnerWood;
        var model = FinishedModel();
        if (!model || !wood)
        {
            return;
        }

        if (leftRunner && leftRunner.transform.parent == model)
        {
            return;
        }

        if (leftRunner)
        {
            Object.Destroy(leftRunner);
        }

        if (rightRunner)
        {
            Object.Destroy(rightRunner);
        }

        CreateRunners(model);
    }

    void CreateRunners(Transform parent)
    {
        MeasureDeck(parent);
        leftRunner = visuals.CreateRunner(parent);
        rightRunner = visuals.CreateRunner(parent);
    }

    void MeasureDeck(Transform parent)
    {
        if (shape == BeltShape.Corner)
        {
            return;
        }

        var mesh = DeckMesh(parent);
        if (!mesh)
        {
            return;
        }

        var bounds = mesh.bounds;
        deckMin = bounds.min.x;
        deckMax = bounds.max.x;
        deckTop = bounds.max.y;
    }

    static UnityEngine.Mesh? DeckMesh(Transform parent)
    {
        var deck = DeckRenderer(parent);
        var filter = deck ? deck.GetComponent<MeshFilter>() : null;
        return filter ? filter.sharedMesh : null;
    }

    static MeshRenderer? DeckRenderer(Transform parent)
    {
        MeshRenderer? found = null;
        foreach (var renderer in parent.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer.gameObject.name == "GoodTemplate")
            {
                continue;
            }

            found ??= renderer;
            if (renderer.gameObject.name == "Deck")
            {
                return renderer;
            }
        }

        return found;
    }

    void MoveRunners(bool follow)
    {
        if (!leftRunner || !rightRunner)
        {
            return;
        }

        if (follow)
        {
            runnerShown = visuals.Along(carrier.SpeedIndex);
        }

        PlaceRunner(leftRunner, runnerShown, 1f);
        PlaceRunner(rightRunner, runnerShown, -1f);
    }

    void PlaceRunner(GameObject runner, float along, float side)
    {
        runner.SetActive(true);
        runner.transform.localScale = Vector3.one * Cube;
        runner.transform.localPosition = RunnerPose(along, side);
    }

    // side +1 is the inside of the corner, the shorter path. side -1 goes the long way around,
    // so that cube moves faster and both still arrive together.
    Vector3 RunnerPose(float along, float side)
    {
        var y = deckTop + Cube * 0.5f + 0.001f;
        var lane = side * Lane;
        var high = 1f - Pad;
        along = Mathf.Repeat(along, 1f);
        if (shape != BeltShape.Corner)
        {
            var span = high - Pad;
            return new Vector3(0.5f + lane, y, high - along * span);
        }

        var elbow = 0.5f + lane;
        var leg = high - elbow;
        var dist = along * leg * 2f;
        if (dist <= leg)
        {
            return new Vector3(0.5f + lane, y, high - dist);
        }

        return new Vector3(elbow + (dist - leg), y, elbow);
    }

    float Lane
    {
        get
        {
            var half = Cube * 0.5f;
            var beside = BoxSize * 0.5f + BoxGap + half;
            var room = Mathf.Min(deckMax - 0.5f, 0.5f - deckMin) - half;
            if (beside > room)
            {
                return Mathf.Max(0f, room);
            }

            return beside;
        }
    }

    void HideRunners()
    {
        if (leftRunner)
        {
            leftRunner.SetActive(false);
        }

        if (rightRunner)
        {
            rightRunner.SetActive(false);
        }
    }

    void UseRendererMaterials()
    {
        if (materialsReady)
        {
            return;
        }

        materialsReady = true;
        goods.SyncFromRenderers();
    }

    void ShowGoods()
    {
        goods.ShowRoot();
        var trails = carrier.Sim.Trails;
        if (trails.Count == 0)
        {
            goods.ShowSettled(carrier.Sim.Items, place);
            return;
        }

        goods.ShowTrails(trails, visuals.Progress, place);
    }

    void RefreshMesh()
    {
        if (!straight || !building)
        {
            return;
        }

        var facesBuilding = swapsMesh && (visuals.FacesBuilding(block, Direction3D.Up) || visuals.FacesBuilding(block, Direction3D.Down));
        straight.SetActive(!facesBuilding);
        building.SetActive(facesBuilding);
    }

    float BoxSize => BeltRates.Spacing * BeltGoodsView.BoxFill;

    Vector3 Pose(float along)
    {
        along = Mathf.Clamp01(along);
        var high = 1f - Pad;
        if (shape != BeltShape.Corner)
        {
            return new Vector3(0.5f, 0f, high - along * Span);
        }

        var leg = high - 0.5f;
        var path = along * leg * 2f;
        if (path <= leg)
        {
            return new Vector3(0.5f, 0f, high - path);
        }

        return new Vector3(0.5f + (path - leg), 0f, 0.5f);
    }

    float Span => 1f - Pad * 2f;

    static GameObject? Child(Transform root, string name)
    {
        var found = root.Find(name);
        return found ? found.gameObject : null;
    }
}
