namespace BeaverChronicles.Services.Buffs;

[BindSingleton]
public class CharacterEntityBuffService(
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle,
    DefaultEntityTracker<Beaver> beavers,
    DefaultEntityTracker<Bot> bots
) : EntityBuffService<CharacterBuffStatus>(loader, dayNightCycle)
{
    protected override string SaveId => nameof(CharacterEntityBuffService);

    public void AddOrUpdateCharacterBuff(CharacterBuffStatus buff, float? days) => AddOrUpdate(buff, days);
    public void RemoveCharacterBuff(string buffId) => Remove(buffId);

    protected override void OnLoaded()
    {
        beavers.OnEntityRegistered += ApplyBuffs;
        bots.OnEntityRegistered += ApplyBuffs;

        foreach (var beaver in beavers.Entities)
        {
            ApplyBuffs(beaver);
        }

        foreach (var bot in bots.Entities)
        {
            ApplyBuffs(bot);
        }
    }

    protected override void Apply(CharacterBuffStatus buff)
    {
        foreach (var entity in GetCharacters(buff.CharacterType))
        {
            ApplyBuff(entity, buff);
        }
    }

    protected override void RemoveBuff(CharacterBuffStatus buff)
    {
        foreach (var entity in GetCharacters(buff.CharacterType))
        {
            entity.GetBonusTracker().Remove(buff.Id);
            entity.GetStatusDescription().RemoveStatus(buff.Id);
        }
    }

    void ApplyBuffs(BaseComponent character)
    {
        var type = character.GetCharacterType();
        if (type == CharacterType.Unknown) { return; }

        foreach (var buff in Buffs.Values)
        {
            if (buff.CharacterType.HasFlag(type))
            {
                ApplyBuff(character, buff);
            }
        }
    }

    static void ApplyBuff(BaseComponent character, CharacterBuffStatus buff)
    {
        character.GetBonusTracker().AddOrUpdate(new(buff.Id, [.. buff.Effects.Select(b => b.ToBonusSpec())]));
        character.GetStatusDescription().AddStatus(buff);
    }

    IEnumerable<BaseComponent> GetCharacters(CharacterType types)
    {
        foreach (var beaver in beavers.Entities)
        {
            if (types.HasFlag(beaver.GetCharacterType()))
            {
                yield return beaver;
            }
        }

        if (types.HasFlag(CharacterType.Bot))
        {
            foreach (var bot in bots.Entities)
            {
                yield return bot;
            }
        }
    }
}
