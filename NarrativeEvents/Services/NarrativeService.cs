global using Timberborn.TickSystem;

namespace NarrativeEvents.Services;

public class NarrativeService(DialogBoxShower diag) : ITickableSingleton
{

    static bool showed = false;

    public void Tick()
    {
        if (showed) { return; }
        showed = true;

        Debug.Log("Showing");
        diag.Create()
            .SetMessage("Hello")
            .SetConfirmButton(() => { }, "Option 1")
            .SetCancelButton(() => { }, "Option 2")
            .SetInfoButton(() => { }, "Option 3")
            .Show();
    }

}
