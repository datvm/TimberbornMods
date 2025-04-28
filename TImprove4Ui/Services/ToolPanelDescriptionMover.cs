
namespace TImprove4Ui.Services;

public class ToolPanelDescriptionMover(MSettings s) : ILoadableSingleton
{

    public void Load()
    {
        s.toolDescPos.ValueChanged += ToolDescPos_ValueChanged;
    }

    private void ToolDescPos_ValueChanged(object sender, string e)
    {
        DescriptionPanelPatches.ChangeToolPanelPosition(e);
    }
}
