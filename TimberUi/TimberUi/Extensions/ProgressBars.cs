namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static TProgressBar AddProgressBar(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default)
    {
        additionalClasses = [UiCssClasses.ProgressBarClass, .. additionalClasses ?? []];

        var result = parent.AddChild<TProgressBar>(name: name, classes: additionalClasses);

        return result;
    }

    public static Label AddProgressLabel<T>(this T parent, string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default) where T : TProgressBar
    {
        var label = parent.AddGameLabel(text: text, name: name, additionalClasses: additionalClasses);
        label.text = text;
        return label;
    }

    public static T SetColor<T>(this T bar, ProgressBarColor color, bool removeColor = false) where T : TProgressBar
    {
        if (color == default) { return bar; }

        var className = UiCssClasses.ProgressBarClass + "--" + color switch
        {
            ProgressBarColor.Teal => UiCssClasses.Teal,
            ProgressBarColor.Red => UiCssClasses.Red,
            ProgressBarColor.Blue => UiCssClasses.Blue,
            _ => throw new NotImplementedException($"Unknown ProgressBarColor: {color}"),
        };

        if (removeColor)
        {
            bar.classList.Remove(className);
        }
        else
        {
            bar.AddClass(className);
        }

        return bar;
    }

    public static T SetProgress<T>(this T bar, float progress, Label? label, string? text = default) where T : TProgressBar
    {
        bar.SetProgress(progress);

        if (text is not null)
        {
            label ??= bar.Q<Label>();
            if (label is not null)
            {
                label.text = text;
            }
        }

        return bar;
    }

}

public enum ProgressBarColor
{
    Green = 0,
    Teal,
    Red,
    Blue
}