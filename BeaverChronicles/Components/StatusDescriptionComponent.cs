namespace BeaverChronicles.Components;

[AddTemplateModule2(typeof(Character))]
[AddTemplateModule2(typeof(BlockObject), AlsoBindTransient = false)]
public class StatusDescriptionComponent : BaseComponent, IEntityMultiEffectsDescriber
{

    readonly Dictionary<string, IEntityStatus> statuses = [];

    public void AddStatus(IEntityStatus status) => statuses[status.Id] = status;
    public void RemoveStatus(string statusId) => statuses.Remove(statusId);

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
    {
        var day = dayNightCycle.PartialDayNumber;

        foreach (var s in statuses.Values)
        {
            yield return new(s.Title, s.Description, s.UntilDay - day);
        }
    }

}
