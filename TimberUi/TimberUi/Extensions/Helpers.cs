namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static T PrintVisualTree<T>(this T el) where T : VisualElement
    {
        var tree = DescribeVisualTree(el);
        Debug.Log(tree);

        return el;
    }

    public static string DescribeVisualTree<T>(this T el) where T : VisualElement
    {
        var builder = new StringBuilder();
        ScanVisualTree(el, 0, builder);

        return builder.ToString();
    }

    static void ScanVisualTree(VisualElement el, int depth, StringBuilder builder)
    {
        var indent = new string(' ', depth * 2);

        var classNames = string.Join(", ", el.GetClasses().Select(q => $"\"{q}\""));
        var styles = ExtractChangedElementStyles(el);

        builder.AppendLine($"{indent}{el.name}: {el.GetType()}, classes = [{classNames}], styles = \"{styles}\"");

        foreach (var child in el.Children())
        {
            ScanVisualTree(child, depth + 1, builder);
        }
    }

    static string ExtractChangedElementStyles(VisualElement el)
    {
        if (el.style is not StyleValueCollection styles) { return ""; }

        return string.Join(";", styles.m_Values.Select(q =>
            $"{q.id} {q.keyword} {q.number}  {q.color} {q.resource}"));
    }

}