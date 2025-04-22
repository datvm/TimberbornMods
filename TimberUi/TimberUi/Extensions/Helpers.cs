namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static T Initialize<T>(this T el, VisualElementLoader loader) where T : VisualElement
        => el.Initialize(loader._visualElementInitializer);

    public static T Initialize<T>(this T el, VisualElementInitializer initializer) where T : VisualElement
    {
        initializer.InitializeVisualElement(el);
        return el;
    }

    public static T PrintVisualTree<T>(this T el, bool printTemplates) where T : VisualElement
        => PrintVisualTree(el, options: UxmlExporter.ExportOptions.PrintTemplate);

    public static T PrintVisualTree<T>(this T el, string? templateId = default, UxmlExporter.ExportOptions options = default) where T : VisualElement
    {
        var tree = DescribeVisualTree(el, templateId, options);
        Debug.Log(tree);

        return el;
    }

    public static string DescribeVisualTree<T>(this T el, string? templateId = default, UxmlExporter.ExportOptions options = default) where T : VisualElement
    {
        templateId ??= el.fullTypeName;

        return UxmlExporter.Dump(el, templateId, options);
    }

    public static T PrintStylesheet<T>(this T el, UssExportOptions? options = default) where T : VisualElement
    {
        for (int i = 0; i < el.styleSheets.count; i++)
        {
            Debug.Log("Stylesheet " + i);
            el.styleSheets[i].Print();
        }

        return el;
    }

    public static T Print<T>(this T stylesheet, UssExportOptions? options = default) where T : StyleSheet
    {
        var tree = Describe(stylesheet, options);
        Debug.Log(tree);

        return stylesheet;
    }

    public static string Describe<T>(this T stylesheet, UssExportOptions? options = default) where T : StyleSheet
    {
        return StyleSheetToUss.ToUssString(stylesheet, options);
    }

}