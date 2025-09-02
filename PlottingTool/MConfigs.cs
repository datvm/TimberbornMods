global using PlottingTool.Services;

namespace PlottingTool;

[Context("Game")]
[Context("MapEditor")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<ToolListener>().AsSingleton();
        Bind<PlotterService>().AsSingleton();
    }
}
