namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    static T InternalAddSlider<T, TSlider, TValue>(this VisualElement parent, string? label = default, string? name = default, IEnumerable<string>? additionalClasses = default, in SliderValues<TValue>? values = default)
        where T : GameSlider<TSlider, TValue>, new()
        where TSlider : BaseSlider<TValue>, new()
        where TValue : IComparable<TValue>
    {
        var s = parent.AddChild<T>(name, additionalClasses);

        if (label is not null)
        {
            s.SetLabel(label);
        }

        if (values is not null)
        {
            s.SetHorizontalSlider(values.Value);
        }

        return s;
    }

    public static GameSlider AddSlider(this VisualElement parent, string? label = default, string? name = default, IEnumerable<string>? additionalClasses = default, in SliderValues<float>? values = default)
    {
        return InternalAddSlider<GameSlider, Slider, float>(parent, label, name, additionalClasses, values);
    }

    public static GameSliderInt AddSliderInt(this VisualElement parent, string? label = default, string? name = default, IEnumerable<string>? additionalClasses = default, in SliderValues<int>? values = default)
    {
        return InternalAddSlider<GameSliderInt, SliderInt, int>(parent, label, name, additionalClasses, values);
    }

}