namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    extension<T>(T el) where T : VisualElement
    {
        public T Initialize(VisualElementLoader loader)
            => el.Initialize(loader._visualElementInitializer);

        public T Initialize(VisualElementInitializer initializer)
        {
            initializer.InitializeVisualElement(el);
            return el;
        }

        public T PrintVisualTree(bool printTemplates)
            => PrintVisualTree(el, options: UxmlExporter.ExportOptions.PrintTemplate);

        public T PrintVisualTree(string? templateId = default, UxmlExporter.ExportOptions options = default)
        {
            var tree = DescribeVisualTree(el, templateId, options);
            Debug.Log(tree);

            return el;
        }

        public T SetName(string name)
        {
            el.name = name;
            return el;
        }

        public string DescribeVisualTree(string? templateId = default, UxmlExporter.ExportOptions options = default)
        {
            templateId ??= el.fullTypeName;

            return UxmlExporter.Dump(el, templateId, options);
        }

        [Obsolete("Stylesheet methods are no longer supported.")]
        public T PrintStylesheet(UssExportOptions? options = default)
        {
            for (int i = 0; i < el.styleSheets.count; i++)
            {
                Debug.Log("Stylesheet " + i);
                el.styleSheets[i].Print();
            }

            return el;
        }

    }

    extension<T>(T stylesheet) where T : StyleSheet
    {

        [Obsolete("Stylesheet methods are no longer supported.")]
        public T Print(UssExportOptions? options = default)
        {
            var tree = Describe(stylesheet, options);
            Debug.Log(tree);

            return stylesheet;
        }

        [Obsolete("Stylesheet methods are no longer supported.")]
        public string Describe(UssExportOptions? options = default)
        {
            return StyleSheetToUss.ToUssString(stylesheet, options);
        }
    }


}