namespace GlobalInventory.Tests;

public class GlobalGoodHandleTests
{
    [Fact]
    public void AddAndClamp()
    {
        var handle = Create("Coin", min: 0, max: 10);

        Assert.Equal(5, handle.AddSafe(5));
        Assert.Equal(5, handle.Amount);
        Assert.Equal(5, handle.AddSafe(100));
        Assert.Equal(10, handle.Amount);
        Assert.Throws<InvalidOperationException>(() => handle.Add(1));
    }

    [Fact]
    public void RemoveClampsToMin()
    {
        var handle = Create("Coin", min: -2, max: 10, initial: 1);

        Assert.Equal(3, handle.RemoveSafe(3));
        Assert.Equal(-2, handle.Amount);
        Assert.Throws<InvalidOperationException>(() => handle.Remove(1));
    }

    [Fact]
    public void SetCapacityThrowVsSafe()
    {
        var handle = Create("Coin", min: 0, max: 10, initial: 8);

        Assert.Throws<InvalidOperationException>(() => handle.SetCapacity(0, 5));
        handle.SetCapacitySafe(0, 5);
        Assert.Equal(5, handle.Amount);
        Assert.Equal(5, handle.MaxCapacity);
    }

    [Fact]
    public void HideOnZero()
    {
        var handle = Create("Coin", min: 0, max: null, hideOnZero: true);
        Assert.False(handle.IsVisible);

        handle.Add(3);
        Assert.True(handle.IsVisible);

        handle.Remove(3);
        Assert.False(handle.IsVisible);
    }

    [Fact]
    public void HideOnlyAtZero()
    {
        var handle = Create("Coin", min: 0, max: null, initial: 4);
        Assert.True(handle.IsVisible);

        handle.Hide();
        Assert.True(handle.IsVisible);

        handle.Set(0);
        handle.Hide();
        Assert.False(handle.IsVisible);
    }

    [Fact]
    public void ZeroDeltaIsNoOp()
    {
        var handle = Create("Coin", min: 0, max: 10);
        handle.Add(0);
        Assert.False(handle.IsVisible);
        Assert.Equal(0, handle.Amount);
    }

    [Fact]
    public void UnlimitedMaxNeverFills()
    {
        var handle = Create("Coin", min: 0, max: null, initial: 4);
        Assert.Null(handle.FillRate);
        handle.Add(100);
        Assert.Equal(104, handle.Amount);
    }

    [Fact]
    public void ZeroMaxIsRealCap()
    {
        var handle = Create("Debt", min: -10, max: 0);
        handle.Remove(4);
        Assert.Equal(-4, handle.Amount);
        Assert.Null(handle.FillRate);
        Assert.Equal(4, handle.AddSafe(100));
        Assert.Equal(0, handle.Amount);
        Assert.Throws<InvalidOperationException>(() => handle.Add(1));
    }

    [Fact]
    public void StallWhenShortOrFull()
    {
        var gold = Create("Gold", min: 0, max: 10, initial: 1);
        var coin = Create("Coin", min: 0, max: 5, initial: 4);

        Assert.False(gold.Amount - 2 >= gold.MinCapacity);
        Assert.True(gold.Amount - 1 >= gold.MinCapacity);
        Assert.True(coin.Amount + 1 <= coin.MaxCapacity);
        Assert.False(coin.Amount + 2 <= coin.MaxCapacity);
    }

    static GlobalGoodHandle Create(
        string id,
        float min,
        float? max,
        float initial = 0,
        bool hideOnZero = false)
        => new(new GlobalGoodSpec
        {
            Id = id,
            DisplayName = new(id),
            MinCapacity = min,
            MaxCapacity = max,
            InitialAmount = initial,
            HideOnZero = hideOnZero,
        }, null);
}
