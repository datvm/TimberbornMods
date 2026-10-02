namespace ConveyorBelt.Models;

public readonly record struct LocalPort(Direction3D Direction, bool Incoming);

public readonly record struct BeltEnd(Vector3Int Cell, Direction3D Direction)
{
    public Vector3Int Target => Cell + Direction.ToOffset();
}

public static class BeltLayout
{
    public static void FillCarrier(BeltShape shape, List<LocalPort> ports)
    {
        ports.Clear();
        switch (shape)
        {
            case BeltShape.Corner:
                ports.Add(new(Direction3D.Up, true));
                ports.Add(new(Direction3D.Right, false));
                break;
            case BeltShape.RiserUp:
                ports.Add(new(Direction3D.Bottom, true));
                ports.Add(new(Direction3D.Top, false));
                break;
            case BeltShape.RiserDown:
                ports.Add(new(Direction3D.Top, true));
                ports.Add(new(Direction3D.Bottom, false));
                break;
            default:
                ports.Add(new(Direction3D.Up, true));
                ports.Add(new(Direction3D.Down, false));
                break;
        }
    }

    public static void FillLift(bool sendingOut, List<LocalPort> inputs, List<LocalPort> outputs)
    {
        inputs.Clear();
        outputs.Clear();
        if (sendingOut)
        {
            inputs.Add(new(Direction3D.Top, true));
            inputs.Add(new(Direction3D.Bottom, true));
            outputs.Add(new(Direction3D.Down, false));
            return;
        }

        inputs.Add(new(Direction3D.Down, true));
        outputs.Add(new(Direction3D.Top, false));
        outputs.Add(new(Direction3D.Bottom, false));
    }

    public static void FillMerger(List<LocalPort> inputs, List<LocalPort> outputs)
    {
        inputs.Clear();
        outputs.Clear();
        inputs.Add(new(Direction3D.Up, true));
        inputs.Add(new(Direction3D.Left, true));
        inputs.Add(new(Direction3D.Right, true));
        outputs.Add(new(Direction3D.Down, false));
    }

    public static void FillSplitter(List<LocalPort> inputs, List<LocalPort> outputs)
    {
        inputs.Clear();
        outputs.Clear();
        inputs.Add(new(Direction3D.Up, true));
        outputs.Add(new(Direction3D.Left, false));
        outputs.Add(new(Direction3D.Down, false));
        outputs.Add(new(Direction3D.Right, false));
    }
}
