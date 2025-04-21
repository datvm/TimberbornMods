namespace MapResizer.UI;

public class MapResizerDialogTrigger(
    IOptionsBox optionsBox,
    ILoc t,
    MapSize mapSize,
    VisualElementInitializer veInit,
    PanelStack panelStack,
    MapResizerService resizer
) : ILoadableSingleton
{
    GameOptionsBox? gameOptionsBox = optionsBox as GameOptionsBox;
    public void Load()
    {
        if (gameOptionsBox is null) { return; }

        InsertResizeButton(gameOptionsBox._root);
    }

    void InsertResizeButton(VisualElement box)
    {
        var btn = box.AddMenuButton(t.T("LV.MRe.ResizeMap"), stretched: true);
        var btnResume = box.Q("ResumeButton");
        btn.InsertSelfAfter(btnResume);

        btn.clicked += OnShowResizeDialogClicked;
    }

    private void OnShowResizeDialogClicked()
    {
        if (gameOptionsBox is null) { return; }

        // Close the game options box
        gameOptionsBox.ResumeClicked(null);

        // Show the resize dialog
        var diag = new MapResizerDialog(mapSize, t);
        diag.Show(veInit, panelStack, () => resizer.PerformResize(diag.Size));
    }

}
