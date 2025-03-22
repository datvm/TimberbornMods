global using Timberborn.CursorToolSystem;

namespace VerticalFarming;

public class ModCursorService : IUnloadableSingleton
{
    public static ModCursorService? Instance { get; private set; }

    public CursorCoordinatesPicker Cursor { get; private set; }

    public ModCursorService(CursorCoordinatesPicker cursor)
    {
        Cursor = cursor;

        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }
}
