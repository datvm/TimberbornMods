namespace ConveyorBelt.Algorithm;

public enum BeltForm
{
    Straight,
    Corner,
    RiserUp,
    RiserDown,
    Impermeable,
}

public readonly record struct BeltPort(BeltDir Direction, bool Incoming);

public static class BeltLayout
{
    public static void FillCarrier(BeltForm form, List<BeltPort> ports)
    {
        ports.Clear();
        switch (form)
        {
            case BeltForm.Corner:
                ports.Add(new(BeltDir.Up, true));
                ports.Add(new(BeltDir.Right, false));
                break;
            case BeltForm.RiserUp:
                ports.Add(new(BeltDir.Bottom, true));
                ports.Add(new(BeltDir.Top, false));
                break;
            case BeltForm.RiserDown:
                ports.Add(new(BeltDir.Top, true));
                ports.Add(new(BeltDir.Bottom, false));
                break;
            default:
                ports.Add(new(BeltDir.Up, true));
                ports.Add(new(BeltDir.Down, false));
                break;
        }
    }

    public static void FillLift(bool sendingOut, List<BeltPort> inputs, List<BeltPort> outputs)
    {
        inputs.Clear();
        outputs.Clear();
        if (sendingOut)
        {
            inputs.Add(new(BeltDir.Top, true));
            inputs.Add(new(BeltDir.Bottom, true));
            outputs.Add(new(BeltDir.Down, false));
            return;
        }

        inputs.Add(new(BeltDir.Down, true));
        outputs.Add(new(BeltDir.Top, false));
        outputs.Add(new(BeltDir.Bottom, false));
    }

    public static void FillMerger(List<BeltPort> inputs, List<BeltPort> outputs)
    {
        inputs.Clear();
        outputs.Clear();
        inputs.Add(new(BeltDir.Up, true));
        inputs.Add(new(BeltDir.Left, true));
        inputs.Add(new(BeltDir.Right, true));
        outputs.Add(new(BeltDir.Down, false));
    }

    public static void FillSplitter(List<BeltPort> inputs, List<BeltPort> outputs)
    {
        inputs.Clear();
        outputs.Clear();
        inputs.Add(new(BeltDir.Up, true));
        outputs.Add(new(BeltDir.Left, false));
        outputs.Add(new(BeltDir.Down, false));
        outputs.Add(new(BeltDir.Right, false));
    }
}
