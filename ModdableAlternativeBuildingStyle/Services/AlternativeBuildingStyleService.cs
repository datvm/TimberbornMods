namespace ModdableAlternativeBuildingStyle.Services;

[BindSingleton]
public class AlternativeBuildingStyleService(
    ISpecService specService,
    OptimizedPrefabInstantiator prefabs,
    IInstantiator instantiator)
{
    const string AlternativePrefix = "#Alternative-";

    public GameObject LoadStyle(string path, Transform parent)
    {
        var blueprint = specService.GetBlueprint(path);
        return prefabs.InstantiateInactive(blueprint, parent);
    }

    public void Capture(AlternativeBuildingStyleComponent building)
    {
        var state = building.State;
        if (state.Captured)
        {
            return;
        }

        var finishedName = building.GetComponent<BuildingModelSpec>()?.FinishedModelName;
        var finished = building.GameObject.FindChildIfNameNotEmpty(finishedName);
        if (!finished)
        {
            return;
        }

        state.Finished = finished.transform;
        state.BuildingModel = building.GetComponent<BuildingModel>();
        state.Animator = state.Finished.GetComponent<TimbermeshAnimator>()
            ?? state.Finished.GetComponentInChildren<TimbermeshAnimator>(true);
        CollectOriginal(state);
        state.OriginalHolder = new GameObject("AlternativeStyle.Original");
        state.OriginalHolder.transform.SetParent(building.GameObject.transform, false);
        state.OriginalHolder.SetActive(false);
        state.Captured = true;
    }

    public void Restore(AlternativeBuildingStyleComponent building)
    {
        var state = building.State;
        if (!state.Captured || !state.StyleInstance)
        {
            return;
        }

        foreach (var updater in state.AddedUpdaters)
        {
            if (updater)
            {
                Object.DestroyImmediate(updater);
            }
        }

        state.AddedUpdaters.Clear();

        foreach (var grafted in state.Grafted)
        {
            if (grafted)
            {
                Object.DestroyImmediate(grafted);
            }
        }

        state.Grafted.Clear();
        Object.DestroyImmediate(state.StyleInstance);
        state.StyleInstance = null;

        foreach (var home in state.Homes)
        {
            if (!home.Target)
            {
                continue;
            }

            home.Target.SetParent(home.Parent, false);
            home.Target.localPosition = home.LocalPosition;
            home.Target.localRotation = home.LocalRotation;
            home.Target.localScale = home.LocalScale;
        }

        foreach (var renderer in state.AnchorRenderers)
        {
            if (renderer)
            {
                renderer.forceRenderingOff = false;
            }
        }

        RestoreColliders(state);
        RestoreAnimation(state);
        Refresh(building);
    }

    public bool TryApply(AlternativeBuildingStyleComponent building, AlternativeBuildingStyle style)
    {
        var state = building.State;
        if (!state.Captured || string.IsNullOrEmpty(style.BlueprintPath))
        {
            return false;
        }

        GameObject instance;
        try
        {
            instance = LoadStyle(style.BlueprintPath, state.Finished);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to load alternative style '{style.Name}' from '{style.BlueprintPath}' on {building.GameObject.name}.\n{exception}");
            return false;
        }

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        RenameAnchors(instance.transform);
        var updaters = instance.GetComponentsInChildren<IAnimationUpdater>(true).ToList();
        Restore(building);
        RememberAnimation(state);
        ParkOriginalVisuals(state);
        HideAnchorRenderers(state);
        RemoveOriginalColliders(state);
        GraftAnchors(state, instance, updaters);
        var selfDriven = BindAnimation(state, instance, updaters);
        instance.SetActive(true);
        if (selfDriven)
        {
            selfDriven.Enabled = true;
        }

        state.StyleInstance = instance;
        Refresh(building);
        return true;
    }

    static void CollectOriginal(BuildingStyleState state)
    {
        var pending = new Queue<Transform>();
        for (var i = 0; i < state.Finished.childCount; i++)
        {
            pending.Enqueue(state.Finished.GetChild(i));
        }

        while (pending.Count > 0)
        {
            var child = pending.Dequeue();
            RememberHome(state, child);
            if (child.name.StartsWith('#'))
            {
                if (!state.Anchors.ContainsKey(child.name))
                {
                    state.Anchors.Add(child.name, child);
                }

                var renderer = child.GetComponent<Renderer>();
                if (renderer)
                {
                    state.AnchorRenderers.Add(renderer);
                }
            }
            else
            {
                state.Visuals.Add(child);
            }

            for (var i = 0; i < child.childCount; i++)
            {
                pending.Enqueue(child.GetChild(i));
            }
        }
    }

    static void RememberHome(BuildingStyleState state, Transform target)
    {
        state.Homes.Add(new(
            target,
            target.parent,
            target.localPosition,
            target.localRotation,
            target.localScale));
    }

    static void ParkOriginalVisuals(BuildingStyleState state)
    {
        foreach (var anchor in state.Anchors.Values)
        {
            if (anchor)
            {
                anchor.SetParent(state.Finished, true);
            }
        }

        foreach (var visual in state.Visuals)
        {
            if (visual)
            {
                visual.SetParent(state.OriginalHolder.transform, true);
            }
        }
    }

    static void HideAnchorRenderers(BuildingStyleState state)
    {
        foreach (var renderer in state.AnchorRenderers)
        {
            if (renderer)
            {
                renderer.forceRenderingOff = true;
            }
        }
    }

    static void RemoveOriginalColliders(BuildingStyleState state)
    {
        if (state.CollidersRemoved)
        {
            return;
        }

        foreach (var collider in state.Finished.GetComponents<Collider>())
        {
            switch (collider)
            {
                case BoxCollider box:
                    state.Boxes.Add(new(box.center, box.size));
                    break;
                case SphereCollider sphere:
                    state.Spheres.Add(new(sphere.center, sphere.radius));
                    break;
                case CapsuleCollider capsule:
                    state.Capsules.Add(new(capsule.center, capsule.radius, capsule.height, capsule.direction));
                    break;
                default:
                    continue;
            }

            Object.DestroyImmediate(collider);
        }

        state.CollidersRemoved = true;
    }

    static void RestoreColliders(BuildingStyleState state)
    {
        if (!state.CollidersRemoved)
        {
            return;
        }

        var host = state.Finished.gameObject;
        foreach (var box in state.Boxes)
        {
            var collider = host.AddComponent<BoxCollider>();
            collider.center = box.Center;
            collider.size = box.Size;
        }

        foreach (var sphere in state.Spheres)
        {
            var collider = host.AddComponent<SphereCollider>();
            collider.center = sphere.Center;
            collider.radius = sphere.Radius;
        }

        foreach (var capsule in state.Capsules)
        {
            var collider = host.AddComponent<CapsuleCollider>();
            collider.center = capsule.Center;
            collider.radius = capsule.Radius;
            collider.height = capsule.Height;
            collider.direction = capsule.Direction;
        }

        state.Boxes.Clear();
        state.Spheres.Clear();
        state.Capsules.Clear();
        state.CollidersRemoved = false;
    }

    static void RenameAnchors(Transform root)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child == root || !child.name.StartsWith('#') || child.name.StartsWith(AlternativePrefix))
            {
                continue;
            }

            child.name = AlternativePrefix + child.name[1..];
        }
    }

    void GraftAnchors(BuildingStyleState state, GameObject style, List<IAnimationUpdater> updaters)
    {
        var moveNodeUpdaters = state.Animator && state.OriginalUpdaters is not null;
        var altAnchors = style.GetComponentsInChildren<Transform>(true)
            .Where(child => child.name.StartsWith(AlternativePrefix))
            .ToArray();

        foreach (var alt in altAnchors)
        {
            var originalName = "#" + alt.name[AlternativePrefix.Length..];
            if (!state.Anchors.TryGetValue(originalName, out var anchor) || !anchor)
            {
                continue;
            }

            anchor.SetPositionAndRotation(alt.position, alt.rotation);
            anchor.localScale = alt.localScale;
            if (moveNodeUpdaters)
            {
                MoveNodeUpdater(state, alt, anchor, updaters);
            }

            alt.SetParent(anchor, false);
            alt.localPosition = Vector3.zero;
            alt.localRotation = Quaternion.identity;
            alt.localScale = Vector3.one;
            state.Grafted.Add(alt.gameObject);
        }
    }

    void MoveNodeUpdater(BuildingStyleState state, Transform alt, Transform anchor, List<IAnimationUpdater> updaters)
    {
        var existing = alt.GetComponent<NodeAnimationUpdater>();
        if (!existing)
        {
            return;
        }

        var moved = instantiator.AddComponent<NodeAnimationUpdater>(anchor.gameObject);
        moved.AssignAnimationsId(existing._animationsId);
        state.AddedUpdaters.Add(moved);
        var index = updaters.IndexOf(existing);
        if (index >= 0)
        {
            updaters[index] = moved;
        }
        else
        {
            updaters.Add(moved);
        }

        Object.DestroyImmediate(existing);
    }

    TimbermeshAnimator? BindAnimation(BuildingStyleState state, GameObject style, List<IAnimationUpdater> updaters)
    {
        var styleAnimators = style.GetComponentsInChildren<TimbermeshAnimator>(true);
        TimbermeshAnimator? source = null;
        List<AnimationMetadata>? clips = null;
        foreach (var animator in styleAnimators)
        {
            if (source is null && animator._animations is { Count: > 0 })
            {
                source = animator;
                clips = [.. animator._animations];
            }
        }

        if (!state.Animator || state.OriginalUpdaters is null)
        {
            if (source)
            {
                return source;
            }

            return styleAnimators.Length > 0 ? styleAnimators[0] : null;
        }

        foreach (var animator in styleAnimators)
        {
            Object.DestroyImmediate(animator);
        }

        if (clips is null)
        {
            state.Animator.Stop();
            state.Animator._animationUpdaters = [];
            state.AnimationRebound = true;
            return null;
        }

        Play(state.Animator, clips, [.. updaters], initialize: true);
        state.AnimationRebound = true;
        return null;
    }

    static void RememberAnimation(BuildingStyleState state)
    {
        if (!state.Animator || state.Animator._animationUpdaters is null || state.OriginalUpdaters is not null)
        {
            return;
        }

        state.OriginalClips = state.Animator._animations is null ? [] : [.. state.Animator._animations];
        state.OriginalUpdaters = [.. state.Animator._animationUpdaters];
    }

    static void RestoreAnimation(BuildingStyleState state)
    {
        if (!state.AnimationRebound || !state.Animator || state.OriginalUpdaters is null || state.OriginalClips is null)
        {
            return;
        }

        if (state.OriginalClips.Count == 0)
        {
            state.Animator.Stop();
            state.Animator._animationUpdaters = [];
        }
        else
        {
            Play(state.Animator, state.OriginalClips, state.OriginalUpdaters, initialize: false);
        }

        state.AnimationRebound = false;
    }

    static void Play(
        TimbermeshAnimator animator,
        IEnumerable<AnimationMetadata> clips,
        IAnimationUpdater[] updaters,
        bool initialize)
    {
        var list = clips.ToList();
        animator.SetAnimations(list);
        animator._animationMap = list.ToDictionary(clip => clip.Name);
        animator._animationUpdaters = updaters;
        if (initialize)
        {
            foreach (var updater in updaters)
            {
                updater.Initialize();
            }
        }

        animator.Play(list[0].Name);
    }

    static void Refresh(AlternativeBuildingStyleComponent building)
    {
        var state = building.State;
        if (state.BuildingModel is BuildingModel model && model)
        {
            model.UpdateModelVisibility();
        }

        var slots = building.GetComponent<SlotManager>();
        if (slots)
        {
            slots.ReassignAllSlots();
        }
    }
}
