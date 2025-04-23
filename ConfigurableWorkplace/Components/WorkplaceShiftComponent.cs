global using Timberborn.TimeSystem;

namespace ConfigurableWorkplace.Components;

public class WorkplaceShiftComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("WorkplaceShift");
    static readonly PropertyKey<bool> EnabledKey = new("Enabled");
    static readonly PropertyKey<int> CustomShiftHourKey = new("CustomShiftHour");
    static readonly PropertyKey<Vector2Int> LunchBreakKey = new("LunchBreak");

    IDayNightCycle cycle = null!;
    WorkingHoursManager workingHoursManager = null!;

    public bool AreWorkingHours
    {
        get
        {
            if (!EnableCustomShift)
            {
                throw new InvalidOperationException($"Can only be called when {nameof(EnableCustomShift)} is true");
            }

            var hours = cycle.HoursPassedToday;
            if (LunchBreakStart != LunchBreakEnd && hours >= LunchBreakStart && hours < LunchBreakEnd)
            {
                return false;
            }

            return hours < CustomShiftHour;
        }
    }

    public bool EnableCustomShift { get; private set; }
    public int CustomShiftHour { get; set; }

    public int LunchBreakStart { get; set; } = 9;
    public int LunchBreakEnd { get; set; } = 9;

    public void SetEnabledCustomShift(bool enabled)
    {
        EnableCustomShift = enabled;

        if (CustomShiftHour == 0)
        {
            CustomShiftHour = Mathf.RoundToInt(workingHoursManager.EndHours);
        }
    }

    [Inject]
    public void Inject(IDayNightCycle cycle, WorkingHoursManager workingHoursManager)
    {
        this.workingHoursManager = workingHoursManager;
        this.cycle = cycle;
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);

        if (EnableCustomShift) { s.Set(EnabledKey, true); }
        s.Set(CustomShiftHourKey, CustomShiftHour);
        s.Set(LunchBreakKey, new(LunchBreakStart, LunchBreakEnd));
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        EnableCustomShift = s.Has(EnabledKey) && s.Get(EnabledKey);

        if (s.Has(CustomShiftHourKey))
        {
            CustomShiftHour = s.Get(CustomShiftHourKey);
        }

        if (s.Has(LunchBreakKey))
        {
            var lunchBreak = s.Get(LunchBreakKey);
            LunchBreakStart = lunchBreak.x;
            LunchBreakEnd = lunchBreak.y;
        }
    }

}
