namespace System;

internal static class ModExtensions
{

    public static string ToTooltipString(this Priority priority) => priority switch
    {
        Priority.VeryLow => "--",
        Priority.Low => "-",
        Priority.Normal => "~",
        Priority.High => "+",
        Priority.VeryHigh => "++",
        _ => priority.ToString()
    };

    public static string Bold(this string input) => $"<b>{input}</b>";
    public static string Bigger(this string input) => $"<size=15>{input}</size>";
    public static string Indent(this string input) => $"  {input}";

}
