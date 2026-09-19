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
}
