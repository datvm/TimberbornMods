namespace NarrativeEvents;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        Harmony harmony = new(nameof(NarrativeEvents));
        harmony.PatchAll();
    }

}
