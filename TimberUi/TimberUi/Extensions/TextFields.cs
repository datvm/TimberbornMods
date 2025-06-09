
namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static T AddValueField<T, TValueType>(this VisualElement parent, string? name = default, Action<TValueType>? changeCallback = default, IEnumerable<string>? additionalClasses = default)
        where T : BaseField<TValueType>, new()
    {
        var txt = parent.AddChild<T>(name, (additionalClasses ?? []).Prepend(UiCssClasses.TextFieldClass));

        if (changeCallback is not null)
        {
            txt.RegisterValueChangedCallback(ev =>
            {
                changeCallback(ev.newValue);
            });
        }

        return txt;
    }

    public static NineSliceTextField AddTextField(this VisualElement parent, string? name = default, Action<string>? changeCallback = default, IEnumerable<string>? additionalClasses = default)
        => parent.AddValueField<NineSliceTextField, string>(name, changeCallback, additionalClasses);

    public static NineSliceIntegerField AddIntField(this VisualElement parent, string? name = default, Action<int>? changeCallback = default, IEnumerable<string>? additionalClasses = default)
        => parent.AddValueField<NineSliceIntegerField, int>(name, changeCallback, additionalClasses);

    public static NineSliceFloatField AddFloatField(this VisualElement parent, string? name = default, Action<float>? changeCallback = default, IEnumerable<string>? additionalClasses = default) 
        => parent.AddValueField<NineSliceFloatField, float>(name, changeCallback, additionalClasses);

}