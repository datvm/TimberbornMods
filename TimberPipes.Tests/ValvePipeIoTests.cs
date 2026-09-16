namespace TimberPipes.Tests;

public class ValvePipeIoTests
{
    [Fact]
    public void InletPoursWhenEnabledAndTakes()
    {
        Assert.True(ValvePipeIo.ShouldInlet(
            enabled: true,
            paused: false,
            contaminated: false,
            volume: PipeFluids.PacketVolume,
            networkGoodId: "Water",
            takes: true,
            hasUnreservedCapacity: true));
    }

    [Fact]
    public void InletRefusesWhenDisabled()
    {
        Assert.False(ValvePipeIo.ShouldInlet(
            enabled: false,
            paused: false,
            contaminated: false,
            volume: 1f,
            networkGoodId: "Water",
            takes: true,
            hasUnreservedCapacity: true));
    }

    [Fact]
    public void InletRefusesFullOrReservedCapacity()
    {
        Assert.False(ValvePipeIo.CanGiveToBuilding(takes: true, hasUnreservedCapacity: false));
        Assert.False(ValvePipeIo.ShouldInlet(
            enabled: true,
            paused: false,
            contaminated: false,
            volume: 1f,
            networkGoodId: "Water",
            takes: true,
            hasUnreservedCapacity: false));
    }

    [Fact]
    public void OutletRefusesEmptyOrReservedStock()
    {
        Assert.False(ValvePipeIo.CanTakeFromBuilding(0));
        Assert.False(ValvePipeIo.ShouldOutlet(
            enabled: true,
            paused: false,
            contaminated: false,
            freeSpace: 1f,
            selectedGoodId: "Water",
            takeableAmount: 0));
    }

    [Fact]
    public void InletRefusesWhenTakesFails()
    {
        Assert.False(ValvePipeIo.ShouldInlet(
            enabled: true,
            paused: false,
            contaminated: false,
            volume: 1f,
            networkGoodId: "Water",
            takes: false,
            hasUnreservedCapacity: true));
    }

    [Fact]
    public void InletRefusesContaminatedOrLowVolume()
    {
        Assert.False(ValvePipeIo.CanInlet(true, false, contaminated: true, 1f, "Water"));
        Assert.False(ValvePipeIo.CanInlet(true, false, false, 0.19f, "Water"));
        Assert.False(ValvePipeIo.CanInlet(true, false, false, 1f, null));
    }

    [Fact]
    public void OutletPullsOnlySelectedGood()
    {
        Assert.True(ValvePipeIo.ShouldOutlet(
            enabled: true,
            paused: false,
            contaminated: false,
            freeSpace: PipeFluids.PacketVolume,
            selectedGoodId: "Water",
            takeableAmount: 4));
        Assert.False(ValvePipeIo.ShouldOutlet(
            enabled: true,
            paused: false,
            contaminated: false,
            freeSpace: 1f,
            selectedGoodId: "Water",
            takeableAmount: 0));
        Assert.False(ValvePipeIo.CanOutlet(true, false, false, 1f, selectedGoodId: null));
        Assert.False(ValvePipeIo.CanOutlet(true, false, false, 1f, selectedGoodId: "Badwater")
            && ValvePipeIo.CanTakeFromBuilding(0));
    }

    [Fact]
    public void FinishedBuildingWithInventoryIsCandidate()
    {
        Assert.True(ValvePipeIo.IsBuildingCandidate(finished: true, hasActiveInventory: true, isTransportPipe: false));
        Assert.False(ValvePipeIo.IsBuildingCandidate(finished: false, hasActiveInventory: true, isTransportPipe: false));
        Assert.False(ValvePipeIo.IsBuildingCandidate(finished: true, hasActiveInventory: false, isTransportPipe: false));
        Assert.False(ValvePipeIo.IsBuildingCandidate(finished: true, hasActiveInventory: true, isTransportPipe: true));
        Assert.False(ValvePipeIo.IsBuildingCandidate(
            finished: true,
            hasActiveInventory: true,
            isTransportPipe: false,
            isTank: true));
    }

    [Fact]
    public void WaterPumpWithoutOutletDoesNotFill()
    {
        Assert.False(ValvePipeIo.CanOutlet(
            enabled: false,
            paused: false,
            contaminated: false,
            freeSpace: 1f,
            selectedGoodId: "Water"));
    }

    [Fact]
    public void ValvePauseSealsIo()
    {
        Assert.False(ValvePipeIo.CanInlet(true, paused: true, false, 1f, "Water"));
        Assert.False(ValvePipeIo.CanOutlet(true, paused: true, false, 1f, "Water"));
    }

    [Fact]
    public void OutletTakesWhenPipeHasPartialSpace()
    {
        Assert.True(ValvePipeIo.ShouldOutlet(
            enabled: true,
            paused: false,
            contaminated: false,
            freeSpace: 0.05f,
            selectedGoodId: "Water",
            takeableAmount: 1));
        Assert.False(ValvePipeIo.ShouldOutlet(
            enabled: true,
            paused: false,
            contaminated: false,
            freeSpace: 0.05f,
            selectedGoodId: "Water",
            takeableAmount: 1,
            buffer: PipeFluids.PacketVolume));
        Assert.False(ValvePipeIo.CanOutlet(true, false, false, 0f, "Water"));
    }

    [Fact]
    public void ExtractBufferFillsRemainingSpace()
    {
        Assert.Equal(0.05f, ValvePipeIo.ExtractPourAmount(PipeFluids.PacketVolume, 0.05f));
        Assert.Equal(PipeFluids.PacketVolume, ValvePipeIo.ExtractPourAmount(PipeFluids.PacketVolume, 1f));
        Assert.Equal(0f, ValvePipeIo.ExtractPourAmount(0f, 1f));
        Assert.True(ValvePipeIo.CanPourExtractBuffer(false, 0.15f, 0.2f));
        Assert.False(ValvePipeIo.CanPourExtractBuffer(paused: true, 0.15f, 0.2f));
    }

    [Fact]
    public void FillOnlyWhenBuildingTakesLiquid()
    {
        HashSet<string> liquids = ["Water", "Badwater"];
        Assert.True(ValvePipeIo.HasLiquidInput(["Water"], liquids));
        Assert.False(ValvePipeIo.HasLiquidInput(["Log", "Plank"], liquids));
        Assert.False(ValvePipeIo.HasLiquidInput([], liquids));
    }

    [Fact]
    public void ExtractOnlyWhenBuildingGivesLiquid()
    {
        HashSet<string> liquids = ["Water", "Badwater"];
        Assert.Equal(["Badwater"], ValvePipeIo.KnownExtractLiquids(["Badwater"], [], liquids));
        Assert.Empty(ValvePipeIo.KnownExtractLiquids(["Log"], [], liquids));
        Assert.Equal(["Water"], ValvePipeIo.KnownExtractLiquids([], ["Water"], liquids));
    }

    [Fact]
    public void ExtractDropdownUsesKnownLiquidsOrFullList()
    {
        Assert.Equal(["Badwater"], ValvePipeIo.ExtractDropdownGoods(["Badwater"], ["Water", "Badwater"]));
        Assert.Equal(["Water", "Badwater"], ValvePipeIo.ExtractDropdownGoods([], ["Water", "Badwater"]));
    }

    [Fact]
    public void ExtractKeepsStoredGoodWhenBuildingChanges()
    {
        var ids = ValvePipeIo.ExtractDropdownGoods(["Badwater"], ["Water", "Badwater"], "Water");
        Assert.Contains("Water", ids);
        Assert.Contains("Badwater", ids);
        Assert.Equal("Water", ValvePipeIo.DefaultExtractGood("Water", ["Badwater"]));
        Assert.Equal("Badwater", ValvePipeIo.DefaultExtractGood(null, ["Badwater"]));
        Assert.Null(ValvePipeIo.DefaultExtractGood(null, []));
    }

    [Fact]
    public void HasKnownExtractLiquidFromOutputOrStock()
    {
        HashSet<string> liquids = ["Water", "Badwater"];
        Assert.True(ValvePipeIo.HasKnownExtractLiquid(["Badwater"], [], liquids));
        Assert.False(ValvePipeIo.HasKnownExtractLiquid(["Log"], [], liquids));
        Assert.True(ValvePipeIo.HasKnownExtractLiquid([], ["Water"], liquids));
    }

    [Fact]
    public void TargetStillValidRequiresFinishedInventory()
    {
        Assert.True(ValvePipeIo.TargetStillValid(true, true, 1));
        Assert.False(ValvePipeIo.TargetStillValid(false, true, 1));
        Assert.False(ValvePipeIo.TargetStillValid(true, false, 1));
        Assert.False(ValvePipeIo.TargetStillValid(true, true, 0));
    }

    [Fact]
    public void BuildingModelYawStraightWhenNeitherEnd()
    {
        Assert.Null(ValvePipeIo.BuildingModelYaw(false, false));
    }

    [Fact]
    public void BuildingModelYawZeroWhenUpEnd()
    {
        Assert.Equal(0f, ValvePipeIo.BuildingModelYaw(true, false));
    }

    [Fact]
    public void BuildingModelYaw180WhenDownEnd()
    {
        Assert.Equal(180f, ValvePipeIo.BuildingModelYaw(false, true));
    }

    [Fact]
    public void BuildingModelYawPrefersUpWhenBoth()
    {
        Assert.Equal(0f, ValvePipeIo.BuildingModelYaw(true, true));
    }

    [Fact]
    public void BuildingVisualNullWhenStraight()
    {
        Assert.Null(ValvePipeIo.BuildingVisual(false, false, true, false));
    }

    [Fact]
    public void BuildingVisualRingNearBendWhenOutputToBuilding()
    {
        var visual = ValvePipeIo.BuildingVisual(true, false, upIsOutput: true, downIsOutput: false);
        Assert.Equal(0f, visual?.Yaw);
        Assert.True(visual?.RingNearBend);
    }

    [Fact]
    public void BuildingVisualRingNearPipeWhenInputFromBuilding()
    {
        var visual = ValvePipeIo.BuildingVisual(false, true, upIsOutput: true, downIsOutput: false);
        Assert.Equal(180f, visual?.Yaw);
        Assert.False(visual?.RingNearBend);
    }

    [Fact]
    public void BuildingVisualFlipMovesRingWithOutput()
    {
        var visual = ValvePipeIo.BuildingVisual(false, true, upIsOutput: false, downIsOutput: true);
        Assert.Equal(180f, visual?.Yaw);
        Assert.True(visual?.RingNearBend);
    }
}
