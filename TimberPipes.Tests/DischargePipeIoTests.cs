namespace TimberPipes.Tests;

public class DischargePipeIoTests
{
    [Fact]
    public void WorldFluidsAreWaterAndBadwater()
    {
        Assert.True(DischargePipeIo.IsWorldFluid(PipeFluids.WaterGoodId));
        Assert.True(DischargePipeIo.IsWorldFluid(PipeFluids.BadwaterGoodId));
        Assert.False(DischargePipeIo.IsWorldFluid("Extract"));
        Assert.False(DischargePipeIo.IsWorldFluid("Biofuel"));
        Assert.False(DischargePipeIo.IsWorldFluid(null));
        Assert.True(DischargePipeIo.IsContaminatedWater(PipeFluids.BadwaterGoodId));
        Assert.False(DischargePipeIo.IsContaminatedWater(PipeFluids.WaterGoodId));
    }

    [Fact]
    public void OtherLiquidsContaminate()
    {
        Assert.True(DischargePipeIo.ShouldContaminate("Extract"));
        Assert.True(DischargePipeIo.ShouldContaminate("Biofuel"));
        Assert.False(DischargePipeIo.ShouldContaminate(PipeFluids.WaterGoodId));
        Assert.False(DischargePipeIo.ShouldContaminate(PipeFluids.BadwaterGoodId));
        Assert.False(DischargePipeIo.ShouldContaminate(null));
        Assert.False(DischargePipeIo.ShouldContaminate(PipeFluids.ContaminatedId));
        var cause = DischargePipeIo.WrongFluidCause("Extract");
        Assert.Equal("Extract", cause.GoodA);
        Assert.Equal(PipeFluids.WaterGoodId, cause.GoodB);
        Assert.True(cause.HasPair);
    }

    [Fact]
    public void SpaceUsesDistanceToGroundOffsetLikeDischarge()
    {
        // offset 0: stop one tile under the pipe (ejectZ - 0.1)
        Assert.Equal(-0.1f, DischargePipeIo.AvailableSpace(0, 0f, 0f), 4);
        Assert.Equal(0.4f, DischargePipeIo.AvailableSpace(0, -0.5f, 0f), 4);
        Assert.Equal(0f, DischargePipeIo.AvailableSpace(0, -0.1f, 0f), 4);
        Assert.Equal(0.9f, DischargePipeIo.AvailableSpace(2, 1.0f, 0f), 4);

        // offset 1: old "fill to top of eject tile" behaviour
        Assert.Equal(0.9f, DischargePipeIo.AvailableSpace(0, 0f, 1f), 4);
        Assert.Equal(0.2f, DischargePipeIo.AvailableSpace(0, 0.7f, 1f), 4);
        Assert.Equal(0f, DischargePipeIo.AvailableSpace(0, 0.9f, 1f), 4);
    }

    [Fact]
    public void EjectsUpToPacketOrRemainingSpace()
    {
        Assert.Equal(
            PipeFluids.PacketVolume,
            DischargePipeIo.EjectAmount(true, 1f, PipeFluids.WaterGoodId, 1f, 0.1f));
        Assert.Equal(
            0.07f,
            DischargePipeIo.EjectAmount(true, 1f, PipeFluids.BadwaterGoodId, 0.07f, 0.1f),
            4);
        Assert.Equal(
            0.15f,
            DischargePipeIo.EjectAmount(true, 0.15f, PipeFluids.WaterGoodId, 1f, 0.1f),
            4);
        Assert.Equal(0f, DischargePipeIo.EjectAmount(true, 1f, PipeFluids.WaterGoodId, 0f, 0.1f));
        Assert.Equal(0f, DischargePipeIo.EjectAmount(true, 1f, PipeFluids.WaterGoodId, -0.2f, 0.1f));
        Assert.Equal(0f, DischargePipeIo.EjectAmount(true, 1f, "Extract", 1f, 0.1f));
        Assert.Equal(0f, DischargePipeIo.EjectAmount(true, 0f, PipeFluids.WaterGoodId, 1f, 0.1f));
    }

    [Fact]
    public void BufferWaitsForHeadroomBeforeResuming()
    {
        // At/over limit: never dump.
        Assert.Equal(0f, DischargePipeIo.EjectAmount(false, 1f, PipeFluids.WaterGoodId, 0f, 0.1f));
        Assert.Equal(0f, DischargePipeIo.EjectAmount(true, 1f, PipeFluids.WaterGoodId, 0f, 0.1f));

        // Tiny room while idle: wait for EjectBuffer headroom (stops chatter around the limit).
        Assert.Equal(0f, DischargePipeIo.EjectAmount(false, 1f, PipeFluids.WaterGoodId, 0.05f, 0.1f));

        // Idle with enough headroom: start dumping.
        Assert.Equal(0.1f, DischargePipeIo.EjectAmount(false, 1f, PipeFluids.WaterGoodId, 0.1f, 0.1f), 4);
        Assert.Equal(
            PipeFluids.PacketVolume,
            DischargePipeIo.EjectAmount(false, 1f, PipeFluids.WaterGoodId, 0.5f, 0.1f));

        // Already dumping: keep going with whatever room is left until the limit.
        Assert.Equal(0.05f, DischargePipeIo.EjectAmount(true, 1f, PipeFluids.WaterGoodId, 0.05f, 0.1f), 4);
    }
}
