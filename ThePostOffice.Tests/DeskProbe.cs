namespace ThePostOffice.Tests;

sealed class DeskProbe
{
    public DeskProbe(OfficeWorld world)
    {
        Sender = new(world.Offices, world.Events);
        Receiver = new(world.Offices, world.Events);
        Set(Sender, "inventory", new PostOfficeInventory());
        Set(Sender, "spec", new PostOfficeSenderSpec { SendHours = 3f });
        Set(Receiver, "inventory", new PostOfficeInventory());
        Set(Receiver, "spec", new PostOfficeReceiverSpec
        {
            RetrieveHours = 3f,
            InventoryCapacity = 80,
        });
    }

    public PostOfficeSender Sender { get; }
    public PostOfficeReceiver Receiver { get; }

    public void StepSender() => Invoke(Sender, "Step");

    public void StepReceiver() => Invoke(Receiver, "Step");

    public void SeedSender(params PostOfficeParcel[] parcels) => Seed(Sender, parcels);

    public void SeedReceiver(params PostOfficeParcel[] parcels) => Seed(Receiver, parcels);

    public void MarkGoodsTaken()
    {
        Sender.GetType().GetProperty(nameof(PostOfficeSender.GoodsTaken))!.SetValue(Sender, true);
    }

    public void MarkStarted(object desk) => Set(desk, "started", true);

    public void SetHold(object desk, int? id) => Field(desk, "holdId").SetValue(desk, id);

    public void SetWorked(object desk, float hours)
    {
        desk.GetType().GetProperty("WorkedHours")!.SetValue(desk, hours);
    }

    static void Seed(object desk, PostOfficeParcel[] parcels)
    {
        var list = (List<PostOfficeParcel>)Field(desk, "parcels").GetValue(desk)!;
        list.Clear();
        list.AddRange(parcels);
    }

    static void Invoke(object desk, string name)
    {
        desk.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(desk, null);
    }

    static void Set(object desk, string name, object? value) => Field(desk, name).SetValue(desk, value);

    static FieldInfo Field(object desk, string name)
        => desk.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static void AssertGoods(PostOfficeParcel parcel, params (string Id, int Amount)[] expected)
    {
        Assert.Equal(expected.Length, parcel.Goods.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Id, parcel.Goods[i].GoodId);
            Assert.Equal(expected[i].Amount, parcel.Goods[i].Amount);
        }
    }
}
