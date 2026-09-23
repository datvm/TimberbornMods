namespace GlobalInventory.UI;

static class GlobalInventoryUi
{
    public static string Format(float value)
    {
        if (value == MathF.Truncate(value))
        {
            return ((long)value).ToString();
        }

        return value.ToString("0.##");
    }

    public static string FormatAmount(GlobalGoodHandle handle)
    {
        var amount = Format(handle.Amount);
        if (handle.MaxCapacity is float max && max != 0)
        {
            return $"{amount} / {Format(max)}";
        }

        return amount;
    }

    public static string Tooltip(GlobalGoodHandle handle, ILoc t)
    {
        var name = handle.Spec.PluralDisplayName.Value;
        if (string.IsNullOrEmpty(name))
        {
            name = handle.Spec.DisplayName.Value;
        }

        var text = name + "\n" + FormatAmount(handle);
        if (handle.MinCapacity != 0)
        {
            text += "\n" + t.T("LV.GI.Min", Format(handle.MinCapacity));
        }

        return text;
    }
}
