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
        if (info.FactionsInfo is null)
        {
            info.ScanFactions();
        }

        foreach (var opt in provider.FactionOptions.Values)
        {
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

        LockInBuilding(opt, path);

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
        LockInPlant(opt, path);
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
        opt.LockedInNeeds.Add(id);
        provider.Save(opt.Id);
    }

    public void RemoveNeed(FactionOptions opt, string id)
    {
        if (!ValidateRemovingNeed(opt, id))
        {
            throw new InvalidOperationException(t.T("LV.CFac.NeedLocked", id));
        }

        if (!opt.Needs.Remove(id)) { return; }
        opt.LockedInNeeds.Remove(id);
        provider.Save(opt.Id);
    }

    public void AddGood(FactionOptions opt, string id)
    {
        opt.Goods.Add(id);
        opt.LockedInGoods.Add(id);
        provider.Save(opt.Id);
    }

    public void RemoveGood(FactionOptions opt, string id)
    {
        if (!ValidateRemovingGood(opt, id))
        {
            throw new InvalidOperationException(t.T("LV.CFac.GoodLocked", id));
        }

        if (!opt.Goods.Remove(id)) { return; }
        opt.LockedInGoods.Remove(id);
        provider.Save(opt.Id);
    }

    void RemoveInvalidEntries(FactionOptions opt)
    {
        opt.Buildings.RemoveNoLongerExistEntries(info.PrefabsByPaths);
        opt.Plantables.RemoveNoLongerExistEntries(info.PrefabsByPaths);
        opt.Needs.RemoveNoLongerExistEntries(info.Needs);
        opt.Goods.RemoveNoLongerExistEntries(info.Goods);
    }

    void LockIn(FactionOptions opt)
    {
        opt.LockedInGoods.Clear();
        opt.LockedInNeeds.Clear();
        opt.LockedOutPlantGroup.Clear();

        // Lock own buildings
        var faction = info.FactionsInfo!.Factions.First(q => q.Spec.Id == opt.Id);

        foreach (var building in faction.Buildings)
        {
            LockInBuilding(opt, building.Path);
        }

        foreach (var plant in opt.Plantables)
        {
            LockInPlant(opt, plant);
        }

        foreach (var building in opt.Buildings)
        {
            LockInBuilding(opt, building);
        }
    }

    void LockInBuilding(FactionOptions opt, string path)
    {
        var prefab = info.PrefabsByPaths[path];
        var spec = prefab.PrefabSpec;

        CheckForManufactury(opt, spec);
        CheckForPlanters(opt, spec);
    }

    void LockInPlant(FactionOptions opt, string path)
    {
        var plant = info.PrefabsByPaths[path].PrefabSpec;

        var cuttable = plant.GetComponentFast<CuttableSpec>();
        if (cuttable)
        {
            var good = cuttable.YielderSpec.Yield.GoodId;
            AddAndLockInGood(opt, good);
        }

        var gatherable = plant.GetComponentFast<GatherableSpec>();
        if (gatherable)
        {
            var good = gatherable.YielderSpec.Yield.GoodId;
            AddAndLockInGood(opt, good);
        }
    }

    void AddAndLockInGood(FactionOptions opt, string id)
    {
        opt.Goods.Add(id);
        LockInGood(opt, id);
    }

    void LockInGood(FactionOptions opt, string id)
    {
        var goodSpec = info.Goods[id];

        foreach (var eff in goodSpec.ConsumptionEffects)
        {
            var need = eff.NeedId;
            opt.Needs.Add(need);
            opt.LockedInNeeds.Add(need);
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
                AddAndLockInGood(opt, item.Id);
            }

            foreach (var item in recipe.Products)
            {
                AddAndLockInGood(opt, item.Id);
            }

            if (recipe.ConsumesFuel)
            {
                AddAndLockInGood(opt, recipe.Fuel);
            }
        }
    }

    void CheckForPlanters(FactionOptions opt, PrefabSpec spec)
    {
        var planter = spec.GetComponentFast<PlanterBuildingSpec>();
        if (!planter) { return; }

        opt.LockedOutPlantGroup.Add(planter.PlantableResourceGroup);
    }

}
