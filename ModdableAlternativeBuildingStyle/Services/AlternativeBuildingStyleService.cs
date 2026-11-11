namespace ModdableAlternativeBuildingStyle.Services;

[BindSingleton]
public class AlternativeBuildingStyleService(
    ISpecService specService,
    OptimizedPrefabInstantiator prefabs,
    IInstantiator instantiator,
    MaterialColorer materialColorer,
    MaterialHeightCutoffSetter cutoff,
    ConstructionModeService constructionMode,
    EventBus events) : ILoadableSingleton
{
    const string AlternativePrefix = "#Alternative-";
    static readonly int EnableDetailId = Shader.PropertyToID("_EnableDetail");
    static readonly int DetailTextureId = Shader.PropertyToID("_DetailAlbedoMap2");
    static readonly int DetailColorId = Shader.PropertyToID("_DetailAlbedoUV2Color");
    readonly List<AlternativeBuildingStyleComponent> styledUnfinished = [];

    public void Load()
    {
        events.Register(this);
    }

    internal void Release(AlternativeBuildingStyleComponent building)
    {
        styledUnfinished.Remove(building);
    }

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

        PointPaintedRenderers(building, styled: false);

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
            if (!home.Target || state.KeptVisuals.Contains(home.Target))
            {
                continue;
            }

            home.Target.SetParent(home.Parent, false);
            home.Target.localPosition = home.LocalPosition;
            home.Target.localRotation = home.LocalRotation;
            home.Target.localScale = home.LocalScale;
        }

        state.KeptVisuals.Clear();

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
        ShowReadyFruit(instance);
        GraftVariantHosts(state, instance);
        ParkOriginalVisuals(state);
        HideAnchorRenderers(state);
        RemoveOriginalColliders(state);
        GraftAnchors(state, instance, updaters, PreservedGate(style), PreservedCannon(style));
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
            if (visual && !state.KeptVisuals.Contains(visual))
            {
                visual.SetParent(state.OriginalHolder.transform, true);
            }
        }
    }

    // Fruit is the detail layer. The shared material leaves it off until the plant has a yield.
    static void ShowReadyFruit(GameObject style)
    {
        foreach (var renderer in style.GetComponentsInChildren<MeshRenderer>(true))
        {
            var show = false;
            foreach (var slot in renderer.sharedMaterials)
            {
                if (slot && (
                    slot.name.StartsWith("Blueberry")
                    || slot.name.StartsWith("CoffeeBush")
                    || slot.name.StartsWith("Mangrove")))
                {
                    show = true;
                    break;
                }
            }

            if (!show)
            {
                continue;
            }

            foreach (var material in renderer.materials)
            {
                if (material && material.HasProperty(EnableDetailId))
                {
                    material.SetFloat(EnableDetailId, 1f);
                }
            }
        }
    }

    static void GraftVariantHosts(BuildingStyleState state, GameObject style)
    {
        Dictionary<string, Transform> hosts = [];
        for (var i = 0; i < state.Finished.childCount; i++)
        {
            var child = state.Finished.GetChild(i);
            if (!child.name.StartsWith('#'))
            {
                hosts.TryAdd(child.name, child);
            }
        }

        var root = style.transform;
        for (var i = root.childCount - 1; i >= 0; i--)
        {
            var piece = root.GetChild(i);
            if (!hosts.TryGetValue(piece.name, out var host))
            {
                continue;
            }

            state.KeptVisuals.Add(host);
            piece.SetParent(host, false);
            piece.localPosition = Vector3.zero;
            piece.localRotation = Quaternion.identity;
            piece.localScale = Vector3.one;
            piece.gameObject.SetActive(true);
            state.Grafted.Add(piece.gameObject);
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

    // The floodgate listener animates the style gate in the style instance. Parenting that
    // node under the live gate would put its height on top of the original gate's height.
    string? PreservedGate(AlternativeBuildingStyle style)
    {
        var blueprint = specService.GetBlueprint(style.BlueprintPath);
        if (!blueprint.HasSpec<FloodgateAnimationStyleSpec>())
        {
            return null;
        }

        var gateName = blueprint.GetSpec<FloodgateAnimationStyleSpec>().GateName;
        return string.IsNullOrEmpty(gateName) ? null : gateName;
    }

    // The launcher writes local rotation on the cached turret and barrel. Parenting those
    // style nodes under the live cannon would stack that rotation on the hidden parts.
    FireworkLauncherAnimationStyleSpec? PreservedCannon(AlternativeBuildingStyle style)
    {
        var blueprint = specService.GetBlueprint(style.BlueprintPath);
        if (!blueprint.HasSpec<FireworkLauncherAnimationStyleSpec>())
        {
            return null;
        }

        var cannon = blueprint.GetSpec<FireworkLauncherAnimationStyleSpec>();
        if (string.IsNullOrEmpty(cannon.Turret) && string.IsNullOrEmpty(cannon.Barrel))
        {
            return null;
        }

        return cannon;
    }

    static bool PreservedPart(string originalName, string? preservedGate, FireworkLauncherAnimationStyleSpec? preservedCannon)
    {
        if (originalName == preservedGate)
        {
            return true;
        }

        if (preservedCannon is null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(preservedCannon.Turret) && originalName == preservedCannon.Turret)
        {
            return true;
        }

        return !string.IsNullOrEmpty(preservedCannon.Barrel) && originalName == preservedCannon.Barrel;
    }

    void GraftAnchors(
        BuildingStyleState state,
        GameObject style,
        List<IAnimationUpdater> updaters,
        string? preservedGate,
        FireworkLauncherAnimationStyleSpec? preservedCannon)
    {
        var moveNodeUpdaters = state.Animator && state.OriginalUpdaters is not null;
        var altAnchors = style.GetComponentsInChildren<Transform>(true)
            .Where(child => child.name.StartsWith(AlternativePrefix))
            .ToArray();
        var taken = new HashSet<Transform>();
        var grafted = new HashSet<Transform>();

        void Graft(Transform alt, Transform anchor, bool moveAnchor)
        {
            if (!anchor || !taken.Add(anchor) || !grafted.Add(alt))
            {
                return;
            }

            // Exact names keep the style node's pose, which is how Metal floodgates line up.
            // A faction alias leaves the cached transform where the component already drives it.
            if (moveAnchor)
            {
                anchor.SetPositionAndRotation(alt.position, alt.rotation);
                anchor.localScale = alt.localScale;
            }

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

        foreach (var alt in altAnchors)
        {
            var originalName = "#" + alt.name[AlternativePrefix.Length..];
            if (PreservedPart(originalName, preservedGate, preservedCannon))
            {
                continue;
            }

            if (!state.Anchors.TryGetValue(originalName, out var anchor))
            {
                continue;
            }

            Graft(alt, anchor, moveAnchor: true);
        }

        foreach (var alt in altAnchors)
        {
            if (grafted.Contains(alt))
            {
                continue;
            }

            var originalName = "#" + alt.name[AlternativePrefix.Length..];
            if (PreservedPart(originalName, preservedGate, preservedCannon))
            {
                continue;
            }

            var other = FactionNames.Counterpart(originalName);
            if (other is null || !state.Anchors.TryGetValue(other, out var anchor))
            {
                continue;
            }

            Graft(alt, anchor, moveAnchor: false);
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
        var normalized = NormalizedTime(animator);
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
        var length = animator.AnimationLength;
        if (length > 0f)
        {
            animator.SetTime(normalized * length);
        }
    }

    static float NormalizedTime(TimbermeshAnimator animator)
    {
        var length = animator.AnimationLength;
        if (length <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(animator.RepeatedTime / length);
    }

    void Refresh(AlternativeBuildingStyleComponent building)
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

        // Lights and the door are cached at Awake. Push the occupied state onto the meshes that are showing now.
        SyncOccupiedVisuals(building);
        SyncConstructionLook(building);
        var styled = building.State.StyleInstance != null;
        PointPaintedRenderers(building, styled);
        PaintStyleMaterials(building, styled);
        RefreshStatusIcon(building);
    }

    internal void SyncMovedStyle(AlternativeBuildingStyleComponent building)
    {
        if (!building.State.StyleInstance)
        {
            return;
        }

        PaintGroundCutoff(building, styled: true);
    }

    // A style replaces the finished mesh and has no scaffold of its own.
    // Keep the original #Unfinished frame up for the whole build, selected or not.
    // Grayscale stays on the hidden style so that mesh never draws in full color.
    void SyncConstructionLook(AlternativeBuildingStyleComponent building)
    {
        var state = building.State;
        if (!state.Captured || !state.Finished)
        {
            styledUnfinished.Remove(building);
            return;
        }

        if (!IsUnfinished(building))
        {
            styledUnfinished.Remove(building);
            ClearGrayscale(state);
            return;
        }

        if (state.StyleInstance)
        {
            if (!styledUnfinished.Contains(building))
            {
                styledUnfinished.Add(building);
            }

            materialColorer.EnableGrayscale(state.Finished.gameObject);
            ShowVanillaUnfinished(building);
            return;
        }

        styledUnfinished.Remove(building);
        // Originals may have been parked during the game's grayscale pass. Put that gray back
        // so the vanilla finished mesh is gray if a construction tool or selection shows it.
        materialColorer.EnableGrayscale(state.Finished.gameObject);
        if (constructionMode.InConstructionMode)
        {
            ShowVanillaFinished(building);
            return;
        }

        ShowVanillaUnfinished(building);
    }

    static void ShowVanillaFinished(AlternativeBuildingStyleComponent building)
    {
        if (building.State.BuildingModel is BuildingModel model && model)
        {
            model.ShowFinishedModel();
        }
    }

    static void ShowVanillaUnfinished(AlternativeBuildingStyleComponent building)
    {
        if (building.State.BuildingModel is BuildingModel model && model)
        {
            model.ShowUnfinishedModel();
        }
    }

    void ClearGrayscale(BuildingStyleState state)
    {
        materialColorer.DisableGrayscale(state.Finished.gameObject);
        if (state.OriginalHolder)
        {
            materialColorer.DisableGrayscale(state.OriginalHolder);
        }
    }

    static bool IsUnfinished(AlternativeBuildingStyleComponent building)
    {
        var block = building.GetComponent<BlockObject>();
        if (!block)
        {
            return false;
        }

        return block.IsUnfinished;
    }

    [OnEvent]
    public void OnEnteredUnfinishedState(EnteredUnfinishedStateEvent entered)
    {
        SyncFromBlock(entered.BlockObject);
    }

    [OnEvent]
    public void OnEnteredFinishedState(EnteredFinishedStateEvent entered)
    {
        SyncFromBlock(entered.BlockObject);
    }

    // Entering construction mode shows every finished mesh. Put the scaffold back on a styled site.
    // Leaving the mode keeps that scaffold up.
    [OnEvent]
    public void OnConstructionModeChanged(ConstructionModeChangedEvent changed)
    {
        for (var i = styledUnfinished.Count - 1; i >= 0; i--)
        {
            var building = styledUnfinished[i];
            if (!building || !building.State.StyleInstance || !IsUnfinished(building))
            {
                styledUnfinished.RemoveAt(i);
                continue;
            }

            if (changed.InConstructionMode && building.State.Finished)
            {
                materialColorer.EnableGrayscale(building.State.Finished.gameObject);
            }

            ShowVanillaUnfinished(building);
        }
    }

    void SyncFromBlock(BlockObject block)
    {
        var building = block.GetComponent<AlternativeBuildingStyleComponent>();
        if (!building)
        {
            return;
        }

        SyncConstructionLook(building);
    }

    static void SyncOccupiedVisuals(AlternativeBuildingStyleComponent building)
    {
        var state = building.State;
        var materials = building.GetComponent<EntityMaterials>();
        if (state.StyleInstance)
        {
            if (materials)
            {
                materials.AddMaterials(state.StyleInstance);
                foreach (var grafted in state.Grafted)
                {
                    if (grafted)
                    {
                        materials.AddMaterials(grafted);
                    }
                }
            }
        }
        else if (materials)
        {
            ForgetDestroyedMaterials(materials);
        }

        var lighting = building.GetComponent<MaterialLightingRenderers>();
        if (lighting)
        {
            lighting.CollectRenderers();
            DropParkedRenderers(lighting, state.OriginalHolder);
        }

        var illuminator = building.GetComponent<Illuminator>();
        if (illuminator)
        {
            illuminator._topColor = null;
            illuminator.UpdateColor();
            illuminator.UpdateStrength();
            KeepParkedLightsActive(illuminator, state);
        }

        if (!state.Animator)
        {
            return;
        }

        building.GetComponent<MechanicalNodeAnimator>()?.FindAndUpdateActiveAnimator();
        building.GetComponent<WorkshopAnimationController>()?.UpdateAnimatorState();
        building.GetComponent<EnterableAnimationController>()?.UpdateAnimatorState();
    }

    // Destroying the style object kills its transforms. Renderer.materials instances stay in the list,
    // and GetChildMaterials then calls IsChildOf on the null child.
    static void ForgetDestroyedMaterials(EntityMaterials materials)
    {
        var list = materials._childMaterials;
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (entry.Child && entry.Material)
            {
                continue;
            }

            if (entry.Material)
            {
                Object.Destroy(entry.Material);
            }

            list.RemoveAt(i);
        }
    }

    static void DropParkedRenderers(MaterialLightingRenderers lighting, GameObject holder)
    {
        if (!holder)
        {
            return;
        }

        var parked = holder.transform;
        var renderers = lighting._renderers;
        for (var i = renderers.Count - 1; i >= 0; i--)
        {
            var renderer = renderers[i];
            if (renderer && renderer.transform.IsChildOf(parked))
            {
                renderers.RemoveAt(i);
            }
        }
    }

    static void KeepParkedLightsActive(Illuminator illuminator, BuildingStyleState state)
    {
        var lights = illuminator._illuminatorLightObjects;
        if (!lights || !state.OriginalHolder)
        {
            return;
        }

        var parked = state.OriginalHolder.transform;
        foreach (var group in lights._lightObjects.Values)
        {
            foreach (var light in group)
            {
                if (light && light.transform.IsChildOf(parked))
                {
                    light.transform.SetParent(state.Finished, true);
                }
            }
        }
    }

    void PointPaintedRenderers(AlternativeBuildingStyleComponent building, bool styled)
    {
        var state = building.State;
        var stockpile = building.GetComponent<StockpileBannerSetter>();
        if (stockpile)
        {
            if (!state.StockpileBanner)
            {
                state.StockpileBanner = stockpile._meshRenderer;
            }

            if (state.StockpileBanner)
            {
                stockpile._meshRenderer = styled
                    ? VisibleBanner(state, state.StockpileBanner) ?? state.StockpileBanner
                    : state.StockpileBanner;
                stockpile.UpdateProperties();
            }
        }

        var counter = building.GetComponent<ResourceCounterBannerSetter>();
        if (counter)
        {
            if (!state.ResourceBanner)
            {
                state.ResourceBanner = counter._meshRenderer;
            }

            if (state.ResourceBanner)
            {
                counter._meshRenderer = styled
                    ? VisibleBanner(state, state.ResourceBanner) ?? state.ResourceBanner
                    : state.ResourceBanner;
                counter.UpdateProperties();
            }
        }

        PointDecal(building, styled);
    }

    void PointDecal(AlternativeBuildingStyleComponent building, bool styled)
    {
        var icon = building.GetComponent<DecalSupplierBuildingIcon>();
        if (!icon)
        {
            return;
        }

        var state = building.State;
        if (!state.DecalIcon)
        {
            state.DecalIcon = icon._iconRenderer;
        }

        if (!state.DecalIcon)
        {
            return;
        }

        var next = state.DecalIcon;
        if (styled)
        {
            next = ChildRenderer(state.DecalIcon.transform, state.DecalIcon) ?? state.DecalIcon;
        }

        icon._iconRenderer = next;
        icon.UpdateIcon();
    }

    MeshRenderer? VisibleBanner(BuildingStyleState state, MeshRenderer original)
    {
        var underAnchor = ChildRenderer(original.transform, original);
        if (underAnchor)
        {
            return underAnchor;
        }

        if (!state.StyleInstance)
        {
            return null;
        }

        var finished = FindNamed(state.StyleInstance.transform, AlternativePrefix + "Finished");
        var root = finished ? finished : state.StyleInstance.transform;
        foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer != original && !renderer.forceRenderingOff)
            {
                return renderer;
            }
        }

        return null;
    }

    static MeshRenderer? ChildRenderer(Transform root, MeshRenderer skip)
    {
        foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer == skip || renderer.forceRenderingOff)
            {
                continue;
            }

            return renderer;
        }

        return null;
    }

    void PaintStyleMaterials(AlternativeBuildingStyleComponent building, bool styled)
    {
        PaintDetailTexture(building, styled);
        PaintGroundCutoff(building, styled);
        PaintMapCutoff(building, styled);
        PaintKeptLighting(building, styled);
    }

    void PaintDetailTexture(AlternativeBuildingStyleComponent building, bool styled)
    {
        if (!styled || !building.State.StyleInstance)
        {
            return;
        }

        var spec = building.GetComponent<BuildingDetailTextureSpec>();
        if (spec is null)
        {
            return;
        }

        var texture = spec.Texture.Asset;
        if (!texture)
        {
            return;
        }

        void Paint(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                foreach (var material in renderer.materials)
                {
                    if (!material)
                    {
                        continue;
                    }

                    material.SetTexture(DetailTextureId, texture);
                    material.SetColor(DetailColorId, spec.Color);
                }
            }
        }

        Paint(building.State.StyleInstance);
        foreach (var grafted in building.State.Grafted)
        {
            if (grafted)
            {
                Paint(grafted);
            }
        }
    }

    void PaintGroundCutoff(AlternativeBuildingStyleComponent building, bool styled)
    {
        var spec = building.GetComponent<BuildingModelGroundCutoffSpec>();
        var center = building.GetComponent<BlockObjectCenter>();
        if (spec is null || !center)
        {
            return;
        }

        var height = center.WorldCenterGrounded.y + spec.Offset;
        foreach (var target in spec.Targets)
        {
            PaintCutoff(building, target, height, styled);
        }
    }

    void PaintMapCutoff(AlternativeBuildingStyleComponent building, bool styled)
    {
        var spec = building.GetComponent<MapBottomGroundCutoffSpec>();
        if (spec is null)
        {
            return;
        }

        foreach (var target in spec.Targets)
        {
            PaintCutoff(building, target, -1f, styled);
        }
    }

    void PaintCutoff(AlternativeBuildingStyleComponent building, string target, float height, bool styled)
    {
        var host = FindNamed(building.Transform, target);
        if (!styled)
        {
            var own = host ? host.GetComponent<MeshRenderer>() : null;
            if (own)
            {
                SetCutoff(own, height);
            }

            return;
        }

        var painted = new HashSet<MeshRenderer>();
        if (host)
        {
            CollectVisible(host, painted);
        }

        var style = building.State.StyleInstance;
        if (style)
        {
            CollectNamed(style.transform, target, painted);
            var other = FactionNames.Counterpart(target);
            if (other is not null)
            {
                CollectNamed(style.transform, other, painted);
            }
        }

        foreach (var renderer in painted)
        {
            SetCutoff(renderer, height);
        }
    }

    void SetCutoff(MeshRenderer renderer, float height)
    {
        foreach (var material in renderer.materials)
        {
            if (material)
            {
                cutoff.SetCutoff(material, height);
            }
        }
    }

    static void CollectVisible(Transform host, HashSet<MeshRenderer> painted)
    {
        var own = host.GetComponent<MeshRenderer>();
        foreach (var renderer in host.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer == own && own.forceRenderingOff)
            {
                continue;
            }

            if (renderer.forceRenderingOff)
            {
                continue;
            }

            painted.Add(renderer);
        }
    }

    static void CollectNamed(Transform root, string name, HashSet<MeshRenderer> painted)
    {
        var named = FindNamed(root, name) ?? FindNamed(root, Renamed(name));
        if (!named)
        {
            return;
        }

        var renderer = named.GetComponent<MeshRenderer>();
        if (renderer && !renderer.forceRenderingOff)
        {
            painted.Add(renderer);
            return;
        }

        foreach (var child in named.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (!child.forceRenderingOff)
            {
                painted.Add(child);
            }
        }
    }

    void PaintKeptLighting(AlternativeBuildingStyleComponent building, bool styled)
    {
        var enforcer = building.GetComponent<FinishedStateLightingEnforcer>();
        var spec = building.GetComponent<FinishedStateLightingEnforcerSpec>();
        if (!enforcer || spec is null)
        {
            return;
        }

        foreach (var name in spec.ChildrenNames)
        {
            var host = FindNamed(building.Transform, name);
            if (host)
            {
                materialColorer.EnableLightingAndDisableChanges(enforcer, host.gameObject);
            }

            if (!styled || !building.State.StyleInstance)
            {
                continue;
            }

            var other = FactionNames.Counterpart(name);
            if (other is null)
            {
                continue;
            }

            var extra = FindNamed(building.State.StyleInstance.transform, other)
                ?? FindNamed(building.State.StyleInstance.transform, Renamed(other));
            if (extra && extra != host)
            {
                materialColorer.EnableLightingAndDisableChanges(enforcer, extra.gameObject);
            }
        }
    }

    static void RefreshStatusIcon(AlternativeBuildingStyleComponent building)
    {
        var offsetter = building.GetComponent<BuildingStatusIconOffsetter>();
        if (!offsetter)
        {
            return;
        }

        offsetter.RefreshTopBounds();
        offsetter.UpdateIcon();
    }

    static string Renamed(string name)
    {
        if (!name.StartsWith('#'))
        {
            return name;
        }

        return AlternativePrefix + name[1..];
    }

    static Transform? FindNamed(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindNamed(root.GetChild(i), name);
            if (found)
            {
                return found;
            }
        }

        return null;
    }
}
