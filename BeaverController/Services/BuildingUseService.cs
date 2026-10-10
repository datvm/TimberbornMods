namespace BeaverController.Services;

[BindSingleton]
public class BuildingUseService(IGoodService goods, EventBus events, EntityRegistry entities)
{
    const float MaxValueTolerance = 0.001f;

    public bool IsCandidate(CharacterControllerComponent character, BaseComponent building)
    {
        if (!Approachable(building) || !Ready(character)) { return false; }

        var attraction = building.GetComponent<Attraction>();
        if (CanUseAttraction(character, attraction)) { return true; }

        return HasConsumable(character, building);
    }

    public bool IsFull(BaseComponent building)
    {
        var enterable = building.GetComponent<Enterable>();
        return enterable && enterable.Enabled && !enterable.CanReserveSlot;
    }

    public bool TryClaim(CharacterControllerComponent character, BaseComponent building, out BuildingUse use)
    {
        use = default;
        if (!IsCandidate(character, building)) { return false; }

        var enterable = building.GetComponent<Enterable>();
        var self = character.GetComponent<Enterer>();
        if (!enterable || !self) { return false; }
        if (!HoldsSlot(self, enterable) && !enterable.CanReserveSlot && !KickOne(enterable, self))
        {
            return false;
        }

        var attraction = building.GetComponent<Attraction>();
        if (CanUseAttraction(character, attraction))
        {
            use = new BuildingUse(enterable, false, null!, default);
            return true;
        }

        if (!TryBestGood(character, building, out var inventory, out var good)) { return false; }

        use = new BuildingUse(enterable, true, inventory, good);
        return true;
    }

    public bool TryBestGood(CharacterControllerComponent character, BaseComponent building, out Inventory inventory, out GoodAmount good)
    {
        inventory = null!;
        good = default;
        if (!Ready(character)) { return false; }

        var needs = character.GetComponent<NeedManager>();
        var best = 0f;
        foreach (var candidate in building.GetComponentsAllocating<Inventory>())
        {
            if (!candidate || !candidate.Enabled || !candidate.IsOutput) { continue; }

            foreach (var stock in candidate.UnreservedTakeableStock())
            {
                var value = Score(needs, goods.GetGood(stock.GoodId));
                if (value <= best) { continue; }

                best = value;
                inventory = candidate;
                good = new GoodAmount(stock.GoodId, 1);
            }
        }

        return best > 0f;
    }

    public bool WorthConsuming(CharacterControllerComponent character, GoodAmount good)
    {
        if (!Ready(character) || good.Amount <= 0) { return false; }

        var needs = character.GetComponent<NeedManager>();
        return Score(needs, goods.GetGood(good.GoodId)) > 0f;
    }

    public bool StayFinished(Attraction attraction, NeedManager needs, bool firstVisit)
    {
        if (firstVisit || !attraction.SatisfiesAnyNeedToMaxValue) { return !firstVisit; }

        foreach (var effect in attraction.Effects)
        {
            if (!effect.SatisfyToMaxValue || !needs.HasNeed(effect.NeedId)) { continue; }
            if (needs.NeedPointsToMax(effect.NeedId) > MaxValueTolerance) { return false; }
        }

        return true;
    }

    public void Consume(NeedManager needs, GoodReserver reserver)
    {
        var reserved = reserver.StockReservation;
        var amount = reserved.GoodAmount;
        reserver.UnreserveStock();
        reserved.Inventory.TakeConsumed(amount);

        foreach (var spec in goods.GetGood(amount.GoodId).ConsumptionEffects)
        {
            Apply(needs, spec, amount.Amount);
        }

        events.Post(new GoodConsumedEvent(amount.GoodId));
    }

    bool KickOne(Enterable enterable, Enterer self)
    {
        var inside = enterable.EnterersInside.FirstOrDefault(visitor => visitor && visitor != self);
        if (inside is Enterer kicked)
        {
            kicked.GetComponent<CharacterControllerComponent>().Stop();
            return enterable.CanReserveSlot;
        }

        foreach (var entity in entities.Entities)
        {
            var visitor = entity.GetComponent<Enterer>();
            if (!visitor || visitor == self || visitor._reservedBuilding != enterable) { continue; }

            visitor.GetComponent<CharacterControllerComponent>().Stop();
            return enterable.CanReserveSlot;
        }

        return false;
    }

    static bool HoldsSlot(Enterer self, Enterable enterable)
    {
        return self.CurrentBuilding == enterable || self._reservedBuilding == enterable;
    }

    static bool CanUseAttraction(CharacterControllerComponent character, Attraction attraction)
    {
        if (!attraction || !attraction.Enabled || !attraction.IsUsable) { return false; }

        var needs = character.GetComponent<NeedManager>();
        if (!needs) { return false; }

        foreach (var effect in attraction.Effects)
        {
            if (!needs.HasNeed(effect.NeedId)) { continue; }
            if (needs.GetNeed(effect.NeedId).Enabled) { return true; }
        }

        return false;
    }

    bool HasConsumable(CharacterControllerComponent character, BaseComponent building)
    {
        return TryBestGood(character, building, out _, out _);
    }

    static bool Approachable(BaseComponent building)
    {
        var block = building.GetComponent<BlockObject>();
        if (!block || !block.IsFinished) { return false; }

        var paused = building.GetComponent<PausableBuilding>();
        if (paused && paused.Paused) { return false; }

        var enterable = building.GetComponent<Enterable>();
        if (!enterable || !enterable.Enabled) { return false; }

        var accessible = building.GetComponent<BuildingAccessible>();
        return accessible && accessible.Accessible && accessible.Accessible.UnblockedSingleAccess.HasValue;
    }

    static bool Ready(CharacterControllerComponent character)
    {
        return character.GetComponent<NeedManager>()
            && character.GetComponent<GoodReserver>()
            && character.GetComponent<Enterer>()
            && character.GetComponent<WalkInsideExecutor>();
    }

    static float Score(NeedManager needs, GoodSpec good)
    {
        if (!good.HasConsumptionEffects) { return 0f; }

        var score = 0f;
        foreach (var spec in good.ConsumptionEffects)
        {
            if (!needs.HasNeed(spec.NeedId)) { continue; }

            var need = needs.GetNeed(spec.NeedId);
            if (!need.Enabled) { continue; }

            score += spec.Points;
        }

        return score;
    }

    static void Apply(NeedManager needs, InstantEffectSpec spec, int amount)
    {
        if (!needs.HasNeed(spec.NeedId)) { return; }

        var need = needs.GetNeed(spec.NeedId);
        if (!need.Enabled) { return; }

        var wasMinimum = need.IsAtMinimumPoints;
        var wasFavorable = need.IsFavorable;
        var wasCritical = need.IsInCriticalState;
        var wasActive = need.IsActive;
        need.SetPoints(need.Points + spec.Points * amount * need.NeedSpec.Effectiveness);
        need._appliedEffectSinceLastUpdate = true;
        needs.CheckNewState(need, wasMinimum, wasFavorable, wasCritical, wasActive);
    }
}

public readonly record struct BuildingUse(Enterable Enterable, bool ConsumesGood, Inventory Inventory, GoodAmount Good);
