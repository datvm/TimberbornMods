namespace ConfigurableFaction.Services;

public class FactionOptionsService(
    FactionOptionsProvider provider,
    FactionInfoService info,
    ILoc t
)
{
    public static readonly FrozenDictionary<string, string> SpecialPairBuildings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Buildings/Monuments/EarthRepopulator/EarthRepopulator.IronTeeth", "Buildings/Monuments/EarthRepopulator/EarthRepopulator.IronTeeth.Plane" }
        }
        .ToFrozenDictionary();

    public void Initialize()
    {
        info.ScanFactions();

        foreach (var opt in provider.FactionOptions.Values)
        {
            InitializeSelfEntries(opt);
            RemoveInvalidEntries(opt);
        }

        foreach (var opt in provider.FactionOptions.Values)
        {
            LockIn(opt);
            provider.Save(opt.Id);
        }
    }

    public string? ValidateAddBuildings(FactionOptions opt, string path)
    {
        var building = info.PrefabsByPaths[path].PrefabSpec;

        // Check for planters
        var planter = building.GetComponentFast<PlanterBuildingSpec>();
        if (planter && opt.LockedOutPlantGroup.Contains(planter.PlantableResourceGroup))
        {
            return t.T("LV.CFac.ValidatePlanter", planter.PlantableResourceGroup);
        }

        return null;
    }
    public bool ValidateRemovingGood(FactionOptions opt, string id) => !opt.LockedInGoods.Contains(id);
    public bool ValidateRemovingNeed(FactionOptions opt, string id) => !opt.LockedInNeeds.Contains(id);

    public void AddBuilding(FactionOptions opt, string path)
    {
        var validation = ValidateAddBuildings(opt, path);
        if (validation is not null)
        {
            throw new InvalidOperationException(validation);
        }


        opt.Buildings.Add(path);
        if (SpecialPairBuildings.TryGetValue(path, out var pairPath))
        {
            opt.SpecialBuildings.Add(pairPath);
        }

        LockInForBuilding(opt, path);

        provider.Save(opt.Id);
    }

    public void RemoveBuilding(FactionOptions opt, string path)
    {
        if (!opt.Buildings.Remove(path)) { return; }
        if (SpecialPairBuildings.TryGetValue(path, out var pairPath))
        {
            opt.SpecialBuildings.Remove(pairPath);
        }

        LockIn(opt);
        provider.Save(opt.Id);
    }

    public void AddPlantable(FactionOptions opt, string path)
    {
        opt.Plantables.Add(path);
        LockInForPlant(opt, path);
        provider.Save(opt.Id);
    }

    public void RemovePlantable(FactionOptions opt, string path)
    {
        if (!opt.Plantables.Remove(path)) { return; }
        LockIn(opt);
        provider.Save(opt.Id);
    }

    public void AddNeed(FactionOptions opt, string id)
    {
        opt.Needs.Add(id);
        provider.Save(opt.Id);
    }

    public void RemoveNeed(FactionOptions opt, string id)
    {
        if (!ValidateRemovingNeed(opt, id))
        {
            throw new InvalidOperationException(t.T("LV.CFac.NeedLocked", id));
        }

        if (!opt.Needs.Remove(id)) { return; }
        LockIn(opt);
        provider.Save(opt.Id);
    }

    public void AddGood(FactionOptions opt, string id)
    {
        opt.Goods.Add(id);
        LockInForGood(opt, id);
        provider.Save(opt.Id);
    }

    public void RemoveGood(FactionOptions opt, string id)
    {
        if (!ValidateRemovingGood(opt, id))
        {
            throw new InvalidOperationException(t.T("LV.CFac.GoodLocked", id));
        }

        if (!opt.Goods.Remove(id)) { return; }
        LockIn(opt);
        provider.Save(opt.Id);
    }

    public string? GetPlantableResourceGroup(PrefabSpec spec)
    {
        var planter = spec.GetComponentFast<PlanterBuildingSpec>();
        return planter ? planter.PlantableResourceGroup : null;
    }

    void RemoveInvalidEntries(FactionOptions opt)
    {
        opt.Buildings.RemoveNoLongerExistEntries(info.PrefabsByPaths);
        opt.Plantables.RemoveNoLongerExistEntries(info.PrefabsByPaths);
        opt.Needs.RemoveNoLongerExistEntries(info.Needs);
        opt.Goods.RemoveNoLongerExistEntries(info.Goods);
    }

    void InitializeSelfEntries(FactionOptions opt)
    {
        opt.ExistingBuildingsPrefabName.Clear();
        opt.ExistingPlantablesPrefabName.Clear();
        opt.ExistingNeeds.Clear();
        opt.ExistingGoods.Clear();

        var faction = info.FactionsInfo!.Factions.First(q => q.Spec.Id == opt.Id);
        foreach (var b in faction.Buildings)
        {
            opt.ExistingBuildingsPrefabName.Add(b);
        }

        foreach (var p in faction.Plantables.SelectMany(q => q.Plants))
        {
            opt.ExistingPlantablesPrefabName.Add(p);
        }

        foreach (var n in faction.Needs)
        {
            opt.ExistingNeeds.Add(n.Id);
        }

        foreach (var g in faction.Goods)
        {
            opt.ExistingGoods.Add(g.Id);
        }
    }

    void LockIn(FactionOptions opt)
    {
        opt.LockedInGoods.Clear();
        opt.LockedInNeeds.Clear();
        opt.LockedOutPlantGroup.Clear();

        // Lock own buildings (for planter groups)
        var faction = info.FactionsInfo!.Factions.First(q => q.Spec.Id == opt.Id);

        foreach (var building in faction.Buildings)
        {
            LockInForBuilding(opt, building.Path);
        }

        // Lock added options
        foreach (var building in opt.Buildings)
        {
            LockInForBuilding(opt, building);
        }

        foreach (var plant in opt.Plantables)
        {
            LockInForPlant(opt, plant);
        }

        foreach (var good in opt.Goods)
        {
            LockInForGood(opt, good);
        }
    }

    void LockInForBuilding(FactionOptions opt, string path)
    {
        var prefab = info.PrefabsByPaths[path];
        var spec = prefab.PrefabSpec;

        CheckForManufactury(opt, spec);
        CheckForPlanters(opt, spec);
        CheckForBuildingCost(opt, spec);
        CheckForBuildingNeeds(opt, spec);
    }

    void LockInForPlant(FactionOptions opt, string path)
    {
        var plant = info.PrefabsByPaths[path].PrefabSpec;

        var cuttable = plant.GetComponentFast<CuttableSpec>();
        if (cuttable)
        {
            var good = cuttable.YielderSpec.Yield.GoodId;
            AddLockedInGood(opt, good);
        }

        var gatherable = plant.GetComponentFast<GatherableSpec>();
        if (gatherable)
        {
            var good = gatherable.YielderSpec.Yield.GoodId;
            AddLockedInGood(opt, good);
        }
    }

    void AddLockedInGood(FactionOptions opt, string id)
    {
        opt.Goods.Add(id);
        opt.LockedInGoods.Add(id);
        LockInForGood(opt, id);
    }

    void AddLockedInNeed(FactionOptions opt, string id)
    {
        opt.Needs.Add(id);
        opt.LockedInNeeds.Add(id);
    }

    void LockInForGood(FactionOptions opt, string id)
    {
        var goodSpec = info.Goods[id];

        foreach (var eff in goodSpec.ConsumptionEffects)
        {
            var need = eff.NeedId;
            AddLockedInNeed(opt, need);
        }
    }

    void CheckForBuildingCost(FactionOptions opt, PrefabSpec spec)
    {
        var buildingSpec = spec.GetComponentFast<BuildingSpec>();
        if (!buildingSpec) { return; }

        foreach (var cost in buildingSpec.BuildingCost)
        {
            AddLockedInGood(opt, cost.GoodId);
        }
    }

    void CheckForManufactury(FactionOptions opt, PrefabSpec spec)
    {
        var manufacturer = spec.GetComponentFast<ManufactorySpec>();
        if (!manufacturer) { return; }

        var recipes = manufacturer.ProductionRecipeIds.Map(info.Recipes);
        foreach (var recipe in recipes)
        {
            foreach (var item in recipe.Ingredients)
            {
                AddLockedInGood(opt, item.Id);
            }

            foreach (var item in recipe.Products)
            {
                AddLockedInGood(opt, item.Id);
            }

            if (recipe.ConsumesFuel)
            {
                AddLockedInGood(opt, recipe.Fuel);
            }
        }
    }

    void CheckForPlanters(FactionOptions opt, PrefabSpec spec)
    {
        var grp = GetPlantableResourceGroup(spec);
        if (grp is not null)
        {
            opt.LockedOutPlantGroup.Add(grp);
        }
    }

    void CheckForBuildingNeeds(FactionOptions opt, PrefabSpec spec)
    {
        CheckForBuildingNeed<WorkshopRandomNeedApplierSpec>(spec => spec._effects.Select(q => q.NeedId));
        CheckForBuildingNeed<ContinuousEffectBuildingSpec>(spec => spec._effects.Select(q => q.NeedId));
        CheckForBuildingNeed<WonderEffectControllerSpec>(spec => spec._effects.Select(q => q.NeedId));
        CheckForBuildingNeed<DwellingSpec>(spec => spec._sleepEffects.Select(q => q.NeedId));
        CheckForBuildingNeed<AttractionSpec>(spec => spec._effects.Select(q => q.NeedId));
        CheckForBuildingNeed<AreaNeedApplierSpec>(spec => [spec._effectPerHour._needId]);
        
        void CheckForBuildingNeed<T>(Func<T, IEnumerable<string>> needsFn) where T : BaseComponent
        {
            var comp = spec.GetComponentFast<T>();

            if (!comp) { return; }

            foreach (var needId in needsFn(comp))
            {
                AddLockedInNeed(opt, needId);
            }
        }
    }



}
