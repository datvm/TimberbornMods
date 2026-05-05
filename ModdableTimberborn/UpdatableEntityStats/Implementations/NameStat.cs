namespace ModdableTimberborn.UpdatableEntityStats.Implementations;

public class NameStat : ComponentUpdatableEntityStatBase<string, NamedEntity>
{
    public override string Id => "Name";

    protected override IEntityStatTracker<string> GetComponentTracker(UpdatableEntityStatComponent statComp, NamedEntity comp)
        => new NameTracker(statComp, comp);

}

public class NameTracker(UpdatableEntityStatComponent comp, NamedEntity namedEntity) : StatTrackerBase<string>(comp)
{

    protected override void OnStart()
    {
        namedEntity.EntityNameChanged += OnNameChanged;
    }

    protected override string CalculateValue() => namedEntity.EntityName;
    void OnNameChanged(object sender, EventArgs e) => UpdateValue();

    protected override void OnPause()
    {
        namedEntity.EntityNameChanged -= OnNameChanged;
    }

}