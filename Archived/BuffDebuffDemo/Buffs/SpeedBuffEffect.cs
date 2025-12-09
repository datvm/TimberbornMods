namespace BuffDebuffDemo.Buffs;

// This is an effect a buff should have.
// Use it however you want. In this project, we can reuse this one for all 3 buffs.
// In your game, you may have different effects to show up in the tooltip.
public class SpeedBuffEffect(ILoc t, float speed) : IBuffEffect
{
    // Show as percentage
    const string Format = "+#%;-#%;0%";

    // This will show up in the tooltip, something like:  Movement speed: +50%
    public string? Description { get; } = t.T("LV.BuffDebuffDemo.SpeedBuffEffectDesc", speed.ToString(Format));

    // For the other code to access and process the speed bonus
    public float Speed => speed;

    public long Id { get; set; }

    public void CleanUp() { }
    public void Init() { }
    public void UpdateEffect() { }

}
