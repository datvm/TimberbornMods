namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static string Bold(this string input) => $"<b>{input}</b>";
    public static string Italic(this string input) => $"<i>{input}</i>";
    public static string Size(this string input, int size) => $"<size={size}>{input}</size>";
    public static string Color(this string input, string color) 
        => $"<color={(color.StartsWith('#') ? color : ('#' + color))}>{input}</color>";

    public static string Color(this string input, TimberbornTextColor color)
    {
        return input.Color(color switch
        {
            TimberbornTextColor.Green => "#59FF61",
            TimberbornTextColor.Red => "#FF4D4D",
            TimberbornTextColor.Solid => "#FFFF19",
            _ => "#000000",
        });
    }


}

public enum TimberbornTextColor
{
    Green,
    Red,
    Solid
}