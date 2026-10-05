namespace ConveyorBelt.Tests;

public class SmartSplitPickerTests
{
    [Fact]
    public void AllAnyRoundRobins()
    {
        SmartSplitPort[] ports = [SmartSplitPort.AnyPort, SmartSplitPort.AnyPort, SmartSplitPort.AnyPort];
        var cursor = 0;

        Assert.Equal(0, SmartSplitPicker.Pick(ports, "Log", _ => true, ref cursor));
        Assert.Equal(1, SmartSplitPicker.Pick(ports, "Plank", _ => true, ref cursor));
        Assert.Equal(2, SmartSplitPicker.Pick(ports, "Gear", _ => true, ref cursor));
        Assert.Equal(0, SmartSplitPicker.Pick(ports, "Log", _ => true, ref cursor));
    }

    [Fact]
    public void NamedGoodBeatsAny()
    {
        SmartSplitPort[] ports =
        [
            SmartSplitPort.AnyPort,
            new(SmartSplitMode.Good, "Plank"),
            SmartSplitPort.AnyPort,
        ];
        var cursor = 0;

        Assert.Equal(1, SmartSplitPicker.Pick(ports, "Plank", _ => true, ref cursor));
        Assert.Equal(2, SmartSplitPicker.Pick(ports, "Log", _ => true, ref cursor));
        Assert.Equal(0, SmartSplitPicker.Pick(ports, "Log", _ => true, ref cursor));
    }

    [Fact]
    public void NoneIsSkipped()
    {
        SmartSplitPort[] ports = [.. SmartSplitPort.SmartDefaults];
        var cursor = 0;

        Assert.Equal(1, SmartSplitPicker.Pick(ports, "Log", _ => true, ref cursor));
        Assert.Equal(1, SmartSplitPicker.Pick(ports, "Plank", _ => true, ref cursor));
    }

    [Fact]
    public void MissingGoodBecomesNone()
    {
        var missing = new SmartSplitPort(SmartSplitMode.Good, "MyItem");
        var kept = new SmartSplitPort(SmartSplitMode.Good, "Log");

        Assert.Equal(SmartSplitPort.NonePort, missing.DropMissing(id => id == "Log"));
        Assert.Equal(kept, kept.DropMissing(id => id == "Log"));
        Assert.Equal(SmartSplitPort.AnyPort, SmartSplitPort.AnyPort.DropMissing(_ => false));
    }

    [Fact]
    public void NoneSerializesApartFromAny()
    {
        Assert.Equal("-", SmartSplitPort.NonePort.Serialize());
        Assert.Equal(SmartSplitPort.NonePort, SmartSplitPort.Deserialize("-"));
        Assert.Equal(SmartSplitPort.AnyPort, SmartSplitPort.Deserialize(""));
    }

    [Fact]
    public void OverflowRunsWhenTheOthersRefuse()
    {
        SmartSplitPort[] ports =
        [
            new(SmartSplitMode.Good, "Plank"),
            SmartSplitPort.AnyPort,
            new(SmartSplitMode.Overflow, null),
        ];
        var cursor = 0;

        Assert.Equal(2, SmartSplitPicker.Pick(ports, "Gear", index => index == 2, ref cursor));
    }
}
