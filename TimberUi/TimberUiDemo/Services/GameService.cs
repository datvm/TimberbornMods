
namespace TimberUiDemo.Services;

public class GameService(PanelStack panelStack) : ILoadableSingleton
{

    public void Load()
    {
        panelStack._root.PrintVisualTree();
    }

}