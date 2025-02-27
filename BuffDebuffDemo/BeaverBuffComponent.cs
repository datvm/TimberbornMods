global using Timberborn.BonusSystem;

namespace BuffDebuffDemo;

// This is a BaseComponent that will attach to the Beaver entities only (due to our declaration in the Config)
// This is the recommended way to process the buff instances compared to the commented out code in PositiveBuff
// because only entities that can receive the buff will have this component
public class BeaverBuffComponent : BaseComponent
{
    // We need the BonusManager to apply the bonus to the entity
    BonusManager bonusManager = null!;
    // We need the BuffableComponent to unregister the event
    BuffableComponent buffable = null!;

    // When the object is initialized, we grab the BuffableComponent and register the event
    public void Awake()
    {
        // We know each Beaver has a BonusManager attached (from the game code)
        bonusManager = GetComponentFast<BonusManager>();

        buffable = GetComponentFast<BuffableComponent>();
        // Register the event
        buffable.OnBuffAdded += Buffable_OnBuffAdded;
        buffable.OnBuffRemoved += Buffable_OnBuffRemoved;
        buffable.OnBuffActiveChanged += Buffable_OnBuffActiveChanged;
    }

    public void OnDestroy()
    {
        // Unregister the event
        buffable.OnBuffAdded -= Buffable_OnBuffAdded;
        buffable.OnBuffRemoved -= Buffable_OnBuffRemoved;
        buffable.OnBuffActiveChanged -= Buffable_OnBuffActiveChanged;
    }

    // Here we can narrow down to our own Instance, or we can just process indiscriminately for all buffs
    // as long as they have the SpeedBuffEffect
    private void Buffable_OnBuffAdded(object sender, BuffInstance e)
    {
        // We can process all the speed effect here
        if (!e.Active) { return; }

        var speedEffect = e.Effects.Where(q => q is SpeedBuffEffect).Cast<SpeedBuffEffect>();
        foreach (var effect in speedEffect)
        {
            bonusManager.AddBonus(Constants.MovementSpeedBonusId, effect.Speed);
        }
    }

    private void Buffable_OnBuffRemoved(object sender, BuffInstance e)
    {
        // Process similarly but to remove the bonus
        if (!e.Active) { return; }

        var speedEffect = e.Effects.Where(q => q is SpeedBuffEffect).Cast<SpeedBuffEffect>();
        foreach (var effect in speedEffect)
        {
            bonusManager.RemoveBonus(Constants.MovementSpeedBonusId, effect.Speed);
        }
    }

    // We know that for our case this one does not happen (we do not Activate/Deactivate our buff instances)
    // But if you do, you can process it here
    private void Buffable_OnBuffActiveChanged(object sender, BuffInstance e)
    {
        if (e.Active)
        {
            Buffable_OnBuffAdded(sender, e);
        }
        else
        {
            Buffable_OnBuffRemoved(sender, e);
        }
    }

}
