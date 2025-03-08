namespace BuffDebuff;

[Obsolete($"Use {nameof(BeaverBuffTarget)} instead.")]
public class GlobalBeaverBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{
    protected override bool Filter(BuffableComponent buffable) => buffable.GetComponentFast<BeaverSpec>();
}

[Obsolete($"Use {nameof(AdultBeaverBuffTarget)} instead.")]
public class GlobalAdultBeaverBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{
    protected override bool Filter(BuffableComponent buffable) => buffable.GetComponentFast<AdultSpec>();
}

[Obsolete($"Use {nameof(ChildBeaverBuffTarget)} instead.")]
public class GlobalChildBeaverBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{
    protected override bool Filter(BuffableComponent buffable) => buffable.GetComponentFast<ChildSpec>();
}

[Obsolete($"Use {nameof(BotBuffTarget)} instead.")]
public class GlobalBotBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{
    protected override bool Filter(BuffableComponent buffable) => buffable.GetComponentFast<BotSpec>();
}
