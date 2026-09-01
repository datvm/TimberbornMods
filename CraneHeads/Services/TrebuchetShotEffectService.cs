namespace CraneHeads.Services;

[BindSingleton]
public class TrebuchetShotEffectService(
    TrebuchetTrajectoryService trajectory,
    OptimizedPrefabInstantiator prefabs,
    IInstantiator instantiator,
    ISpecService specs,
    ISoundSystem sounds,
    IDayNightCycle clock
) : ILoadableSingleton, IUnloadableSingleton, IUpdatableSingleton
{
    const float Duration = 1f;
    const float MinSpin = 540f;
    const float MaxSpin = 1260f;
    const string BoomSound = "Environment.Fireworks.Burst";
    const string GoodStackModelPath = "Environment/GoodStack/GoodStackModel.blueprint";

    readonly List<Shot> shots = [];
    GameObject root = null!;
    Blueprint boxTemplate = null!;
    GoodStackModelSpec boxSpec = null!;

    public void Load()
    {
        root = instantiator.InstantiateEmpty("TrebuchetShotEffects");
        boxTemplate = specs.GetBlueprint(GoodStackModelPath);
        boxSpec = boxTemplate.GetSpec<GoodStackModelSpec>();
    }

    public void Unload()
    {
        foreach (var shot in shots)
        {
            DestroyBox(shot);
        }

        shots.Clear();
        if (root)
        {
            Object.Destroy(root);
        }
    }

    public bool TryStart(CraneHeadTrebuchetLauncher launcher)
    {
        if (!launcher || IsFlying(launcher))
        {
            return false;
        }

        var trebuchet = launcher.GetComponent<CraneHeadTrebuchet>();
        if (!trebuchet || launcher.Target is not { } dest)
        {
            return false;
        }

        var box = CreateBox();
        box.transform.position = trajectory.WorldPoint(trebuchet.Origin, dest, trebuchet.PeakDelta, 0f);
        sounds.PlaySound2D(root, BoomSound, 5);
        shots.Add(new(
            launcher,
            dest,
            trebuchet.Origin,
            trebuchet.PeakDelta,
            box,
            RandomSpin(),
            clock.FluidSecondsPassedToday));
        return true;
    }

    public void UpdateSingleton()
    {
        for (var i = shots.Count - 1; i >= 0; i--)
        {
            var shot = shots[i];
            var elapsed = clock.FluidSecondsPassedToday - shot.Started;
            if (elapsed < 0f)
            {
                elapsed = 0f;
            }

            var t = Mathf.Clamp01(elapsed / Duration);
            if (shot.Box)
            {
                shot.Box.transform.position = trajectory.WorldPoint(shot.Origin, shot.Dest, shot.PeakDelta, t);
                shot.Box.transform.rotation = Quaternion.Euler(shot.Spin * elapsed);
            }

            if (t < 1f)
            {
                continue;
            }

            Complete(shot);
            shots.RemoveAt(i);
        }
    }

    public bool IsFlying(CraneHeadTrebuchetLauncher launcher)
    {
        foreach (var shot in shots)
        {
            if (shot.Launcher == launcher)
            {
                return true;
            }
        }

        return false;
    }

    void Complete(Shot shot)
    {
        DestroyBox(shot);
        if (shot.Launcher)
        {
            shot.Launcher.FinishShot();
        }
    }

    GameObject CreateBox()
    {
        var holder = instantiator.InstantiateEmpty("TrebuchetShotBox", root.transform);
        var model = prefabs.InstantiateInactive(boxTemplate, holder.transform);
        model.FindChild(boxSpec.LogObjectName).SetActive(false);
        model.FindChild(boxSpec.BarrelObjectName).SetActive(false);
        model.FindChild(boxSpec.BagObjectName).SetActive(false);
        var box = model.FindChild(boxSpec.BoxObjectName);
        box.SetActive(true);
        box.transform.localPosition = Vector3.zero;
        foreach (var collider in model.GetComponentsInChildren<Collider>(true))
        {
            Object.Destroy(collider);
        }

        model.SetActive(true);
        return holder;
    }

    static Vector3 RandomSpin()
        => new(
            RandomAxisSpin(),
            RandomAxisSpin(),
            RandomAxisSpin());

    static float RandomAxisSpin()
        => Random.Range(MinSpin, MaxSpin) * (Random.value < 0.5f ? -1f : 1f);

    static void DestroyBox(Shot shot)
    {
        if (shot.Box)
        {
            Object.Destroy(shot.Box);
        }
    }

    readonly record struct Shot(
        CraneHeadTrebuchetLauncher Launcher,
        Vector3Int Dest,
        Vector3Int Origin,
        int PeakDelta,
        GameObject Box,
        Vector3 Spin,
        float Started
    );
}
