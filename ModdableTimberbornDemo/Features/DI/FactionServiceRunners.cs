namespace ModdableTimberbornDemo.Features.DI;

public class DemoFactionServiceRunner :
    ILoadableSingletonTailRunner<FactionSpecService>,
    ILoadableSingletonFrontRunner<FactionService>,
    ILoadableSingletonTailRunner<FactionService>,
    ILoadableSingleton
{

    HashSet<string>? ids;

    public DemoFactionServiceRunner(FactionSpecService _)
    {
        // Just for DI
    }

    public void TailRun(FactionSpecService service)
    {
        ids = [.. service.Factions.Select(q => q.Id)];
        Debug.Log($"[DemoFactionServiceRunner] Running after {nameof(FactionSpecService)} Load. Factions: {string.Join(", ", ids)}");
    }

    public void FrontRun(FactionService service)
    {
        Debug.Log($"[DemoFactionServiceRunner] Running before {nameof(FactionService)} Load. Current faction: {service.Current?.Id ?? "null"}. This message should appear after {nameof(FactionSpecService)} Load message. Faction Ids: {string.Join(", ", ids)}");
    }

    public void TailRun(FactionService service)
    {
        Debug.Log($"[DemoFactionServiceRunner] Running after {nameof(FactionService)} Load. Current faction: {service.Current?.Id ?? "null"}");
    }

    public void Load()
    {
        Debug.Log($"[DemoFactionServiceRunner] ILoadableSingleton.Load is called properly by the DI");
    }
}
