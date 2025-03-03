global using Timberborn.AssetSystem;
using UnityEditor.UIElements.Debugger;

namespace TImprove4Modders.DevModules;

public class PrintUiModule(PanelStack panelStack, IAssetLoader loader) : IDevModule
{
    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("UI: Print current UI Uxml (to Log)", PrintStackPanelUxml))
            .AddMethod(DevMethod.Create("UI: Print current UI Uss (to Log)", PrintStackPanelStylesheet))
            .AddMethod(DevMethod.Create("UI: Export All UI/Views", ExportAllViews))
            .Build();
    }

    void ExportAllViews()
    {
        var assets = loader.LoadAll<VisualTreeAsset>("UI/Views");
        var folder = Path.Combine(ModStarter.ModPath, "ExportedViews");
        Directory.CreateDirectory(folder);

        foreach (var a in assets)
        {
            var ve = a.Asset.Instantiate();

            var path = Path.Combine(folder, a.Asset.name + ".uxml");
            var uxml = ve.DescribeVisualTree(options: 
                UxmlExporter.ExportOptions.PrintTemplate |
                UxmlExporter.ExportOptions.StyleFields |
                UxmlExporter.ExportOptions.AutoNameElements);
            File.WriteAllText(path, uxml);

            for (int i = 0; i < ve.styleSheets.count; i++)
            {
                var ss = ve.styleSheets[i];

                path = Path.Combine(folder, $"{ss.name}.uss");
                File.WriteAllText(path, ss.Describe());
            }
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(folder)
        {
            UseShellExecute = true,
        });
    }

    void PrintStackPanelUxml() => PrintUxml(panelStack._root);
    void PrintStackPanelStylesheet() => PrintUss(panelStack._root);

    void PrintUss(VisualElement el)
    {
        el.PrintStylesheet();
    }

    void PrintUxml(VisualElement el)
    {
        el.PrintVisualTree(true);
    }

}
