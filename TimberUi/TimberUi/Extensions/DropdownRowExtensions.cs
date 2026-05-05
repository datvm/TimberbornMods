namespace UnityEngine.UIElements;

public static class DropdownRowExtensions
{

    extension(VisualElement parent)
    {

        public DropdownRow<TValue> AddDropdownRow<TValue>(
            string? label = null,
            Action<IndexedDropdownRowItem<TValue>>? onChange = default,
            VisualElementInitializer? veInit = default,
            DropdownItemsSetter? itemSetter = default)
        {
            var dropdown = parent.AddChild<DropdownRow<TValue>>();

            if (label is not null)
            {
                dropdown.SetLabel(label);
            }

            if (onChange is not null)
            {
                dropdown.OnValueChanged += (_, e) => onChange(e);
            }

            if (veInit is not null && itemSetter is not null)
            {
                dropdown.Initialize(veInit, itemSetter);
            }

            return dropdown;
        }

        public DropdownRow<TValue> AddDropdownRow<TValue>(
            IEnumerable<DropdownRowItem<TValue>> values,
            VisualElementInitializer veInit,
            DropdownItemsSetter itemSetter,
            string? label = null,
            Action<IndexedDropdownRowItem<TValue>>? onChange = default,
            bool generateUniqueNames = false
        )
        {
            var dropdown = parent.AddDropdownRow(label, onChange, veInit, itemSetter);

            dropdown.SetItems(values, generateUniqueNames);

            return dropdown;
        }

        public DropdownRow<TValue> AddDropdownRow<TValue>(
            IEnumerable<TValue> values,
            Func<TValue, string> textFn,
            VisualElementInitializer veInit,
            DropdownItemsSetter itemSetter,
            string? label = null,
            Action<IndexedDropdownRowItem<TValue>>? onChange = default,
            bool generateUniqueNames = false
        )
        {
            var dropdown = parent.AddDropdownRow(label, onChange, veInit, itemSetter);

            dropdown.SetItems(values, textFn, generateUniqueNames);

            return dropdown;
        }

    }

}
