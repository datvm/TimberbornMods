namespace UnityEngine.UIElements;

public readonly record struct MinMaxSliderValues(Vector2 Value, float Min, float Max);

public static partial class UiBuilderExtensions
{

    extension(VisualElement parent)
    {

        T InternalAddSlider<T, TSlider, TValue>(string? label = default, string? name = default, IEnumerable<string>? additionalClasses = default, in SliderValues<TValue>? values = default)
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

        public GameSlider AddSlider(string? label = default, string? name = default, IEnumerable<string>? additionalClasses = default, in SliderValues<float>? values = default) 
            => parent.InternalAddSlider<GameSlider, Slider, float>(label, name, additionalClasses, values);

        public GameSliderInt AddSliderInt(string? label = default, string? name = default, IEnumerable<string>? additionalClasses = default, in SliderValues<int>? values = default)
            => parent.InternalAddSlider<GameSliderInt, SliderInt, int>(label, name, additionalClasses, values);

        public MinMaxSlider AddMinMaxSlider(MinMaxSliderValues? values = default, Action<Vector2>? changeCallback = default, string? label = default, string? name = default, IEnumerable<string>? additionalClasses = default)
        {
            var slider = parent.AddChild<MinMaxSlider>(name, additionalClasses);

            if (label is not null)
            {
                slider.SetLabel(label);
            }

            if (values is not null)
            {
                slider.SetValue(values.Value, false);
            }

            if (changeCallback is not null)
            {
                slider.RegisterChangeCallback(changeCallback);
            }

            return slider;
        }
    }

    extension<T>(T slider) where T : MinMaxSlider
    {

        public T SetLabel(string label)
        {
            slider.label = label;
            return slider;
        }

        public T RegisterChangeCallback(Action<Vector2> callback)
        {
            slider.RegisterCallback<ChangeEvent<Vector2>>(ev => callback(ev.newValue));
            return slider;
        }

        public T SetValueWithoutNotify(Vector2 value) => slider.SetValue(value, notify: false);

        public T SetValue(in MinMaxSliderValues values, bool notify = true) => slider.SetValue(values.Value, notify, min: values.Min, max: values.Max);

        public T SetValue(Vector2? value = default, bool notify = true, float? min = default, float? max = default)
        {
            var s = slider;
            if (value is not null)
            {
                if (notify)
                {
                    s.value = value.Value;
                }
                else
                {
                    s.SetValueWithoutNotify(value.Value);
                }
            }

            if (min is not null)
            {
                s.lowLimit = min.Value;
            }

            if (max is not null)
            {
                s.highLimit = max.Value;
            }

            return slider;
        }

    }

}