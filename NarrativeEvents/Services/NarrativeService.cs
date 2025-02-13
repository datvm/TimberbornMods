global using Timberborn.TickSystem;

namespace NarrativeEvents.Services;

public class NarrativeService(DialogBoxShower diag) : ITickableSingleton
{

    static bool showed = false;

    public void Tick()
    {
        if (showed) { return; }
        showed = true;

    }

}
