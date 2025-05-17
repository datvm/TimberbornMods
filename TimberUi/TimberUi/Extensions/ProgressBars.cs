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
        var className = UiCssClasses.ProgressBarClass + "--" + color switch
        {
            ProgressBarColor.Green => UiCssClasses.Green,
            ProgressBarColor.Teal => UiCssClasses.Teal,
            ProgressBarColor.Red => UiCssClasses.Red,
            ProgressBarColor.Blue => UiCssClasses.Blue,
            _ => throw new NotImplementedException($"Unknown ProgressBarColor: {color}"),
        };

        var progressBar = bar.Q("ProgressBar");

        if (removeColor)
        {
            bar.classList.Remove(className);
            progressBar.classList.Remove(className);
        }
        else
        {
            bar.classList.Add(className);
            progressBar.classList.Add(className);
        }

        return bar;
    }

    public static T SetProgress<T>(this T bar, float progress, Label? label, string? text = default) where T : TProgressBar
    {
        return bar.SetProgress(progress, label, text, default, default);
    }

    public static T SetProgress<T>(this T bar,
        float progress, Label? label = default, string? text = default,
        ProgressBarColor? oldColor = default, ProgressBarColor? newColor = default) where T : TProgressBar
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

        if (oldColor != newColor)
        {
            if (oldColor.HasValue)
            {
                bar.SetColor(oldColor.Value, true);
            }

            if (newColor.HasValue)
            {
                bar.SetColor(newColor.Value);
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