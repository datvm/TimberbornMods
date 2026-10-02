namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltCarrierSpec))]
public class BeltVisual(BeltVisualService visuals)
    : BaseComponent, IAwakableComponent, IInitializablePreview, IPostPlacementChangeListener,
        IFinishedStateListener, IPreviewSelectionListener, IUpdatableComponent, IDeletableEntity
{
    const float Pad = 0.001f;
    const float BoxFill = 0.6f;
    const float BoxY = 0.11f;
    const float Deck = 0.11f;
    const float Cube = 0.04f;
    const float BoxGap = 0.015f;
    const float PlankMin = 0.35525f;
    const float PlankMax = 0.64475f;

    readonly List<GameObject> goods = [];
    readonly List<Material> materials = [];
    readonly List<Material> iconMaterials = [];
    readonly List<string> shownIds = [];
    bool ownsMaterials;
    bool materialsReady;
    EntityMaterials? entityMaterials;

    BlockObject block = null!;
    BeltCarrier carrier = null!;
    GameObject? straight;
    GameObject? building;
    bool finished;
    bool swapsMesh;
    BeltShape shape;
    GameObject? goodsRoot;
    GameObject? leftRunner;
    GameObject? rightRunner;
    float runnerShown;
    float deckMin = PlankMin;
    float deckMax = PlankMax;
    float deckTop = Deck;

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        carrier = GetComponent<BeltCarrier>();
        var spec = GetComponent<BeltCarrierSpec>();
        shape = spec.Shape;
        swapsMesh = shape is BeltShape.Straight or BeltShape.Impermeable;

        var finishedModel = Transform.Find("#Finished");
        if (!finishedModel)
        {
            DisableComponent();
            return;
        }

        straight = Child(finishedModel, "#Straight");
        building = Child(finishedModel, "#Building");
        foreach (var child in finishedModel.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "GoodTemplate")
            {
                child.gameObject.SetActive(false);
            }
        }

        entityMaterials = GetComponent<EntityMaterials>();
        ownsMaterials = !entityMaterials;
        EnsureRunners();
        if (!visuals.Ready)
        {
            return;
        }

        goodsRoot = new GameObject("Goods");
        goodsRoot.layer = finishedModel.gameObject.layer;
        goodsRoot.transform.SetParent(finishedModel, false);
        goodsRoot.SetActive(false);
        EnsureCount(BeltRates.Capacity);
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

        foreach (var material in materials)
        {
            Object.Destroy(material);
        }

        foreach (var material in iconMaterials)
        {
            Object.Destroy(material);
        }
    }

    public void Update()
    {
        EnsureRunners();
        if (!visuals.Animation || !RunsOnDeck || (!block.IsPreview && !finished))
        {
            HideRunners();
            HideGoods();
            return;
        }

        if (block.IsPreview)
        {
            MoveRunners(follow: true);
            HideGoods();
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

    void EnsureRunners()
    {
        var wood = visuals.RunnerWood;
        var finishedModel = Transform.Find("#Finished");
        if (!finishedModel || !wood)
        {
            return;
        }

        if (leftRunner && leftRunner.transform.parent == finishedModel)
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

        CreateRunners(finishedModel);
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
        for (var i = 0; i < goods.Count; i++)
        {
            var current = goods[i].GetComponent<MeshRenderer>().sharedMaterial;
            if (current && current != materials[i])
            {
                materials[i] = current;
            }

            var icon = goods[i].transform.Find("Icon").GetComponent<MeshRenderer>().sharedMaterial;
            if (icon && icon != iconMaterials[i])
            {
                iconMaterials[i] = icon;
            }
        }
    }

    void ShowGoods()
    {
        if (goodsRoot)
        {
            goodsRoot.SetActive(true);
        }

        var trails = carrier.Trails;
        if (trails.Count == 0)
        {
            ShowSettled();
            return;
        }

        var progress = visuals.Progress;
        EnsureCount(trails.Count);
        for (var i = 0; i < trails.Count; i++)
        {
            var trail = trails[i];
            if (!trail.Shows(progress))
            {
                goods[i].SetActive(false);
                continue;
            }

            ShowOne(i, trail.Id, trail.Along(progress));
        }

        HideRest(trails.Count);
    }

    void ShowSettled()
    {
        var items = carrier.Items;
        EnsureCount(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            ShowOne(i, item.Id, item.Position);
        }

        HideRest(items.Count);
    }

    void ShowOne(int i, string id, float along)
    {
        var good = goods[i];
        var icon = good.transform.Find("Icon").gameObject;
        var boxRenderer = good.GetComponent<MeshRenderer>();
        var iconRenderer = icon.GetComponent<MeshRenderer>();
        Adopt(i, boxRenderer, iconRenderer);
        if (shownIds[i] != id)
        {
            Paint(i, id);
        }

        Place(good, along);
        Adopt(i, boxRenderer, iconRenderer);
        if (shownIds[i] != id)
        {
            Paint(i, id);
        }

        icon.SetActive(shownIds[i] == id);
    }

    void HideRest(int used)
    {
        for (var i = used; i < goods.Count; i++)
        {
            goods[i].SetActive(false);
            shownIds[i] = "";
        }
    }

    void EnsureCount(int count)
    {
        if (!goodsRoot)
        {
            return;
        }

        while (goods.Count < count)
        {
            var copy = visuals.CreateGood(goodsRoot.transform, entityMaterials);
            goods.Add(copy);
            materials.Add(copy.GetComponent<MeshRenderer>().sharedMaterial);
            iconMaterials.Add(copy.transform.Find("Icon").GetComponent<MeshRenderer>().sharedMaterial);
            shownIds.Add("");
        }
    }

    void Adopt(int i, MeshRenderer boxRenderer, MeshRenderer iconRenderer)
    {
        var boxMaterial = boxRenderer.sharedMaterial;
        var iconMaterial = iconRenderer.sharedMaterial;
        if (boxMaterial && boxMaterial != materials[i])
        {
            materials[i] = boxMaterial;
            shownIds[i] = "";
        }

        if (iconMaterial && iconMaterial != iconMaterials[i])
        {
            iconMaterials[i] = iconMaterial;
            shownIds[i] = "";
        }
    }

    void Paint(int i, string id)
    {
        if (!visuals.ApplyGood(materials[i], iconMaterials[i], id))
        {
            return;
        }

        shownIds[i] = id;
    }

    void Place(GameObject good, float along)
    {
        var size = BoxSize;
        var scale = visuals.GoodScale(size);
        var position = Pose(along);
        position.y = BoxY;
        good.SetActive(true);
        good.transform.localScale = Vector3.one * scale;
        good.transform.localPosition = position;
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

    void HideGoods()
    {
        if (goodsRoot)
        {
            goodsRoot.SetActive(false);
        }
    }

    float Span => 1f - Pad * 2f;

    float BoxSize => BeltRates.Spacing * BoxFill;

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

    static GameObject? Child(Transform root, string name)
    {
        var found = root.Find(name);
        return found ? found.gameObject : null;
    }
}
