namespace TimberUiDemo.UI;

public class PrintVisualTreeDevModule(PanelStack panelStack) : IDevModule
{
    
    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Print PanelStack root Visual Tree", PrintVisualTree))
            .Build();
    }

    void PrintVisualTree()
    {
        panelStack._root.PrintVisualTree();
    }

}
