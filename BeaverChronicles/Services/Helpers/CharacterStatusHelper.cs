namespace BeaverChronicles.Services.Helpers;

[BindSingleton]
public class CharacterStatusHelper(
    DefaultEntityTracker<Beaver> beavers,
    EntityRegistry entityRegistry
)
{
    public void BoostAllBeaversNeed(IReadOnlyCollection<string> ids, float? amount = null)
    {
        foreach (var b in beavers.Entities)
        {
            foreach (var id in ids)
            {
                BoostNeed(b.GetNeedManager(), id, amount);
            }
        }
    }

    public void BoostNeed(NeedManager needMan, string id, float? amount = null)
    {
        if (amount is null)
        {
            var n = needMan.GetNeed(id);
            if (n is null) { return; }

            amount = n.PointsToMax;
        }

        needMan.ApplyEffect(new(id, amount.Value, 1));
    }

    public void FindAndInflictRandomBeavers(string needId, int count)
    {
        var counter = 0;

        foreach (var (man, n) in FindUncontaminatedBeavers())
        {
            man.ApplyEffect(new(needId, -n.PointToMin, 1));
            counter++;

            if (counter >= count) { break; }
        }

        IEnumerable<(NeedManager, Need)> FindUncontaminatedBeavers()
        {
            foreach (var b in beavers.Entities)
            {
                var needMan = b.GetNeedManager();
                var n = needMan.GetNeed(needId);

                if (n is not null && n.Points == 0)
                {
                    yield return (needMan, n);
                }
            }
        }
    }

    public void FindAndContaminateRandomBeavers(int count) => FindAndInflictRandomBeavers(ChronicleGameEventHandler.ContaminationId, count);
    public void FindAndInjureRandomBeavers(int count) => FindAndInflictRandomBeavers(ChronicleGameEventHandler.InjuryId, count);

    public void CureContamination(Guid characterId) => RemoveNeed(characterId, ChronicleGameEventHandler.ContaminationId);

    public bool CharacterExists(Guid characterId) => entityRegistry.GetEntity(characterId);

    public bool IsContaminated(Guid characterId) => IsNeedActive(characterId, ChronicleGameEventHandler.ContaminationId);

    public bool IsNeedActive(Guid characterId, string needId)
    {
        var e = entityRegistry.GetEntity(characterId);
        if (!e) { return false; }

        var man = e.GetNeedManager();
        if (!man) { return false; }

        var n = man.GetNeed(needId);
        return n is not null && n.Points > 0;
    }

    public void RemoveNeed(Guid characterId, string needId)
    {
        var e = entityRegistry.GetEntity(characterId);
        if (!e) { return; }

        var man = e.GetNeedManager();
        if (!man) { return; }

        var n = man.GetNeed(needId);
        if (n is null) { return; }

        man.ApplyEffect(new(needId, -n.Points, 1));
    }

}
