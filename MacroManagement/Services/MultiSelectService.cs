namespace MacroManagement.Services;

public class MultiSelectService(
    EntityRegistry entities,
    Highlighter highlighter,
    EventBus eb,
    EntitySelectionService entitySelectionService,
    EntityService entityService,
    IInstantiator instantiator,
    EntityBadgeService entityBadgeService
) : ILoadableSingleton
{
    public void Load()
    {
        eb.Register(this);
    }

    public void SelectItems(in MacroManagementInfo info, MacroManagementSelectionFlags flags)
    {
        var matching = GetMatchingBuildings(info, flags);
        if (matching.IsDefaultOrEmpty) { return; }

        var mmComp = CreateDummyObject(info, matching);
        var entity = mmComp.GetComponentFast<EntityComponent>();
        entitySelectionService.Select(entity);

        // Highlight all
        foreach (var building in matching)
        {
            highlighter.HighlightPrimary(building, entitySelectionService._entitySelectionColor);
        }

        mmComp.OnSelectionChanged += OnBuildingSelectionChanged;
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

    MMComponent CreateDummyObject(MacroManagementInfo info, in ImmutableArray<PrefabSpec> matching)
    {
        var original = info.PrefabSpec;

        var marker = InstantiateDummyObject(original);
        marker.Init(matching, original, entityBadgeService);

        MapDummyComponents(marker, original);

        return marker;
    }

    void MapDummyComponents(MMComponent marker, PrefabSpec original)
    {
        var gameObj = marker.GameObjectFast;

        // All buildings
        CopyLabel(gameObj, original);
        CopyBuildingSpec(gameObj, original);
        CopySelectable(gameObj, original);

        PerformMap<DummyDistrictBuilding, DistrictBuilding>();
        PerformAdd<BlockableBuilding>();
        PerformMap<DummyBlockObject, BlockObject>();
        PerformMap<DummyRecoverableGoodProvider, RecoverableGoodProvider>();

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

        // Manufactory
        var manufactorySpec = original.GetComponentFast<ManufactorySpec>();

        if (manufactorySpec)
        {
            CopyManufactorySpec(gameObj, manufactorySpec);
            PerformMap<DummyManufactory, Manufactory>();
            PerformAdd<ManufactoryDropdownProvider>();
            PerformAdd<ManufactoryDescriber>();
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
        => instantiator.AddComponent<TComponent>(gameObj);

    TSelf? MapDummyComponent<TSelf, TComponent>(GameObject gameObj, MMComponent marker, PrefabSpec original)
        where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        var comp = original.GetComponentFast<TComponent>();
        if (!comp) { return null; }

        var dummy = instantiator.AddComponent<TSelf>(gameObj);
        dummy.MMComponent = marker;
        dummy.Init(comp);

        return dummy;
    }

    MMComponent InstantiateDummyObject(PrefabSpec original)
    {
        var gameObj = instantiator.InstantiateEmpty("DummySelection");

        var dummySpec = instantiator.AddComponent<PrefabSpec>(gameObj);
        dummySpec._prefabName = original._prefabName;

        var entity = entityService.Instantiate(dummySpec);

        if (gameObj != entity.GameObjectFast)
        {
            UnityEngine.Object.Destroy(gameObj);
        }

        gameObj = entity.GameObjectFast;

        var marker = instantiator.AddComponent<MMComponent>(gameObj);
        entity._componentCache = marker._componentCache;

        return marker;
    }

    void CopySelectable(GameObject gameObj, PrefabSpec originalPrefab)
    {
        var selectable = instantiator.AddComponent<SelectableObject>(gameObj);
        selectable._cameraTarget = new DummyCameraTarget(originalPrefab.GetComponentFast<ICameraTarget>().CameraTargetPosition);
    }

    void CopyLabel(GameObject gameObj, PrefabSpec originalPrefab)
    {
        {
            var originalSpec = originalPrefab.GetComponentFast<LabeledEntitySpec>();
            var spec = instantiator.AddComponent<LabeledEntitySpec>(gameObj);

            spec._displayNameLocKey = originalSpec._displayNameLocKey;
            spec._descriptionLocKey = originalSpec._descriptionLocKey;
            spec._flavorDescriptionLocKey = originalSpec._flavorDescriptionLocKey;
            spec._imagePath = originalSpec._imagePath;
        }

        {
            var originalSpec = originalPrefab.GetComponentFast<LabeledEntity>();
            var label = instantiator.AddComponent<LabeledEntity>(gameObj);

            label._displayName = originalSpec._displayName;
            label._image = originalSpec._image;
        }

        {
            var badge = instantiator.AddComponent<DummyEntityBadge>(gameObj);
            badge.Init(originalPrefab, entityBadgeService);
        }
    }

    void CopyBuildingSpec(GameObject gameObj, PrefabSpec originalPrefab)
    {
        var original = originalPrefab.GetComponentFast<BuildingSpec>();
        if (!original) { return; }

        var spec = instantiator.AddComponent<BuildingSpec>(gameObj);
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
        var spec = instantiator.AddComponent<WorkplaceSpec>(gameObj);
        spec._maxWorkers = original._maxWorkers;
        spec._defaultWorkers = original._defaultWorkers;
        spec._defaultWorkerType = original._defaultWorkerType;
        spec._disallowOtherWorkerTypes = original._disallowOtherWorkerTypes;
        spec._workerTypeUnlockCosts = original._workerTypeUnlockCosts;
    }

    void CopyStockpileSpec(GameObject gameObj, StockpileSpec original)
    {
        var spec = instantiator.AddComponent<StockpileSpec>(gameObj);
        spec._maxCapacity = original._maxCapacity;
        spec._whitelistedGoodType = original._whitelistedGoodType;
    }

    void CopyManufactorySpec(GameObject gameObj, ManufactorySpec original)
    {
        var spec = instantiator.AddComponent<ManufactorySpec>(gameObj);
        spec._productionRecipeIds = original._productionRecipeIds;
    }

    [OnEvent]
    public void OnEntityDeselect(SelectableObjectUnselectedEvent e)
    {
        highlighter.UnhighlightAllPrimary();

        var marker = e.SelectableObject.GetComponentFast<MMComponent>();
        if (marker)
        {
            marker.IsUnselecting = true;
            entityService.Delete(marker);
            UnityEngine.Object.Destroy(marker.GameObjectFast);
        }
    }

}
