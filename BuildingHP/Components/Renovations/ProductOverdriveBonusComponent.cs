namespace BuildingHP.Components.Renovations;

public class ProductOverdriveBonusComponent : TogglableWorkplaceBonusComponent
{
    const string BonusId = "Renovation.ProductOverdriveBonus";
    static readonly BonusTrackerItem Default = new(BonusId, BonusType.WorkingSpeed, 0);

    BonusTrackerItem bonus = Default;
    protected override BonusTrackerItem Bonuses => bonus;

    public void Toggle(float? workingBonus)
    {
        Toggle(false);
        if (workingBonus is not null)
        {            
            bonus = new(BonusId, BonusType.WorkingSpeed, workingBonus.Value);
            Toggle(true);
        }
    }

}
