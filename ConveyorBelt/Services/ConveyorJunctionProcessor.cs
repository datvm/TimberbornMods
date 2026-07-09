namespace ConveyorBelt.Services;

[BindSingleton]
public class ConveyorJunctionProcessor(ConveyorBeltService beltService) : ITickableSingleton
{
    
    public void Tick()
    {
        var belts = beltService.Belts;
        if (belts.Count == 0) { return; }

        var junctions = beltService.Junctions;
        if (junctions.Count == 0) { return; }

        foreach (var (c, j) in junctions)
        {
            if (!j.CanUse) { continue; }

            // Check for output
            if (!belts.TryGetValue(j.OutputCoordinates, out var receiver)
                || receiver.InputCoordinates != c
                || !receiver.CanAcceptPotentialItem()) { continue; }

            foreach (var input in j.GetInputCoordinates())
            {
                if (!belts.TryGetValue(input, out var giver)
                    || giver.OutputCoordinates != c
                    || !giver.CanGiveItem) { continue; }

                var goodId = giver.Head!.GoodId;
                if (!receiver.IsValidGood(goodId)) { continue; }

                var item = giver.Pop();
                receiver.Push(item.GoodId);
                break;
            }
        }
    }

}
