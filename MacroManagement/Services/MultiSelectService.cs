namespace MacroManagement.Services;

public class MultiSelectService(
    EntityRegistry entities,
    Highlighter highlighter,
    EntitySelectionService entitySelectionService,
    EntityService entityService,
    IInstantiator instantiator,
    BaseInstantiator baseInstantiator,
    EntityBadgeService entityBadgeService,
    InputService inputService
) : ILoadableSingleton, IUnloadableSingleton
{
    public const string AltKeyId = "AlternateClickableAction";

    public static MultiSelectService? Instance { get; private set; }

    public void Load()
    {
        Instance = this;
    }

    public void SelectItems(in MacroManagementInfo info, MacroManagementSelectionFlags flags)
    {
        var matching = GetMatchingBuildings(info, flags);
        if (matching.IsDefaultOrEmpty) { return; }

        var mm = CreateCollectionFor(info.PrefabSpec, matching);
        entitySelectionService.Select(mm);
    }

    public MMComponent CreateCollectionFor(PrefabSpec originalPrefab, ImmutableArray<PrefabSpec> buildings)
    {
        if (originalPrefab.GetComponentFast<MMComponent>())
        {
            throw new InvalidOperationException("Original prefab cannot be a dummy object");
        }

        var mm = CreateDummyObject(originalPrefab, buildings);
        mm.GetComponentFast<EntityComponent>();

        // Highlight event
        mm.OnSelectionChanged += OnBuildingSelectionChanged;

        var selectable = mm.GetComponentFast<DummySelectableObject>();
        selectable.OnSelected += OnDummySelected;
        selectable.OnUnselected += OnDummyDeselected;

        return mm;
    }

    private void OnDummyDeselected(DummySelectableObject obj)
    {
        highlighter.UnhighlightAllPrimary();

        var mm = obj.MMComponent;
        if (!mm) { return; }

        mm.IsUnselecting = true;
        entityService.Delete(mm);
        UnityEngine.Object.Destroy(mm.GameObjectFast);
    }

    private void OnDummySelected(DummySelectableObject obj)
    {
        foreach (var building in obj.MMComponent.Buildings)
        {
            highlighter.HighlightPrimary(building.Prefab, entitySelectionService._entitySelectionColor);
        }
    }

    public SelectableObject? TryAddSelection(SelectableObject target)
    {        
        if (!inputService.IsKeyHeld(AltKeyId) // Only when holding Shift
            || !entitySelectionService.IsAnythingSelected // Only when something is selected
            || !target.GetComponentFast<BuildingSpec>()) // Only for buildings
        {
            return target;
        }

        var targetPrefab = target.GetComponentFast<PrefabSpec>();
        if (!targetPrefab) { return target; }

        var currSelection = entitySelectionService.SelectedObject;
        if (!currSelection) { return target; }

        var currPrefab = currSelection.GetComponentFast<PrefabSpec>();
        if (!currPrefab) { return target; }

        var mm = currPrefab.GetComponentFast<MMComponent>();
        
        if (mm)
        {
            // Check if the building is already selected
            var selected = mm.Buildings.FirstOrDefault(q => q.Prefab == targetPrefab);
            if (selected is not null)
            {
                selected.ToggleSelect(!selected.Select);

                return null;
            }

            currPrefab = mm.Original.Prefab;
        }
        if (currPrefab.Name != targetPrefab.Name) { return target; }

        var dummy = CreateCollectionFor(
            currPrefab,
            [
                .. mm ? mm.Buildings.Select(q => q.Prefab) : [currPrefab],
                targetPrefab
            ]);
        return dummy.GetComponentFast<SelectableObject>();
    }

    private void OnBuildingSelectionChanged(MMBuildingItem building, bool select)
    {
        if (select)
        {
            highlighter.HighlightPrimary(building.Prefab, entitySelectionService._entitySelectionColor);
        }
        else
        {
            highlighter.UnhighlightPrimary(building.Prefab);
        }
    }

    public ImmutableArray<PrefabSpec> GetMatchingBuildings(in MacroManagementInfo info, MacroManagementSelectionFlags flags)
    {
        var name = info.PrefabSpec.Name;

        var hasDistrictFlag = (flags & MacroManagementSelectionFlags.District) != 0;
        var districtCenter = info.DistrictCenter;
        if (!districtCenter && hasDistrictFlag) { return []; }

        List<PrefabSpec> result = [];
        foreach (var entity in entities.Entities)
        {
            if (!entity || !entity.Initialized || entity.Deleted) { continue; }

            var prefab = entity.GetComponentFast<PrefabSpec>();
            if (!prefab || prefab.Name != name) { continue; }

            var pausable = entity.GetComponentFast<PausableBuilding>();
            if (pausable)
            {
                if (pausable.Paused)
                {
                    if ((flags & MacroManagementSelectionFlags.Paused) == 0) { continue; }
                }
                else if ((flags & MacroManagementSelectionFlags.Running) == 0)
                {
                    continue;
                }
            }

            if (hasDistrictFlag)
            {
                var district = entity.GetComponentFast<DistrictBuilding>();
                if (!district || district.District != districtCenter)
                {
                    continue;
                }
            }

            result.Add(prefab);
        }

        return [.. result];
    }

    MMComponent CreateDummyObject(PrefabSpec originalPrefab, in ImmutableArray<PrefabSpec> matching)
    {
        var marker = InstantiateDummyObject(originalPrefab);
        marker.Init(matching, originalPrefab, entityBadgeService);

        MapDummyComponents(marker, originalPrefab);

        return marker;
    }

    void MapDummyComponents(MMComponent marker, PrefabSpec original)
    {
        var gameObj = marker.GameObjectFast;

        // All buildings
        CopyLabel(gameObj, original);
        CopyBuildingSpec(gameObj, original);
        PerformMap<DummySelectableObject, SelectableObject>();
        PerformMap<DummyDistrictBuilding, DistrictBuilding>();
        PerformAdd<BlockableBuilding>();
        PerformMap<DummyBlockObject, BlockObject>();
        PerformMap<DummyRecoverableGoodProvider, RecoverableGoodProvider>();
        PerformAdd<BlockObjectDeletionDescriber>();

        // Pause/Unpause
        PerformMap<DummyPausableBuilding, PausableBuilding>();

        // Workplaces
        var workplaceSpec = original.GetComponentFast<WorkplaceSpec>();

        if (workplaceSpec)
        {
            CopyWorkplaceSpec(gameObj, workplaceSpec);

            PerformMap<DummyWorkplaceWorkerType, WorkplaceWorkerType>();
            PerformMap<DummyWorkplacePriority, WorkplacePriority>();
            PerformMap<DummyWorkplace, Workplace>();

            PerformAdd<WorkplaceDescriber>();
        }

        // Stockpile
        var stockpileSpec = original.GetComponentFast<StockpileSpec>();

        if (stockpileSpec)
        {
            CopyStockpileSpec(gameObj, stockpileSpec);
            PerformMap<DummyStockpile, Stockpile>();
            PerformAdd<StockpileDropdownProvider>();
            PerformAdd<StockpileDescriber>();
            PerformMap<DummySingleGoodAllower, SingleGoodAllower>();
        }

        var goodObtainer = original.GetComponentFast<GoodObtainer>();
        if (goodObtainer)
        {
            PerformMap<DummyGoodObtainer, GoodObtainer>();
            PerformMap<DummyEmptiable, Emptiable>();
            PerformMap<DummyGoodSupplier, GoodSupplier>();
        }

        // Manufactory
        var manufactorySpec = original.GetComponentFast<ManufactorySpec>();

        if (manufactorySpec)
        {
            CopyManufactorySpec(gameObj, manufactorySpec);
            PerformMap<DummyManufactory, Manufactory>();
            PerformAdd<ManufactoryDropdownProvider>();
            PerformAdd<ManufactoryDescriber>();
        }

        // Hauler
        var haulCandidate = original.GetComponentFast<HaulCandidate>();
        if (haulCandidate)
        {
            PerformMap<DummyHaulCandidate, HaulCandidate>();
            PerformMap<DummyHaulPrioritizable, HaulPrioritizable>();
        }

        TComponent PerformAdd<TComponent>()
            where TComponent : BaseComponent
        {
            return MapEmptyComponent<TComponent>(gameObj);
        }

        TSelf? PerformMap<TSelf, TComponent>()
            where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
            where TComponent : BaseComponent
        {
            return MapDummyComponent<TSelf, TComponent>(gameObj, marker, original);
        }
    }

    TComponent MapEmptyComponent<TComponent>(GameObject gameObj)
        where TComponent : BaseComponent
        => baseInstantiator.AddComponent<TComponent>(gameObj);

    TSelf? MapDummyComponent<TSelf, TComponent>(GameObject gameObj, MMComponent marker, PrefabSpec original)
        where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        var comp = original.GetComponentFast<TComponent>();
        if (!comp) { return null; }

        var dummy = baseInstantiator.AddComponent<TSelf>(gameObj);
        dummy.MMComponent = marker;
        dummy.Init(comp);

        return dummy;
    }

    MMComponent InstantiateDummyObject(PrefabSpec original)
    {
        var gameObj = instantiator.InstantiateEmpty("DummySelection");

        var dummySpec = instantiator.AddComponent<PrefabSpec>(gameObj); // This one use instantiator because it's not initialized yet
        dummySpec._prefabName = original._prefabName;

        var entity = entityService.Instantiate(dummySpec);

        if (gameObj != entity.GameObjectFast)
        {
            UnityEngine.Object.Destroy(gameObj);
        }

        gameObj = entity.GameObjectFast;

        var marker = baseInstantiator.AddComponent<MMComponent>(gameObj);
        entity._componentCache = marker._componentCache;

        return marker;
    }

    void CopyLabel(GameObject gameObj, PrefabSpec originalPrefab)
    {
        {
            var originalSpec = originalPrefab.GetComponentFast<LabeledEntitySpec>();
            var spec = baseInstantiator.AddComponent<LabeledEntitySpec>(gameObj);

            spec._displayNameLocKey = originalSpec._displayNameLocKey;
            spec._descriptionLocKey = originalSpec._descriptionLocKey;
            spec._flavorDescriptionLocKey = originalSpec._flavorDescriptionLocKey;
            spec._imagePath = originalSpec._imagePath;
        }

        {
            var originalSpec = originalPrefab.GetComponentFast<LabeledEntity>();
            var label = baseInstantiator.AddComponent<LabeledEntity>(gameObj);

            label._displayName = originalSpec._displayName;
            label._image = originalSpec._image;
        }

        {
            var badge = baseInstantiator.AddComponent<DummyEntityBadge>(gameObj);
            badge.Init(originalPrefab, entityBadgeService);
        }
    }

    void CopyBuildingSpec(GameObject gameObj, PrefabSpec originalPrefab)
    {
        var original = originalPrefab.GetComponentFast<BuildingSpec>();
        if (!original) { return; }

        var spec = baseInstantiator.AddComponent<BuildingSpec>(gameObj);
        spec._selectionSoundName = original._selectionSoundName;
        spec._loopingSoundName = original._loopingSoundName;
        spec._buildingCost = original._buildingCost;
        spec._scienceCost = original._scienceCost;
        spec._placeFinished = original._placeFinished;
        spec._finishableWithBeaversOnSite = original._finishableWithBeaversOnSite;
        spec._drawRangeBoundsOnIt = original._drawRangeBoundsOnIt;
    }

    void CopyWorkplaceSpec(GameObject gameObj, WorkplaceSpec original)
    {
        var spec = baseInstantiator.AddComponent<WorkplaceSpec>(gameObj);
        spec._maxWorkers = original._maxWorkers;
        spec._defaultWorkers = original._defaultWorkers;
        spec._defaultWorkerType = original._defaultWorkerType;
        spec._disallowOtherWorkerTypes = original._disallowOtherWorkerTypes;
        spec._workerTypeUnlockCosts = original._workerTypeUnlockCosts;
    }

    void CopyStockpileSpec(GameObject gameObj, StockpileSpec original)
    {
        var spec = baseInstantiator.AddComponent<StockpileSpec>(gameObj);
        spec._maxCapacity = original._maxCapacity;
        spec._whitelistedGoodType = original._whitelistedGoodType;
    }

    void CopyManufactorySpec(GameObject gameObj, ManufactorySpec original)
    {
        var spec = baseInstantiator.AddComponent<ManufactorySpec>(gameObj);
        spec._productionRecipeIds = original._productionRecipeIds;
    }

    public void Unload()
    {
        Instance = null;
    }
}
