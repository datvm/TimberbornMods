namespace ConveyorBelt.Services;

[MultiBind(typeof(IBlockObjectValidator))]
public class ConveyorPlacementValidator(ILoc t, IBlockService blocks) : IBlockObjectValidator
{
    public bool IsValid(BlockObject blockObject, out string errorMessage)
    {
        errorMessage = "";
        var carrier = blockObject.GetComponent<BeltCarrierSpec>();
        if (carrier is { Shape: BeltShape.RiserUp or BeltShape.RiserDown })
        {
            if (HasColumnBelow(blockObject.Coordinates))
            {
                return true;
            }

            errorMessage = t.T("LV.CBlt.RiserNeedsLift");
            return false;
        }

        if (blockObject.GetComponent<BeltTeleporterSpec>() is { Kind: TeleporterKind.LiftDown })
        {
            if (SupportsLiftDown(blockObject.Coordinates))
            {
                return true;
            }

            errorMessage = t.T("LV.CBlt.LiftDownNeedsColumn");
            return false;
        }

        return true;
    }

    bool HasColumnBelow(Vector3Int cell)
    {
        foreach (var obj in blocks.GetObjectsAt(cell + Direction3D.Bottom.ToOffset()))
        {
            if (obj.GetComponent<BeltTeleporterSpec>() is { Kind: TeleporterKind.LiftUp or TeleporterKind.LiftDown })
            {
                return true;
            }

            if (obj.GetComponent<BeltCarrierSpec>() is { Shape: BeltShape.RiserUp or BeltShape.RiserDown })
            {
                return true;
            }
        }

        return false;
    }

    bool SupportsLiftDown(Vector3Int cell)
    {
        foreach (var obj in blocks.GetObjectsAt(cell + Direction3D.Bottom.ToOffset()))
        {
            if (obj.GetComponent<BeltTeleporterSpec>() is { Kind: TeleporterKind.LiftUp })
            {
                return true;
            }

            if (obj.GetComponent<BeltCarrierSpec>() is { Shape: BeltShape.RiserUp or BeltShape.RiserDown })
            {
                return true;
            }
        }

        return false;
    }
}
