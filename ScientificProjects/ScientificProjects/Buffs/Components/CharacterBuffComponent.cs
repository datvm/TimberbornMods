﻿namespace ScientificProjects.Buffs;

public class CharacterBuffComponent : BaseComponent
{
    const string SpeedBonusId = "MovementSpeed";
    const string WorkEffBonusId = "WorkingSpeed";
    const string CarryingCapacityBonusId = "CarryingCapacity";

    BuffableComponent buffable = null!;
    BonusManager bonus = null!;
    OneTimeUnlockProcessor processor = null!;
    WalkerSpeedManager? walker;

    [Inject]
    public void Inject(OneTimeUnlockProcessor processor)
    {
        this.processor = processor;
    }

    public void Awake()
    {
        buffable = GetComponentFast<BuffableComponent>();
        bonus = GetComponentFast<BonusManager>();
        TryGetWalker();
    }

    WalkerSpeedManager? TryGetWalker()
    {
        if (this && !walker)
        {
            try
            {
                walker = GetComponentFast<WalkerSpeedManager>();
            }
            catch (Exception)
            {
                Debug.LogError($"Could not get {nameof(WalkerSpeedManager)} for Wheelbarrows");
            }
        }
        return walker;
    }

    public void Start()
    {
        if (processor.HasWheelbarrows) // Use this so not every character has to check for wheelbarrows
        {
            ActivateWheelbarrow();
        }
        else
        {
            processor.OnWheelbarrowsUnlocked += ActivateWheelbarrow;
        }

        buffable.OnBuffAdded += Buffable_OnBuffAdded;
        buffable.OnBuffRemoved += Buffable_OnBuffRemoved;

        // This is not used in this project but we process it anyway in case some other mods use it
        buffable.OnBuffActiveChanged += Buffable_OnBuffActiveChanged;
    }
    
    void ActivateWheelbarrow()
    {
        var walker = TryGetWalker();
        if (walker is null) { return; }

        var spec = walker._walkerSpeedManagerSpec;
        spec._baseSlowedSpeed = spec._baseWalkingSpeed;
    }

    private void Buffable_OnBuffAdded(object sender, BuffInstance e) => ProcessBuffAddRemove(e, false);
    private void Buffable_OnBuffRemoved(object sender, BuffInstance e) => ProcessBuffAddRemove(e, true);
    private void Buffable_OnBuffActiveChanged(object sender, BuffInstance e) => ProcessBuffAddRemove(e, !e.Active, ignoreActive: true);

    void ProcessBuffAddRemove(BuffInstance e, bool remove, bool ignoreActive = false)
    {
        if (!ignoreActive && !e.Active) { return; }

        switch (e)
        {
            case MoveSpeedUpgradeBuffInst ms:
                ApplySpeedBuff(ms, remove);
                break;
            case WorkEffUpgradeBuffInst we:
                ApplyWorkEffBuff(we, remove);
                break;
            case CarryingUpgradeBuffInst cc:
                ApplyCarryingCapacityBuff(cc, remove);
                break;
            case CarryingBuilderUpgradeBuffInst ccb:
                ApplyCarryingBuilderCapacityBuff(ccb, remove);
                break;
        }
    }

    void ApplySpeedBuff(MoveSpeedUpgradeBuffInst instance, bool negative) => ApplyBonusBuff<MoveSpeedBuffEff>(instance, SpeedBonusId, negative);
    void ApplyWorkEffBuff(WorkEffUpgradeBuffInst instance, bool negative) => ApplyBonusBuff<WorkEffBuffEff>(instance, WorkEffBonusId, negative);
    void ApplyCarryingCapacityBuff(CarryingUpgradeBuffInst instance, bool negative) => ApplyBonusBuff<CarryingBuffEff>(instance, CarryingCapacityBonusId, negative);
    void ApplyCarryingBuilderCapacityBuff(CarryingBuilderUpgradeBuffInst instance, bool negative) => ApplyBonusBuff<CarryingBuffEff>(instance, CarryingCapacityBonusId, negative);

    void ApplyBonusBuff<TEffect>(BuffInstance instance, string bonusId, bool negative)
        where TEffect : SimpleValueBuffEffect<float>
    {
        var effects = instance.GetBuffEffects<TEffect>(includeDisabled: true);
        foreach (var e in effects)
        {
            bonus.AddBonus(bonusId, e.Value * (negative ? -1 : 1));
        }
    }
}
