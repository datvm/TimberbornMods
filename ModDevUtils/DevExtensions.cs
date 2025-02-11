using System.Text;

namespace ModDevUtils;

public static class DevExtensions
{

    public static void PrintVisualTree(this VisualElement el)
    {
        var builder = new StringBuilder();
        ScanVisualTree(el, 0, builder);

        Debug.Log(builder.ToString());
    }

    static void ScanVisualTree(VisualElement el, int depth, StringBuilder builder)
    {
        var indent = new string(' ', depth * 2);
        builder.AppendLine($"{indent}{el.name} ({el.GetType()})");

        foreach (var child in el.Children())
        {
            ScanVisualTree(child, depth + 1, builder);
        }
    }

}
