namespace ConveyorBelt.Algorithm;

public enum BeltDir
{
    Down,
    Left,
    Up,
    Right,
    Bottom,
    Top,
}

public readonly record struct BeltCell(int X, int Y, int Z)
{
    public BeltCell Step(BeltDir dir) => dir switch
    {
        BeltDir.Down => new(X, Y - 1, Z),
        BeltDir.Left => new(X - 1, Y, Z),
        BeltDir.Up => new(X, Y + 1, Z),
        BeltDir.Right => new(X + 1, Y, Z),
        BeltDir.Bottom => new(X, Y, Z - 1),
        BeltDir.Top => new(X, Y, Z + 1),
        _ => this,
    };
}

public static class BeltDirs
{
    public static BeltDir Turn(BeltDir dir, int quarterTurns)
    {
        var turns = quarterTurns % 4;
        if (turns < 0)
        {
            turns += 4;
        }

        for (var i = 0; i < turns; i++)
        {
            dir = dir switch
            {
                BeltDir.Down => BeltDir.Left,
                BeltDir.Left => BeltDir.Up,
                BeltDir.Up => BeltDir.Right,
                BeltDir.Right => BeltDir.Down,
                _ => dir,
            };
        }

        return dir;
    }
}
