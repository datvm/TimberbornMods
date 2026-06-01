namespace ModdableTimberborn.Areas;

public class CharacterAreaTrackerHandle : AreaTrackerHandle<CharacterPositionTracker, CharacterAreaTrackerRegistration>
{
    public CharacterType CharacterTypes { get; }

    public CharacterAreaTrackerHandle(CharacterAreaTrackerRegistration registration, AreaSegmentService areaSegmentService) : base(registration, areaSegmentService)
    {
        CharacterTypes = registration.CharacterTypes;

        if (CharacterTypes == CharacterType.Unknown)
        {
            throw new ArgumentException("At least one character type must be specified.", nameof(registration));
        }
    }

    protected override bool IsEntityInAreas(CharacterPositionTracker entity)
    {
        if ((entity.CharacterType & CharacterTypes) == 0) { return false; }
        return Areas.FastAny(a => a.Contains(entity.Cell));
    }

}
