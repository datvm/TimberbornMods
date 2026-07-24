namespace MoreBuildingRenovations.Renovations;

[BindRenovation]
public class InventorExtraScience(ILoc t) : ExpirableRenovationBase
{
    const string BonusId = "Renovation_" + nameof(InventorExtraScience);

    public override string Id => nameof(InventorExtraScience);

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.HasComponent<InventorRenovationBonus>();

    /// <remarks>Duration is stored in hours; expiry tracking uses partial days (24h day).</remarks>
    public override float GetDurationDays(BuildingRenovationComponent building) => Spec.Parameters[0] / 24f;

    protected override void OnActivated(BuildingRenovationComponent building, bool isLoad)
    {
        base.OnActivated(building, isLoad);

        var desc = t.T("LV.MBR.InventorExtraScienceBuff", Spec.Parameters[1]);

        building.GetComponent<BonusDescriptionComponent>().AddBonus(new(
            BonusId,
            Spec.Title.Value,
            desc,
            _ => building.Expirable.TryGetRemainingDays(Id, out var days)
                ? days * 24f
                : null
        ));

        building.GetComponent<InventorRenovationBonus>().AddExtraScience((int)Spec.Parameters[1]);
    }

    public override void OnExpired(BuildingRenovationComponent building)
    {
        building.GetComponent<BonusDescriptionComponent>().RemoveBonus(BonusId);
        building.GetComponent<InventorRenovationBonus>().Deactivate();
        base.OnExpired(building);
    }
}
