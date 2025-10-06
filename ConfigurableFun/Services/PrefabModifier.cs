namespace ConfigurableFun.Services;

public class PrefabModifier(
    MSettings s,
    FactionNeedService factionNeedService
) : IPrefabModifier
{
    public int Order { get; } = 1000;

    public GameObject? Modify(GameObject prefab, PrefabSpec prefabSpec, GameObject original)
    {
        var changed = false;
        var c = s.Current;

        if (c.PointChanged)
        {
            changed = true;

            var eff = GetEffectSpec(prefabSpec)!;
            var mul = c.PointMul;

            var newEff = eff.Select(q => new ContinuousEffectSpec()
            {
                _needId = q.NeedId,
                _pointsPerHour = q.PointsPerHour * mul,
                _satisfyToMaxValue = q.SatisfyToMaxValue,
            }).ToList();
            eff.Clear();
            eff.AddRange(newEff);

            TimberUiUtils.LogDev($"Updated {prefab} {eff[0].NeedId} to {eff[0].PointsPerHour}");
        }

        if (c.CapacityChanged)
        {
            var enterable = prefab.GetComponent<EnterableSpec>();
            // Make sure it's not a workplace (Wonder)
            if (enterable && !prefab.GetComponent<WorkplaceSpec>())
            {
                changed = true;
                var newCapacity = enterable._capacityFinished = Math.Max(1, (int)(enterable._capacityFinished * c.CapacityMul) + c.CapacityAdd);
                ModdableTimberbornUtils.AddSlotsToPrefab(enterable, newCapacity);
            }
        }

        if (c.RangeChanged)
        {
            var rangeComp = prefab.GetComponent<RangedEffectBuildingSpec>();
            if (rangeComp)
            {
                changed = true;
                rangeComp._effectRadius = Math.Max(1, (int)(rangeComp._effectRadius * c.RangeMul) + c.RangeAdd);
            }
        }

        return changed ? prefab : null;
    }

    public bool ShouldModify(string prefabName, PrefabSpec prefabSpec)
    {
        var c = s.Current;
        if (c.IsDefault) { return true; }

        var eff = GetEffectSpec(prefabSpec);
        if (eff is null) { return false; }

        return true;
    }

    List<ContinuousEffectSpec>? GetEffectSpec(BaseComponent comp)
    {
        var attraction = comp.GetComponentFast<AttractionSpec>();
        if (attraction) { return attraction._effects; }

        var cont = comp.GetComponentFast<ContinuousEffectBuildingSpec>();
        if (cont) { return cont._effects; }

        var wonder = comp.GetComponentFast<WonderEffectControllerSpec>();
        if (wonder) { return wonder._effects; }

        return null;
    }

}
