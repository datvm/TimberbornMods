namespace ConveyorBelt.Tests;

public class LayoutSimTests
{
    const float Tick = 24f / 768f;
    const float Fast = 100f;

    [Fact]
    public void StraightFeedsASink()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.Source, new(0, 1, 0), 0, Fast).GoodId = "Log";
        board.Place(SimKind.Straight, new(0, 0, 0), 0, Fast);
        var sink = board.Place(SimKind.Sink, new(0, -1, 0), 0, Fast);

        Run(board, 4);

        Assert.True(sink.CountOf("Log") > 0);
    }

    [Fact]
    public void CornerTurnsTheGood()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.Source, new(0, 1, 0), 0, Fast).GoodId = "Plank";
        board.Place(SimKind.Corner, new(0, 0, 0), 0, Fast);
        var sink = board.Place(SimKind.Sink, new(1, 0, 0), 0, Fast);

        Run(board, 4);

        Assert.True(sink.CountOf("Plank") > 0);
    }

    [Fact]
    public void RiserDropsToTheFloor()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.Source, new(0, 0, 2), 0, Fast).GoodId = "Gear";
        board.Place(SimKind.RiserDown, new(0, 0, 1), 0, Fast);
        var sink = board.Place(SimKind.Sink, new(0, 0, 0), 0, Fast);

        Run(board, 4);

        Assert.True(sink.CountOf("Gear") > 0);
    }

    [Fact]
    public void ExitThatFacesAwayDoesNotDeliver()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.Source, new(0, 1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(0, 0, 0), 0, Fast);
        var sink = board.Place(SimKind.Sink, new(1, 0, 0), 0, Fast);

        Run(board, 6);

        Assert.Equal(0, sink.Delivered);
    }

    [Fact]
    public void MergerTakesEveryInput()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.Merger, new(0, 0, 0), 0, Fast);
        board.Place(SimKind.Source, new(0, 2, 0), 0, Fast).GoodId = "Log";
        board.Place(SimKind.Straight, new(0, 1, 0), 0, Fast);
        board.Place(SimKind.Source, new(-2, 0, 0), 0, Fast).GoodId = "Plank";
        board.Place(SimKind.Straight, new(-1, 0, 0), 3, Fast);
        board.Place(SimKind.Source, new(2, 0, 0), 0, Fast).GoodId = "Gear";
        board.Place(SimKind.Straight, new(1, 0, 0), 1, Fast);
        board.Place(SimKind.Straight, new(0, -1, 0), 0, Fast);
        var sink = board.Place(SimKind.Sink, new(0, -2, 0), 0, Fast);

        Run(board, 12);

        Assert.True(sink.CountOf("Log") > 0);
        Assert.True(sink.CountOf("Plank") > 0);
        Assert.True(sink.CountOf("Gear") > 0);
    }

    [Fact]
    public void SplitterRoundRobins()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.Splitter, new(0, 0, 0), 0, Fast);
        board.Place(SimKind.Source, new(0, 2, 0), 0, Fast).GoodId = "Log";
        board.Place(SimKind.Straight, new(0, 1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(-1, 0, 0), 1, Fast);
        board.Place(SimKind.Straight, new(0, -1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(1, 0, 0), 3, Fast);
        var left = board.Place(SimKind.Sink, new(-2, 0, 0), 0, Fast);
        var down = board.Place(SimKind.Sink, new(0, -2, 0), 0, Fast);
        var right = board.Place(SimKind.Sink, new(2, 0, 0), 0, Fast);

        Run(board, 12);

        Assert.True(left.CountOf("Log") > 0);
        Assert.True(down.CountOf("Log") > 0);
        Assert.True(right.CountOf("Log") > 0);
    }

    [Fact]
    public void SmartSplitterDefaultsToTheMiddle()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.SmartSplitter, new(0, 0, 0), 0, Fast);
        board.Place(SimKind.Source, new(0, 2, 0), 0, Fast).GoodId = "Log";
        board.Place(SimKind.Straight, new(0, 1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(-1, 0, 0), 1, Fast);
        board.Place(SimKind.Straight, new(0, -1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(1, 0, 0), 3, Fast);
        var left = board.Place(SimKind.Sink, new(-2, 0, 0), 0, Fast);
        var down = board.Place(SimKind.Sink, new(0, -2, 0), 0, Fast);
        var right = board.Place(SimKind.Sink, new(2, 0, 0), 0, Fast);

        Run(board, 8);

        Assert.Equal(0, left.Delivered);
        Assert.True(down.CountOf("Log") > 0);
        Assert.Equal(0, right.Delivered);
    }

    [Fact]
    public void SmartSplitterPrefersTheNamedPort()
    {
        var board = new LayoutBoard();
        var splitter = board.Place(SimKind.SmartSplitter, new(0, 0, 0), 0, Fast);
        board.SetSplitPort(splitter.Id, 0, new(SmartSplitMode.Good, "Plank"));
        board.SetSplitPort(splitter.Id, 1, SmartSplitPort.AnyPort);
        board.SetSplitPort(splitter.Id, 2, new(SmartSplitMode.Overflow, null));
        board.Place(SimKind.Source, new(0, 2, 0), 0, Fast).GoodId = "Plank";
        board.Place(SimKind.Straight, new(0, 1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(-1, 0, 0), 1, Fast);
        board.Place(SimKind.Straight, new(0, -1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(1, 0, 0), 3, Fast);
        var left = board.Place(SimKind.Sink, new(-2, 0, 0), 0, Fast);
        var down = board.Place(SimKind.Sink, new(0, -2, 0), 0, Fast);
        var right = board.Place(SimKind.Sink, new(2, 0, 0), 0, Fast);

        Run(board, 8);

        Assert.True(left.CountOf("Plank") > 0);
        Assert.Equal(0, down.Delivered);
        Assert.Equal(0, right.Delivered);
    }

    [Fact]
    public void LiftInSendsUpAndDown()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.LiftIn, new(0, 0, 1), 0, Fast);
        board.Place(SimKind.Source, new(0, -2, 1), 0, Fast).GoodId = "Log";
        board.Place(SimKind.Straight, new(0, -1, 1), 2, Fast);
        board.Place(SimKind.RiserUp, new(0, 0, 2), 0, Fast);
        board.Place(SimKind.RiserDown, new(0, 0, 0), 0, Fast);
        var up = board.Place(SimKind.Sink, new(0, 0, 3), 0, Fast);
        var down = board.Place(SimKind.Sink, new(0, 0, -1), 0, Fast);

        Run(board, 12);

        Assert.True(up.CountOf("Log") > 0);
        Assert.True(down.CountOf("Log") > 0);
    }

    [Fact]
    public void LiftSendsFromBothShafts()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.LiftOut, new(0, 0, 1), 0, Fast);
        board.Place(SimKind.Source, new(0, 0, 3), 0, Fast).GoodId = "Top";
        board.Place(SimKind.RiserDown, new(0, 0, 2), 0, Fast);
        board.Place(SimKind.Source, new(0, 0, -1), 0, Fast).GoodId = "Bot";
        board.Place(SimKind.RiserUp, new(0, 0, 0), 0, Fast);
        board.Place(SimKind.Straight, new(0, -1, 1), 0, Fast);
        var sink = board.Place(SimKind.Sink, new(0, -2, 1), 0, Fast);

        Run(board, 12);

        Assert.True(sink.CountOf("Top") > 0);
        Assert.True(sink.CountOf("Bot") > 0);
    }

    [Fact]
    public void ChainedMergersDoNotPass()
    {
        var board = new LayoutBoard();
        const float speed = 8f;
        LayRow(board, 5, "Log", speed);
        LayRow(board, 4, "Plank", speed);
        LayRow(board, 3, "Test", speed);
        board.Place(SimKind.Straight, new(4, 2, 0), 0, speed);
        board.Place(SimKind.Straight, new(4, 1, 0), 0, speed);
        board.Place(SimKind.Straight, new(4, 0, 0), 0, speed);
        var sink = board.Place(SimKind.Sink, new(4, -1, 0), 0, speed);

        Run(board, 80);

        Assert.Equal(0, sink.CountOf("Log"));
        Assert.Equal(0, sink.CountOf("Plank"));
        Assert.True(sink.CountOf("Test") > 0);
    }

    [Fact]
    public void MergerFeedsABeltThenAnotherMerger()
    {
        var board = new LayoutBoard();
        board.Place(SimKind.Source, new(0, 3, 0), 0, Fast).GoodId = "Log";
        board.Place(SimKind.Straight, new(0, 2, 0), 0, Fast);
        board.Place(SimKind.Merger, new(0, 1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(0, 0, 0), 0, Fast);
        board.Place(SimKind.Merger, new(0, -1, 0), 0, Fast);
        board.Place(SimKind.Straight, new(0, -2, 0), 0, Fast);
        var sink = board.Place(SimKind.Sink, new(0, -3, 0), 0, Fast);

        Run(board, 8);

        Assert.True(sink.CountOf("Log") > 0);
    }

    static void LayRow(LayoutBoard board, int y, string good, float speed)
    {
        board.Place(SimKind.Source, new(0, y, 0), 0, speed).GoodId = good;
        board.Place(SimKind.Straight, new(1, y, 0), 3, speed);
        board.Place(SimKind.Straight, new(2, y, 0), 3, speed);
        board.Place(SimKind.Straight, new(3, y, 0), 3, speed);
        board.Place(SimKind.Merger, new(4, y, 0), 0, speed);
    }

    static void Run(LayoutBoard board, int ticks)
    {
        for (var i = 0; i < ticks; i++)
        {
            board.Tick(Tick);
        }
    }
}
