namespace TImprove4Ui.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolPanelDescriptionMover(MSettings s) : ILoadableSingleton
{

    public void Load()
    {
        s.toolDescPos.ValueChanged += ToolDescPos_ValueChanged;
    }

    void ToolDescPos_ValueChanged(object sender, string e)
    {
        DescriptionPanelPatches.ChangeToolPanelPosition(e);
    }
}
