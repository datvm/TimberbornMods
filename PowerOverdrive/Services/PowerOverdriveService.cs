namespace PowerOverdrive.Services;

[BindSingleton]
public class PowerOverdriveService(
    ISpecService specs,
    GlobalInventoryService inventory,
    IAssetLoader assets,
    ILoc t
) : ILoadableSingleton
{
    const string EmptyButtonBgPath = "Sprites/PowerOverdrive/PowerOverdriverBg";

    public readonly ILoc t = t;

    public ImmutableArray<PowerOverdriveLevel> Levels { get; private set; } = [];
    public int LevelCount => Levels.Length;
    public Sprite EmptyButtonBackground { get; private set; } = null!;

    bool anyOverdriverMade;

    public void Load()
    {
        var spec = specs.GetSpecs<PowerOverdriveSpec>().SingleOrDefault()
            ?? throw new InvalidOperationException("PowerOverdriveSpec is missing.");

        if (spec.Levels.Length == 0)
        {
            throw new InvalidOperationException("PowerOverdriveSpec has no levels.");
        }

        List<PowerOverdriveLevel> levels = [];
        for (var i = 0; i < spec.Levels.Length; i++)
        {
            var levelSpec = spec.Levels[i];
            if (string.IsNullOrEmpty(levelSpec.ItemId))
            {
                throw new InvalidOperationException($"PowerOverdriveSpec level {i + 1} has no ItemId.");
            }

            levels.Add(new(
                i + 1,
                levelSpec.ItemId,
                levelSpec.ProductionBonus,
                levelSpec.PowerUsageBonus,
                inventory.Get(levelSpec.ItemId)));
        }

        Levels = [.. levels];
        EmptyButtonBackground = assets.Load<Sprite>(EmptyButtonBgPath);
    }

    public bool HasAnyOverdriverMade
    {
        get
        {
            if (anyOverdriverMade)
            {
                return true;
            }

            foreach (var level in Levels)
            {
                if (level.Good.IsVisible || level.Good.Amount > 0)
                {
                    anyOverdriverMade = true;
                    return true;
                }
            }

            return false;
        }
    }

    public bool ShouldShowFragment(PowerOverdriveComponent component)
        => component.Eligible && (HasAnyOverdriverMade || component.ActiveLevel > 0);

    public PowerOverdriveLevel GetLevel(int slotIndex) => Levels[slotIndex];

    public PowerOverdriveLevel BonusAt(int activeLevel) => Levels[activeLevel - 1];

    public string Percent(float bonus) => $"{bonus * 100:0}%";

    public string FormatEffect(int activeLevel)
    {
        if (activeLevel <= 0)
        {
            return t.T("LV.PO.NoEffect");
        }

        var bonus = BonusAt(activeLevel);
        return t.T("LV.PO.EffectDesc", Percent(bonus.ProductionBonus), Percent(bonus.PowerUsageBonus));
    }

    public Sprite? Icon(PowerOverdriveLevel level)
        => level.Good.Spec.IconSmall?.Value ?? level.Good.Spec.Icon?.Asset;

    public bool TrySetActiveLevel(PowerOverdriveComponent component, int level, out string? error)
    {
        error = null;
        if (level < 0 || level > LevelCount)
        {
            return false;
        }

        if (level == component.ActiveLevel)
        {
            return true;
        }

        if (!HasItems(component, level, out error))
        {
            return false;
        }

        Deactivate(component);
        for (var i = 0; i < level; i++)
        {
            Levels[i].Good.Remove(1);
        }

        component.SetActiveLevel(level);
        return true;
    }

    public void Deactivate(PowerOverdriveComponent component)
    {
        if (component.ActiveLevel <= 0)
        {
            return;
        }

        var active = component.ActiveLevel;
        component.SetActiveLevel(0);
        for (var i = 0; i < active; i++)
        {
            Levels[i].Good.Add(1);
        }
    }

    bool HasItems(PowerOverdriveComponent component, int level, out string? error)
    {
        List<string> missing = [];
        for (var i = 0; i < level; i++)
        {
            var entry = Levels[i];
            var amount = entry.Good.Amount + (i < component.ActiveLevel ? 1 : 0);
            if (amount < 1)
            {
                missing.Add(entry.Good.Spec.DisplayName.Value);
            }
        }

        if (missing.Count == 0)
        {
            error = null;
            return true;
        }

        error = t.T("LV.PO.NeedItems", string.Join(", ", missing));
        return false;
    }
}
