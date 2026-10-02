namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltMergerSpec))]
[AddTemplateModule2(typeof(BeltSplitterSpec), AlsoBindTransient = false)]
[AddTemplateModule2(typeof(SmartSplitterSpec), AlsoBindTransient = false)]
public class JunctionVisual(BeltVisualService visuals)
    : BaseComponent, IAwakableComponent, IFinishedStateListener, IUpdatableComponent, IDeletableEntity
{
    const float Pad = 0.001f;
    const float BoxFill = 0.6f;
    const float BoxY = 0.11f;
    const float Cube = 0.04f;
    const float LaneOffset = 0.095f;
    const float Plus = 0.5f + LaneOffset;
    const float Minus = 0.5f - LaneOffset;
    const float Pair1 = 0.55f;
    const float Pair2 = 0.36f;
    const float Pair3 = 0.12f;

    // Merger paths. Each point is (X, Z) on this tile:
    //   X 0 = left entrance, X 1 = right entrance, X 0.5 = middle (goods use this)
    //   Z 1 = back entrance, Z 0 = exit
    // Minus (0.405) is the left lane. Plus (0.595) is the right lane.
    // A cube walks its points in order, then restarts at the first point.
    // Offset is how far along that path it already is, from 0 to 1. A bigger offset is closer to the exit.
    // Pair1 leads, on the side entrance closer to the exit. Pair2 is the other cube on that entrance.
    // Pair3 is the straight entrance. The same offset is used on both lanes.
    // Row order is fixed: back +X, back -X, left high Z, left low Z, right high Z, right low Z.
    static readonly RunnerRoute[] MergerRoutes =
    [
        new(Pair3, [new(Plus, 1f), new(Plus, 0f)]),
        new(Pair3, [new(Minus, 1f), new(Minus, 0f)]),
        new(Pair2, [new(0f, Plus), new(Minus, Plus), new(Minus, 0f)]),
        new(Pair1, [new(0f, Minus), new(Minus, Minus), new(Minus, 0f)]),
        new(Pair2, [new(1f, Plus), new(Plus, Plus), new(Plus, 0f)]),
        new(Pair1, [new(1f, Minus), new(Plus, Minus), new(Plus, 0f)]),
    ];

    // Splitter paths. Each point is (X, Z) on this tile:
    //   X 0 = left exit, X 1 = right exit, X 0.5 = middle (goods use this)
    //   Z 1 = entrance, Z 0 = straight exit
    // Pair1 leaves on the side exit closer to the straight exit. Pair2 leaves on the far side of that arm.
    // Pair3 goes straight out. The same offset is used on both lanes.
    // Row order is fixed: straight +X, straight -X, left high Z, left low Z, right high Z, right low Z.
    static readonly RunnerRoute[] SplitterRoutes =
    [
        new(Pair3, [new(Plus, 1f), new(Plus, 0f)]),
        new(Pair3, [new(Minus, 1f), new(Minus, 0f)]),
        new(Pair2, [new(Minus, 1f), new(Minus, Plus), new(0f, Plus)]),
        new(Pair1, [new(Minus, 1f), new(Minus, Minus), new(0f, Minus)]),
        new(Pair2, [new(Plus, 1f), new(Plus, Plus), new(1f, Plus)]),
        new(Pair1, [new(Plus, 1f), new(Plus, Minus), new(1f, Minus)]),
    ];

    readonly record struct RunnerRoute(float Offset, Vector2[] Points);

    readonly List<GameObject> goods = [];
    readonly List<Material> materials = [];
    readonly List<Material> iconMaterials = [];
    readonly List<string> shownIds = [];
    readonly GameObject?[] runnerCubes = new GameObject?[6];

    BlockObject block = null!;
    BeltMerger? merger;
    BeltSplitter? splitter;
    EntityMaterials? entityMaterials;
    GameObject? goodsRoot;
    float runnerShown;
    bool finished;

    RunnerRoute[] Routes => merger is not null ? MergerRoutes : SplitterRoutes;

    bool Running => merger is not null ? merger.Running : splitter is not null && splitter.Running;

    IReadOnlyList<JunctionHop> Hops
    {
        get
        {
            if (merger is not null)
            {
                return merger.Hops;
            }

            if (splitter is not null)
            {
                return splitter.Hops;
            }

            return [];
        }
    }

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        merger = this.GetComponentOrNull<BeltMerger>();
        if (merger is null)
        {
            splitter = GetComponent<BeltSplitter>();
        }

        entityMaterials = this.GetComponentOrNull<EntityMaterials>();
    }

    public void OnEnterFinishedState()
    {
        finished = true;
        EnsureRoot();
    }

    public void OnExitFinishedState()
    {
        finished = false;
        HideGoods();
    }

    public void DeleteEntity()
    {
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
        var preview = block.IsPreview;
        if ((!preview && !finished) || !visuals.Animation)
        {
            HideRunners();
            HideGoods();
            return;
        }

        EnsureRunners();
        MoveRunners(preview || Running);
        if (preview || !finished || !visuals.Ready)
        {
            HideGoods();
            return;
        }

        EnsureRoot();
        if (!goodsRoot)
        {
            return;
        }

        goodsRoot.SetActive(true);

        var hops = Hops;
        var progress = visuals.Progress;
        EnsureCount(hops.Count);
        var shown = 0;
        for (var i = 0; i < hops.Count; i++)
        {
            var hop = hops[i];
            if (!hop.Shows(progress))
            {
                continue;
            }

            ShowOne(shown, hop.Id, hop.Direction, hop.Along(progress));
            shown++;
        }

        for (var i = shown; i < goods.Count; i++)
        {
            goods[i].SetActive(false);
            shownIds[i] = "";
        }
    }

    void EnsureRoot()
    {
        if (goodsRoot)
        {
            return;
        }

        var finishedModel = Transform.Find("#Finished");
        if (!finishedModel)
        {
            return;
        }

        goodsRoot = new GameObject("Goods");
        goodsRoot.layer = finishedModel.gameObject.layer;
        goodsRoot.transform.SetParent(finishedModel, false);
    }

    void ShowOne(int i, string id, Direction3D direction, float along)
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

        var size = BeltRates.Spacing * BoxFill;
        var position = Pose(direction, along);
        position.y = BoxY;
        good.SetActive(true);
        good.transform.localScale = Vector3.one * visuals.GoodScale(size);
        good.transform.localPosition = position;
        Adopt(i, boxRenderer, iconRenderer);
        if (shownIds[i] != id)
        {
            Paint(i, id);
        }

        icon.SetActive(shownIds[i] == id);
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

    Vector3 Pose(Direction3D direction, float along) => merger is not null
        ? MergerPose(direction, along)
        : SplitterPose(direction, along);

    static Vector3 MergerPose(Direction3D from, float along)
    {
        along = Mathf.Clamp01(along);
        var high = 1f - Pad;
        var low = Pad;
        var entry = from switch
        {
            Direction3D.Left => new Vector3(low, 0f, 0.5f),
            Direction3D.Right => new Vector3(high, 0f, 0.5f),
            _ => new Vector3(0.5f, 0f, high),
        };
        var center = new Vector3(0.5f, 0f, 0.5f);
        var exit = new Vector3(0.5f, 0f, low);
        if (along <= 0.5f)
        {
            return Vector3.Lerp(entry, center, along * 2f);
        }

        return Vector3.Lerp(center, exit, (along - 0.5f) * 2f);
    }

    static Vector3 SplitterPose(Direction3D to, float along)
    {
        along = Mathf.Clamp01(along);
        var high = 1f - Pad;
        var low = Pad;
        var entry = new Vector3(0.5f, 0f, high);
        var center = new Vector3(0.5f, 0f, 0.5f);
        var exit = to switch
        {
            Direction3D.Left => new Vector3(low, 0f, 0.5f),
            Direction3D.Right => new Vector3(high, 0f, 0.5f),
            _ => new Vector3(0.5f, 0f, low),
        };
        if (along <= 0.5f)
        {
            return Vector3.Lerp(entry, center, along * 2f);
        }

        return Vector3.Lerp(center, exit, (along - 0.5f) * 2f);
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

    void HideGoods()
    {
        if (goodsRoot)
        {
            goodsRoot.SetActive(false);
        }
    }

    void EnsureRunners()
    {
        var finishedModel = Transform.Find("#Finished");
        if (!finishedModel || !visuals.RunnerWood)
        {
            return;
        }

        if (runnerCubes[0] && runnerCubes[0]!.transform.parent == finishedModel)
        {
            return;
        }

        for (var i = 0; i < runnerCubes.Length; i++)
        {
            if (runnerCubes[i])
            {
                Object.Destroy(runnerCubes[i]);
            }

            runnerCubes[i] = visuals.CreateRunner(finishedModel);
        }
    }

    void MoveRunners(bool follow)
    {
        if (!runnerCubes[0])
        {
            return;
        }

        if (follow)
        {
            runnerShown = visuals.JunctionAlong;
        }

        var routes = Routes;
        for (var index = 0; index < routes.Length; index++)
        {
            var cube = runnerCubes[index];
            if (!cube)
            {
                continue;
            }

            var point = Along(routes[index].Points, runnerShown + routes[index].Offset);
            cube.SetActive(true);
            cube.transform.localScale = Vector3.one * Cube;
            cube.transform.localPosition = new Vector3(point.x, RunnerY, point.y);
        }
    }

    static Vector2 Along(Vector2[] points, float progress)
    {
        var count = points.Length;
        if (count <= 1)
        {
            return count == 0 ? Vector2.zero : points[0];
        }

        var total = 0f;
        for (var i = 1; i < count; i++)
        {
            total += Vector2.Distance(points[i - 1], points[i]);
        }

        if (total <= 0f)
        {
            return points[0];
        }

        var dist = Mathf.Repeat(progress, 1f) * total;
        for (var i = 1; i < count; i++)
        {
            var span = Vector2.Distance(points[i - 1], points[i]);
            if (dist <= span || i == count - 1)
            {
                var u = span <= 0f ? 1f : dist / span;
                return Vector2.Lerp(points[i - 1], points[i], Mathf.Clamp01(u));
            }

            dist -= span;
        }

        return points[^1];
    }

    void HideRunners()
    {
        foreach (var cube in runnerCubes)
        {
            if (cube)
            {
                cube.SetActive(false);
            }
        }
    }

    static float RunnerY => BoxY + Cube * 0.5f + 0.001f;
}
