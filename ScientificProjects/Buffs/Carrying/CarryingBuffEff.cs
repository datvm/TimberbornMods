namespace ScientificProjects.Buffs;

public class CarryingBuffEff(float value, string name, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value)
        => string.Format("LV.SP.CarryUpgradeBuffEff".T(t), value, name);
}
