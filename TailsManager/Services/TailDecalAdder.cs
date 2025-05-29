using UnityEngine;

namespace TailsManager.Services;

public class TailDecalAdder(
    TailsManagerService tailsManagerService,
    TailDecalService tailDecalService,
    FactionService factions) : ILoadableSingleton
{

    public void Load()
    {
        AddDecals();
    }

    void AddDecals()
    {
        var subscribed = tailsManagerService
            .AllSubscribedTails;

        if (!tailsManagerService.IgnoreFaction)
        {
            var factionId = factions.Current.Id;

            subscribed = subscribed
                .Where(q => q.Factions is null || q.Factions.Value.Length == 0
                    || q.Factions.Value.Contains(factionId));
        }

        ConcurrentBag<TailDecalSpec> specs = [];
        Parallel.ForEach(subscribed, tail =>
        {
            var spec = new TailDecalSpec()
            {

                Texture = Texture2D.LoadImage()
            };
            specs.Add(spec);
        });

        foreach (var tail in specs)
        {
            tailDecalService._tailDecalSpecs.Add(tail.Id, tail);
        }

    }

}
