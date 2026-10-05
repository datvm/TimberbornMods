namespace ConveyorBelt.Components.Visuals;

[AddTemplateModule2(typeof(BeltMergerSpec))]
[AddTemplateModule2(typeof(BeltSplitterSpec), AlsoBindTransient = false)]
[AddTemplateModule2(typeof(SmartSplitterSpec), AlsoBindTransient = false)]
public class JunctionVisual(BeltVisualService visuals)
    : BaseComponent, IAwakableComponent, IFinishedStateListener, IUpdatableComponent, IDeletableEntity
{
    const float Pad = 0.001f;
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

    readonly BeltGoodsView goods = new(visuals);
    readonly GameObject?[] runnerCubes = new GameObject?[6];

    BlockObject block = null!;
    BeltMerger? merger;
    BeltSplitter? splitter;
    float runnerShown;
    bool finished;
    Transform? finishedModel;
    Func<float, Vector3> place = null!;

    RunnerRoute[] Routes => merger is not null ? MergerRoutes : SplitterRoutes;

    bool Running => merger is not null ? merger.Running : splitter is not null && splitter.Running;

    SimBelt? Lane => merger is not null ? merger.Sim.Belt : splitter?.Sim.Belt;

    public void Awake()
    {
        place = Pose;
        block = GetComponent<BlockObject>();
        merger = this.GetComponentOrNull<BeltMerger>();
        if (merger is null)
        {
            splitter = GetComponent<BeltSplitter>();
        }

        goods.Bind(this.GetComponentOrNull<EntityMaterials>());
    }

    public void OnEnterFinishedState()
    {
        finished = true;
        EnsureRoot();
    }

    public void OnExitFinishedState()
    {
        finished = false;
        goods.Hide();
    }

    public void DeleteEntity() => goods.DestroyMaterials();

    public void Update()
    {
        var preview = block.IsPreview;
        if ((!preview && !finished) || !visuals.Animation)
        {
            HideRunners();
            goods.Hide();
            return;
        }

        EnsureRunners();
        MoveRunners(preview || Running);
        if (preview || !finished || !visuals.Ready)
        {
            goods.Hide();
            return;
        }

        EnsureRoot();
        if (!goods.HasRoot)
        {
            return;
        }

        goods.ShowRoot();

        var lane = Lane;
        if (lane is null)
        {
            goods.Hide();
            return;
        }

        var trails = lane.Trails;
        if (trails.Count == 0)
        {
            goods.ShowSettled(lane.Items, place);
            return;
        }

        goods.ShowTrails(trails, visuals.Progress, place);
    }

    void EnsureRoot()
    {
        if (goods.HasRoot)
        {
            return;
        }

        var model = FinishedModel();
        if (!model)
        {
            return;
        }

        goods.Attach(model, active: true);
    }

    Vector3 Pose(float along)
    {
        along = Mathf.Clamp01(along);
        var high = 1f - Pad;
        var low = Pad;
        var entry = new Vector3(0.5f, 0f, high);
        var center = new Vector3(0.5f, 0f, 0.5f);
        var exit = new Vector3(0.5f, 0f, low);
        if (along <= 0.5f)
        {
            return Vector3.Lerp(entry, center, along * 2f);
        }

        return Vector3.Lerp(center, exit, (along - 0.5f) * 2f);
    }

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
        var model = FinishedModel();
        if (!model || !visuals.RunnerWood)
        {
            return;
        }

        if (runnerCubes[0] && runnerCubes[0]!.transform.parent == model)
        {
            return;
        }

        for (var i = 0; i < runnerCubes.Length; i++)
        {
            if (runnerCubes[i])
            {
                Object.Destroy(runnerCubes[i]);
            }

            runnerCubes[i] = visuals.CreateRunner(model);
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

    static float RunnerY => BeltGoodsView.BoxY + Cube * 0.5f + 0.001f;
}
