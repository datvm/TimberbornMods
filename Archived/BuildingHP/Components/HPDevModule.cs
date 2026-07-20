namespace BuildingHP.Components;

public class HPDevModule(EntitySelectionService entitySelectionService) : IDevModule
{
    public DevModuleDefinition GetDefinition()
    {
        ImmutableArray<int> Damages = [5, 10, 100];

        var builder = new DevModuleDefinition.Builder();

        foreach (var d in Damages)
        {
            builder.AddMethod(DevMethod.Create($"HP: Damage {d}", () => Damage(d)));
        }

        foreach (var d in Damages)
        {
            builder.AddMethod(DevMethod.Create($"HP: Heal {d}", () => Heal(d)));
        }

        builder.AddMethod(DevMethod.Create("HP: Set to 50% (ignore invulnerability)", () => SetHPTo(.5f)));
        builder.AddMethod(DevMethod.Create("HP: Set to 100%", () => SetHPTo(1f)));

        return builder.Build();
    }

    void Damage(int damage)
    {
        SelectingHP?.Damage(damage);
    }

    void Heal(int delta)
    {
        SelectingHP?.ChangeHP(delta);
    }

    void SetHPTo(float perc)
    {
        var hp = SelectingHP;
        if (!hp) { return; }

        var target = (int)(hp.Durability * perc);
        hp.SetHP(target, true);
    }

    BuildingHPComponent? SelectingHP
    {
        get
        {
            var obj = entitySelectionService.SelectedObject;
            if (!obj) { return null; }

            var hp = obj.GetHPComponent();
            return hp ? hp : null;
        }
    }
}
