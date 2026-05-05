namespace ModdableTimberborn.UpdatableEntityStats.Implementations;

public class AvatarStat(EntityBadgeService entityBadgeService) : UpdatableEntityStatBase<Sprite?>, IImageStat
{
    public override string Id => "Avatar";

    public override bool CanTrack(UpdatableEntityStatComponent? comp)
        => entityBadgeService.GetHighestPriorityEntityBadge(comp) is not null;

    public override bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker<Sprite?>? tracker)
    {
        if (!comp)
        {
            tracker = null;
            return false;
        }

        tracker = new AvatarStatTracker(comp!, entityBadgeService);
        return true;
    }
}

public class AvatarStatTracker(UpdatableEntityStatComponent comp, EntityBadgeService entityBadgeService) : StatTrackerBase<Sprite?>(comp), IImageStatTracker
{
    readonly Contaminable? contaminable = comp.GetComponent<Contaminable>();

    public override string ValueFormatted => Value?.name ?? "?";
    protected override Sprite? CalculateValue() => entityBadgeService.GetEntityAvatar(comp);

    protected override void OnPause()
    {
        if (contaminable)
        {
            contaminable!.ContaminationChanged -= OnContaminableChanged;
        }
    }

    protected override void OnStart()
    {
        if (contaminable)
        {
            contaminable!.ContaminationChanged += OnContaminableChanged;
        }
    }

    void OnContaminableChanged(object sender, EventArgs e) => UpdateValue();
}
