namespace BuffDebuff;

public class GlobalBotBuffTarget(EventBus eventBus, BotPopulation botPops) : EntityBasedBuffTarget(eventBus)
{
    protected readonly BotPopulation botPops = botPops;

    protected override bool Filter(EntityComponent entity) => entity.GetComponentFast<BotSpec>();

    protected override HashSet<BuffableComponent> GetTargets() => [.. botPops._bots.Select(q => q.GetBuffable())];
}
