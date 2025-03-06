﻿namespace ScientificProjects.Buffs;

public class WorkEffBuffEff(float value, string name, ILoc t) : SimpleValueEffect<float>(value)
{

    protected override string? GetDescription(float value)
        => string.Format("LV.SP.WorkEffUpgradeBuffEff".T(t), value, name);

}
