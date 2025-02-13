using System;
using System.Text;
using UnityEngine.InputSystem;

namespace Timberborn.Common;

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
        var styles =  el.style as StyleValueCollection ?? throw new NotSupportedException();
        return string.Join(";", styles.m_Values.Select(q =>
            $"{q.id} {q.keyword} {q.number}  {q.color} {q.resource}"));
    }
}

