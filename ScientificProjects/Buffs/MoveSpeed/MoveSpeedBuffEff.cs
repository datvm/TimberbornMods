namespace ScientificProjects.Buffs;

public class MoveSpeedBuffEff(float value, string name, ILoc t) : SimpleValueEffect<float>(value)
{
    protected override string? GetDescription(float value)
        => string.Format("LV.SP.MoveSpeedUpgradeBuffEff".T(t), value, name);

}
