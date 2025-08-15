namespace TImprove4UX.Services;

public class UndoBuildingService(
    InputService inputService,
    DialogBoxShower diag,
    EntityService entityService,
    MSettings s,
    QuickNotificationService quickNotificationService,
    ILoc t
) : ILoadableSingleton, IInputProcessor, IUnloadableSingleton
{
    public static UndoBuildingService? Instance { get; private set; }

    const string HotkeyId = "UndoBuilding";

    FixedDeque<List<BlockObject>> lastBuilts = null!;

    public void Load()
    {
        Instance = this;
        inputService.AddInputProcessor(this);
        lastBuilts = new FixedDeque<List<BlockObject>>(s.UndoCount.Value);

        s.UndoCount.ValueChanged += UndoCount_ValueChanged;
    }

    private void UndoCount_ValueChanged(object sender, int e)
    {
        lastBuilts.Resize(e);
    }

    public void RegisterLastBuilt(List<BlockObject> buildings)
    {
        lastBuilts.Add(buildings);
    }

    public bool ProcessInput()
    {
        if (!inputService.IsKeyDown(HotkeyId)) { return false; }

        OnUndoRequested();
        return true;
    }

    void OnUndoRequested()
    {
        if (lastBuilts.Empty)
        {
            quickNotificationService.SendNotification(t.T("LV.T4UX.UndoEmpty"));
            return;
        }

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
        var lastBuilt = lastBuilts.PopStack();

        foreach (var b in lastBuilt)
        {
            if (!b) { continue; }
            entityService.Delete(b);
        }
    }

    bool HasProgressed()
    {
        var lastBuilt = lastBuilts.PeekStack();

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
