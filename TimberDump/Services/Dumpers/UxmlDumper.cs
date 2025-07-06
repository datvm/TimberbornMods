namespace TimberDump.Services.Dumpers;

public class UxmlDumper(IAssetLoader assets) : IDumper
{
    public int Order { get; }
    public string? Folder { get; } = "UI";

    public void Dump(string folder)
    {
        var all = assets.LoadAll<VisualTreeAsset>("");

        foreach (var asset in all)
        {
            var veAsset = asset.Asset;
            if (!veAsset) { continue; }

            try
            {
                Debug.Log($"  Dumping {veAsset.name}");

                var ve = veAsset.Instantiate();

                var path = Path.Combine(folder, veAsset.name + ".uxml");
                var uxml = DescribeVisualTree(ve, options:
                    UxmlExporter.ExportOptions.PrintTemplate |
                    UxmlExporter.ExportOptions.StyleFields |
                    UxmlExporter.ExportOptions.AutoNameElements);
                File.WriteAllText(path, uxml);

                for (int i = 0; i < ve.styleSheets.count; i++)
                {
                    var ss = ve.styleSheets[i];

                    path = Path.Combine(folder, $"{ss.name}.uss");
                    File.WriteAllText(path, Describe(ss));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to export {veAsset.name}: {ex.Message}");
                Debug.LogWarning(ex);
            }
        }
    }

    public static string DescribeVisualTree<T>(T el, string? templateId = default, UxmlExporter.ExportOptions options = default) where T : VisualElement
    {
        templateId ??= el.fullTypeName;

        return UxmlExporter.Dump(el, templateId, options);
    }

    public static string Describe<T>(T stylesheet, UssExportOptions? options = default) where T : StyleSheet
    {
        return StyleSheetToUss.ToUssString(stylesheet, options);
    }
}
