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

        if (carrier is { Shape: BeltShape.LiftDown })
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
            if (!obj)
            {
                continue;
            }

            if (obj.GetComponent<BeltCarrierSpec>() is { Shape: BeltShape.LiftUp or BeltShape.LiftDown or BeltShape.RiserUp or BeltShape.RiserDown })
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
            if (!obj)
            {
                continue;
            }

            if (obj.GetComponent<BeltCarrierSpec>() is { Shape: BeltShape.LiftUp or BeltShape.RiserUp or BeltShape.RiserDown })
            {
                return true;
            }
        }

        return false;
    }
}
