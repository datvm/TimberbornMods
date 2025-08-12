namespace TImprove4UX.Services;

public class UndoBuildingService(
    InputService inputService,
    DialogBoxShower diag,
    EntityService entityService
) : ILoadableSingleton, IInputProcessor, IUnloadableSingleton
{
    public static UndoBuildingService? Instance { get; private set; }

    const string HotkeyId = "UndoBuilding";

    List<BlockObject> lastBuilt = [];

    public void Load()
    {
        Instance = this;
        inputService.AddInputProcessor(this);
    }

    public void RegisterLastBuilt(List<BlockObject> buildings)
    {
        lastBuilt = buildings;
    }

    public bool ProcessInput()
    {
        if (!inputService.IsKeyDown(HotkeyId)) { return false; }

        OnUndoRequested();
        return true;
    }

    void OnUndoRequested()
    {
        if (lastBuilt.Count == 0) { return; }

        if (HasProgressed())
        {
            diag.Create()
                .SetLocalizedMessage("LV.T4UX.ConfirmUndo")
                .SetConfirmButton(Undo)
                .SetDefaultCancelButton()
                .Show();
        }
        else
        {
            Undo();
        }
    }

    void Undo()
    {
        foreach (var b in lastBuilt)
        {
            if (!b) { continue; }
            entityService.Delete(b);
        }
    }

    bool HasProgressed()
    {
        foreach (var b in lastBuilt)
        {
            if (!b) { continue; }

            if (b.IsFinished) { return true; }

            var construction = b.GetComponentFast<ConstructionSite>();
            if (!construction) { continue; }

            if (construction.MaterialProgress > 0)
            {
                return true;
            }
        }

        return false;
    }

    public void Unload()
    {
        Instance = null;
    }
}
