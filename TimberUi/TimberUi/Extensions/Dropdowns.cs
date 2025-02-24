namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static Dropdown AddDropdown(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default)
    {
        return parent.AddChild<Dropdown>(name: name, classes: [.. additionalClasses ?? [],]);
    }

    public static Dropdown AddMenuDropdown(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default)
    {
        return parent.AddDropdown(name, additionalClasses: [.. additionalClasses ?? [], UiCssClasses.DropDownMenuClass]);
    }

    public static Dropdown AddChangeHandler(this Dropdown dropdown, Action<string?, int> handler)
    {
        dropdown.ValueChanged += (_, _) => handler(dropdown.GetSelectedValue(), dropdown.GetSelectedIndex());
        return dropdown;
    }

    public static IDropdownProvider SetItems<T>(this T dropdown, DropdownItemsSetter setter, IReadOnlyList<string> list, string? defaultValue = default) where T : Dropdown
    {
        var provider = new SimpleDropdownItemProvider(list, defaultValue);
        setter.SetItems(dropdown, provider);

        return provider;
    }

    public static Dropdown SetSelectedItem<T>(this T dropdown, string value) where T : Dropdown
    {
        dropdown._dropdownProvider?.SetValue(value);
        dropdown.RefreshContent();
        return dropdown;
    }

    public static Dropdown SetSelectedItem<T>(this T dropdown, int index) where T : Dropdown
    {
        var count = dropdown._dropdownProvider?.Items.Count ?? 0;
        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"The dropdown only has {count} items");
        }

        dropdown._dropdownProvider!.SetValue(dropdown._dropdownProvider.Items[index]);
        dropdown.RefreshContent();
        return dropdown;
    }

    public static string? GetSelectedValue<T>(this T dropdown) where T : Dropdown
    {
        return dropdown._dropdownProvider?.GetValue();
    }

    public static int GetSelectedIndex<T>(this T dropdown) where T : Dropdown
    {
        if (dropdown._dropdownProvider is null) { return -1; }

        return dropdown._dropdownProvider.Items.IndexOf(dropdown._dropdownProvider.GetValue());
    }

}