namespace ConveyorBelt.Algorithm;

public readonly record struct BeltGood(string Id, float Position, int Generation = 0, float Previous = 0f)
{
    public string Serialize() => $"{Id};{Position.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

    public static bool TryDeserialize(string text, out BeltGood good)
    {
        good = default;
        var split = text.IndexOf(';');
        if (split <= 0 || split == text.Length - 1)
        {
            return false;
        }

        if (!float.TryParse(text[(split + 1)..], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var position))
        {
            return false;
        }

        good = new(text[..split], position, Previous: position);
        return true;
    }
}
