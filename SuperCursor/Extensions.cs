namespace System;

static class ModExtensions
{

    public static string ToTooltipString(this Priority priority) => "P" + (int)priority;

    public static string Bold(this string input) => $"<b>{input}</b>";
    public static string Bigger(this string input) => $"<size=15>{input}</size>";
    public static string Indent(this string input) => $"  {input}";

}
