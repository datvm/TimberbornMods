namespace ThePostOffice.Tests;

public class SaveFormatTests
{
    [Fact]
    public void RoundTripParcel()
    {
        var line = PostOfficeSave.WriteParcel(new([new("Log", 4), new("Plank", 2)], true));

        var parcel = PostOfficeSave.ReadParcel(line);

        Assert.True(parcel.HasValue);
        Assert.True(parcel.Value.Auto);
        DeskProbe.AssertGoods(parcel.Value, ("Log", 4), ("Plank", 2));
    }

    [Fact]
    public void RoundTripRule()
    {
        var line = PostOfficeSave.WriteRule(new("Log", 10, 3));

        var rule = PostOfficeSave.ReadRule(line);

        Assert.Equal(new PostOfficeAutoRule("Log", 10, 3), rule);
    }

    [Fact]
    public void RejectBrokenLines()
    {
        Assert.False(PostOfficeSave.ReadParcel("1").HasValue);
        Assert.False(PostOfficeSave.ReadParcel("0|Log|0").HasValue);
        Assert.Null(PostOfficeSave.ReadRule("Log|0|1"));
        Assert.Null(PostOfficeSave.ReadRule("Log|4|-1"));
        Assert.Empty(PostOfficeSave.ReadGoods("Log"));
    }
}
