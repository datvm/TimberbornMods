namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    extension(string input)
    {
        public string Bold() => $"<b>{input}</b>";
        public string Italic() => $"<i>{input}</i>";
        public string Size(int size) => $"<size={size}>{input}</size>";
        public string Strikethrough() => $"<s>{input}</s>";
        public string Highlight() => input.Color(TimberbornTextColor.Yellow);

        public string Strikethrough(bool strikethrough)
            => strikethrough ? input.Strikethrough() : input;

        public string Color(string color)
            => $"<color={(color.StartsWith('#') ? color : ('#' + color))}>{input}</color>";

        public string Color(TimberbornTextColor color) => input.Color(color switch
        {
            TimberbornTextColor.Green => "#59FF61",
            TimberbornTextColor.Red => "#FF4D4D",
            TimberbornTextColor.Yellow => "#FFFF19",
            _ => "#000000",
        });
    }




}

public enum TimberbornTextColor
{
    Green,
    Red,
    [Obsolete($"Use {nameof(Yellow)} instead.")]
    Solid = 2,
    Yellow = 2,
}