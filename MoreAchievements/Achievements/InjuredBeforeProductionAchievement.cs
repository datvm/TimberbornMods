
namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class InjuredBeforeProductionAchievement(DefaultEntityTracker<Manufactory> manufactoryTracker) : Achievement
{
    public const string InjuryId = "Injury";
    public static string AchId = "LV.MA.InjuredBeforeProduction";
    public override string Id => AchId;

    public override void EnableInternal()
    {
        manufactoryTracker.OnEntityRegistered += OnNewEntity;
    }

    public override void DisableInternal()
    {
        manufactoryTracker.OnEntityRegistered -= OnNewEntity;

        foreach (var e in manufactoryTracker.Entities)
        {
            e.ProductionFinished -= OnManufactoryProductionFinished;

            var applier = e.GetComponent<WorkshopRandomNeedApplier>();
            if (applier) { applier.NeedApplied -= OnNeedApplied; }
        }
    }

    void OnNewEntity(Manufactory man)
    {
        var applier = man.GetComponent<WorkshopRandomNeedApplier>();
        if (!applier) { return; }

        var produced = man.GetComponent<ManufactoryProducedComponent>().HasProduced;
        if (produced) { return; }

        man.ProductionFinished += OnManufactoryProductionFinished;
        applier.NeedApplied += OnNeedApplied;
    }

    void OnManufactoryProductionFinished(object sender, EventArgs e)
    {
        var applier = ((Manufactory) sender).GetComponent<WorkshopRandomNeedApplier>();
        applier.NeedApplied -= OnNeedApplied;
    }

    void OnNeedApplied(object sender, NeedAppliedEventArgs e)
    {
        if (e.NeedEffect.NeedId != InjuryId) { return; }
        if (!e.Character.GetCharacterType().IsBeaver()) { return; }

        var man = ((WorkshopRandomNeedApplier)sender).GetComponent<ManufactoryProducedComponent>();
        if (man.HasProduced) { return; }

        Unlock();
    }
}
