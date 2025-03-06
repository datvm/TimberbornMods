namespace ScientificProjects.Buffs;

public class MovementSpeedBuffEffect(float speed, string name, ILoc t) : IBuffEffect
{
    public float Speed { get; } = speed;

    public string? Description { get; } = string.Format("LV.SP.MoveSpeedUpgradeBuffEff".T(t), speed, name);
    public long Id { get; set; }

    public void CleanUp() { }
    public void Init() { }
    public void UpdateEffect() { }

}
