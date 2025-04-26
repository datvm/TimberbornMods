namespace TheArchitectsToolkit.Services.Copier;

public class CopierController(
    MapSize mapSize,
    ILoc t,
    VisualElementInitializer veInit,
    PanelStack panelStack,
    BlockObjectCopierService blockObjectCopierService,
    IOptionsBox optionsBox
) : ILoadableSingleton
{
    readonly MapEditorOptionsBox mapEditorOptionsBox = (MapEditorOptionsBox)optionsBox;

    public void Load()
    {
        InsertCopyButton();
    }

    void InsertCopyButton()
    {
        var root = mapEditorOptionsBox._root;
        var btnResume = root.Q("Resume");

        var btnCopy = root.AddMenuButton("LV.TAT.CopyObjects".T(t), onClick: ShowCopyDialog, stretched: true);
        btnCopy.InsertSelfAfter(btnResume);
    }

    public void ShowCopyDialog()
    {
        var diag = new CopierDialog(t, mapSize.TerrainSize.XY() / 2);

        diag.Show(veInit, panelStack,
            () => PerformCopy(diag.Size));
    }

    void PerformCopy(in Vector2Int originalSize)
    {
        blockObjectCopierService.Copy(in originalSize);
    }

}
