namespace ThePostOffice.Tests;

public class DeskTests
{
    [Fact]
    public void SenderQueuesBelowMargin()
    {
        var desk = Desk(new Good("Log", 1, 7));
        desk.Sender.SetAuto(true, [new("Log", 10, 2)]);

        desk.StepSender();

        Assert.Single(desk.Sender.Parcels);
        Assert.True(desk.Sender.Parcels[0].Auto);
        DeskProbe.AssertGoods(desk.Sender.Parcels[0], ("Log", 3));
    }

    [Fact]
    public void SenderStaysQuietAboveTheMargin()
    {
        var desk = Desk(new Good("Log", 1, 3));
        desk.Sender.SetAuto(true, [new("Log", 10, 8)]);

        desk.StepSender();

        Assert.Empty(desk.Sender.Parcels);
    }

    [Fact]
    public void SenderAddsTheLookaheadInTheSamePass()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Sender.SetAuto(true, [new("Log", 100, 0)]);

        desk.StepSender();

        Assert.Equal(2, desk.Sender.Parcels.Count);
        DeskProbe.AssertGoods(desk.Sender.Parcels[0], ("Log", 60));
        DeskProbe.AssertGoods(desk.Sender.Parcels[1], ("Log", 40));
        Assert.All(desk.Sender.Parcels, parcel => Assert.True(parcel.Auto));
    }

    [Fact]
    public void SenderDropsTheLookaheadOnceTheMarginIsMet()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1));
        var desk = new DeskProbe(world);
        desk.Sender.SetAuto(true, [new("Log", 100, 0)]);
        desk.StepSender();
        world.Handle("Log").Add(50);

        desk.StepSender();

        Assert.Single(desk.Sender.Parcels);
        DeskProbe.AssertGoods(desk.Sender.Parcels[0], ("Log", 60));
    }

    [Fact]
    public void SenderKeepsTheActiveAutoUntilTarget()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1));
        var desk = new DeskProbe(world);
        desk.Sender.SetAuto(true, [new("Log", 10, 8)]);
        desk.StepSender();
        world.Handle("Log").Add(3);

        desk.StepSender();

        Assert.Single(desk.Sender.Parcels);
        DeskProbe.AssertGoods(desk.Sender.Parcels[0], ("Log", 10));
    }

    [Fact]
    public void SenderDropsTheActiveAutoAtTarget()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1));
        var desk = new DeskProbe(world);
        desk.Sender.SetAuto(true, [new("Log", 10, 0)]);
        desk.StepSender();
        world.Handle("Log").Add(10);

        desk.StepSender();

        Assert.Empty(desk.Sender.Parcels);
    }

    [Fact]
    public void SenderConfirmRepacksTheActiveAuto()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Sender.SetAuto(true, [new("Log", 10, 0)]);
        desk.StepSender();

        desk.Sender.SetAuto(true, [new("Log", 4, 0)]);
        desk.StepSender();

        Assert.Single(desk.Sender.Parcels);
        DeskProbe.AssertGoods(desk.Sender.Parcels[0], ("Log", 4));
    }

    [Fact]
    public void SenderLeavesATakenAutoAlone()
    {
        var desk = Desk(new Good("Log", 1));
        desk.SeedSender(new PostOfficeParcel([new("Log", 10)], true));
        desk.MarkGoodsTaken();

        desk.Sender.SetAuto(true, [new("Log", 4, 0)]);
        desk.StepSender();

        Assert.Single(desk.Sender.Parcels);
        DeskProbe.AssertGoods(desk.Sender.Parcels[0], ("Log", 10));
        Assert.True(desk.Sender.GoodsTaken);
    }

    [Fact]
    public void SenderCountsReservedGlobalStock()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1, 10));
        world.Inventory.TryReserveOutgoing([new("Log", 10)]);
        var desk = new DeskProbe(world);
        desk.Sender.SetAuto(true, [new("Log", 10, 0)]);

        desk.StepSender();

        Assert.Empty(desk.Sender.Parcels);
    }

    [Fact]
    public void SenderDoesNotReserveAnUngatheredParcel()
    {
        var desk = Desk(new Good("Log", 1, 20));
        desk.Sender.Enqueue([new("Log", 10)]);

        desk.StepSender();

        Assert.False(desk.Sender.HasHold);
        Assert.Single(desk.Sender.Parcels);
    }

    [Fact]
    public void ManualLandsBehindTheActiveAuto()
    {
        var desk = Desk(new Good("Log", 1), new Good("Plank", 1));
        desk.SeedSender(new PostOfficeParcel([new("Log", 4)], true), new PostOfficeParcel([new("Log", 4)], true));

        desk.Sender.Enqueue([new("Plank", 3)]);

        Assert.Equal(3, desk.Sender.Parcels.Count);
        Assert.True(desk.Sender.Parcels[0].Auto);
        Assert.False(desk.Sender.Parcels[1].Auto);
        DeskProbe.AssertGoods(desk.Sender.Parcels[1], ("Plank", 3));
        Assert.True(desk.Sender.Parcels[2].Auto);
    }

    [Fact]
    public void ManualLandsInFrontOfAQueuedAuto()
    {
        var desk = Desk(new Good("Log", 1), new Good("Plank", 1));
        desk.SeedSender(new PostOfficeParcel([new("Plank", 2)], false), new PostOfficeParcel([new("Log", 4)], true));

        desk.Sender.Enqueue([new("Plank", 3)]);

        Assert.False(desk.Sender.Parcels[0].Auto);
        Assert.False(desk.Sender.Parcels[1].Auto);
        Assert.True(desk.Sender.Parcels[2].Auto);
    }

    [Fact]
    public void CancelRemovesALaterManual()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Sender.Enqueue([new("Log", 70)]);

        desk.Sender.Cancel(1);

        Assert.Single(desk.Sender.Parcels);
        DeskProbe.AssertGoods(desk.Sender.Parcels[0], ("Log", 60));
    }

    [Fact]
    public void ActiveAutoCanBeCancelledOnlyWhenAutoIsOff()
    {
        var desk = Desk(new Good("Log", 1));
        desk.SeedSender(new PostOfficeParcel([new("Log", 4)], true));
        desk.Sender.SetAuto(true, [new("Log", 10, 0)]);

        Assert.False(desk.Sender.CanCancel(0));

        desk.Sender.SetAuto(false, [new("Log", 10, 0)]);

        Assert.True(desk.Sender.CanCancel(0));
        Assert.Single(desk.Sender.Parcels);
    }

    [Fact]
    public void SetAutoDropsInvalidRules()
    {
        var desk = Desk(new Good("Log", 1));

        desk.Sender.SetAuto(true, [new("Log", 0, 1), new("Missing", 5, 0), new("Log", 4, -1)]);

        Assert.False(desk.Sender.AutoEnabled);
        Assert.Empty(desk.Sender.Rules);
    }

    [Fact]
    public void SenderAcceptsTheGatheringParcel()
    {
        var desk = Desk(new Good("Log", 1), new Good("Plank", 1));
        desk.SeedSender(new PostOfficeParcel([new("Log", 4)], false), new PostOfficeParcel([new("Plank", 2)], false));
        Dictionary<string, int> accepted = [];

        desk.Sender.CopyAccepted(accepted);
        Assert.Equal(4, accepted["Log"]);

        accepted.Clear();
        desk.MarkGoodsTaken();
        desk.Sender.CopyAccepted(accepted);

        Assert.False(accepted.ContainsKey("Log"));
        Assert.Equal(2, accepted["Plank"]);
    }

    [Fact]
    public void ReleasedHoldCancelsTheSenderParcel()
    {
        var desk = Desk(new Good("Log", 1));
        desk.SeedSender(new PostOfficeParcel([new("Log", 4)], false));
        desk.MarkStarted(desk.Sender);
        desk.SetHold(desk.Sender, 7);
        desk.SetWorked(desk.Sender, 1.5f);

        desk.Sender.OnHoldsReleased(new([7]));

        Assert.False(desk.Sender.HasHold);
        Assert.Empty(desk.Sender.Parcels);
        Assert.Equal(0f, desk.Sender.WorkedHours);
    }

    [Fact]
    public void StalledHoldLeavesTheSenderParcel()
    {
        var desk = Desk(new Good("Log", 1));
        desk.SeedSender(new PostOfficeParcel([new("Log", 4)], false));
        desk.MarkStarted(desk.Sender);
        desk.SetHold(desk.Sender, 7);
        desk.SetWorked(desk.Sender, 1.5f);

        desk.Sender.OnHoldsStalled(new([7]));

        Assert.False(desk.Sender.HasHold);
        Assert.Single(desk.Sender.Parcels);
        Assert.Equal(1.5f, desk.Sender.WorkedHours);
    }

    [Fact]
    public void ReceiverIgnoresGlobalStock()
    {
        var desk = Desk(new Good("Log", 1, 100));
        desk.Receiver.SetAuto(true, [new("Log", 10, 0)]);

        desk.StepReceiver();

        Assert.Single(desk.Receiver.Parcels);
        DeskProbe.AssertGoods(desk.Receiver.Parcels[0], ("Log", 10));
    }

    [Fact]
    public void ReceiverQueuesOneAuto()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Receiver.SetAuto(true, [new("Log", 100, 0)]);

        desk.StepReceiver();
        desk.StepReceiver();

        Assert.Single(desk.Receiver.Parcels);
        DeskProbe.AssertGoods(desk.Receiver.Parcels[0], ("Log", 60));
    }

    [Fact]
    public void ReceiverPacksToTargetOnceUnderTheMargin()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Receiver.SetAuto(true, [new("Log", 10, 8)]);

        desk.StepReceiver();

        DeskProbe.AssertGoods(desk.Receiver.Parcels[0], ("Log", 10));
    }

    [Fact]
    public void ReceiverStaysQuietWhenTheMarginCoversZero()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Receiver.SetAuto(true, [new("Log", 10, 10)]);

        desk.StepReceiver();

        Assert.Empty(desk.Receiver.Parcels);
    }

    [Fact]
    public void ReceiverDoesNotShrinkAnExistingAuto()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Receiver.SetAuto(true, [new("Log", 10, 0)]);
        desk.StepReceiver();

        desk.Receiver.SetAuto(true, [new("Log", 4, 0)]);
        desk.StepReceiver();

        Assert.Single(desk.Receiver.Parcels);
        DeskProbe.AssertGoods(desk.Receiver.Parcels[0], ("Log", 10));
    }

    [Fact]
    public void ReceiverDropsAnAutoTheOtherParcelsAlreadyCover()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Receiver.SetAuto(true, [new("Log", 10, 0)]);
        desk.SeedReceiver(new PostOfficeParcel([new("Log", 10)], false), new PostOfficeParcel([new("Log", 10)], true));

        desk.StepReceiver();

        Assert.Single(desk.Receiver.Parcels);
        Assert.False(desk.Receiver.Parcels[0].Auto);
    }

    [Fact]
    public void ReceiverSetAutoRemovesTheQueuedAuto()
    {
        var desk = Desk(new Good("Log", 1));
        desk.SeedReceiver(new PostOfficeParcel([new("Log", 4)], false), new PostOfficeParcel([new("Log", 4)], true));

        desk.Receiver.SetAuto(true, [new("Log", 10, 0)]);

        Assert.Single(desk.Receiver.Parcels);
        Assert.False(desk.Receiver.Parcels[0].Auto);
    }

    [Fact]
    public void ReceiverReservesOnlyTheActiveParcel()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1, 100));
        var desk = new DeskProbe(world);
        desk.Receiver.Enqueue([new("Log", 100)]);

        desk.StepReceiver();

        Assert.True(desk.Receiver.HasHold);
        Assert.Equal(40, world.Inventory.Available("Log"));
        Assert.Equal(2, desk.Receiver.Parcels.Count);
    }

    [Fact]
    public void ReceiverKeepsTheParcelWhenTheReserveFails()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1, 50));
        var desk = new DeskProbe(world);
        desk.Receiver.Enqueue([new("Log", 60)]);

        desk.StepReceiver();

        Assert.False(desk.Receiver.HasHold);
        Assert.Single(desk.Receiver.Parcels);
        Assert.Equal(50, world.Inventory.Available("Log"));
    }

    [Fact]
    public void ReceiverCancelReleasesTheHold()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1, 10));
        var desk = new DeskProbe(world);
        desk.Receiver.Enqueue([new("Log", 4)]);
        desk.StepReceiver();

        desk.Receiver.Cancel(0);

        Assert.False(desk.Receiver.HasHold);
        Assert.Empty(desk.Receiver.Parcels);
        Assert.Equal(10, world.Inventory.Available("Log"));
    }

    [Fact]
    public void ReceiverCopiesNoAcceptedGoods()
    {
        var desk = Desk(new Good("Log", 1));
        desk.Receiver.SetAuto(true, [new("Log", 10, 0)]);
        Dictionary<string, int> accepted = [];

        desk.Receiver.CopyAccepted(accepted);

        Assert.Empty(accepted);
    }

    [Fact]
    public void OverweightParcelDoesNotWaitForRoom()
    {
        var desk = Desk(new Good("Metal", 90));
        desk.SeedReceiver(new PostOfficeParcel([new("Metal", 1)], false));

        Assert.False(desk.Receiver.NeedsRoom());
    }

    [Fact]
    public void ReleasedHoldCancelsTheReceiverParcel()
    {
        var desk = Desk(new Good("Log", 1));
        desk.SeedReceiver(new PostOfficeParcel([new("Log", 4)], false));
        desk.MarkStarted(desk.Receiver);
        desk.SetHold(desk.Receiver, 7);
        desk.SetWorked(desk.Receiver, 1.5f);

        desk.Receiver.OnHoldsReleased(new([7]));

        Assert.False(desk.Receiver.HasHold);
        Assert.Empty(desk.Receiver.Parcels);
        Assert.Equal(0f, desk.Receiver.WorkedHours);
    }

    [Fact]
    public void StalledHoldLeavesTheReceiverParcel()
    {
        var desk = Desk(new Good("Log", 1));
        desk.SeedReceiver(new PostOfficeParcel([new("Log", 4)], false));
        desk.MarkStarted(desk.Receiver);
        desk.SetHold(desk.Receiver, 7);
        desk.SetWorked(desk.Receiver, 1.5f);

        desk.Receiver.OnHoldsStalled(new([7]));

        Assert.False(desk.Receiver.HasHold);
        Assert.Single(desk.Receiver.Parcels);
        Assert.Equal(1.5f, desk.Receiver.WorkedHours);
    }

    static DeskProbe Desk(params Good[] goods) => new(OfficeWorld.Create(goods: goods));
}
