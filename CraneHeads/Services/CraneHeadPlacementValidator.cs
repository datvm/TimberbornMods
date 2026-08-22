namespace CraneHeads.Services;

[MultiBind(typeof(IBlockObjectValidator))]
public class CraneHeadPlacementValidator(
    ILoc t,
    CraneHeadStructureService heads
) : IBlockObjectValidator
{
    public bool IsValid(BlockObject blockObject, out string errorMessage)
    {
        var head = blockObject.GetComponent<CraneHeadComponent>();
        if (!head)
        {
            errorMessage = "";
            return true;
        }

        if (heads.HasCraneSectionBelow(head))
        {
            errorMessage = "";
            return true;
        }

        errorMessage = t.T("LV.CrH.MustPlaceHeadOnCraneSection");
        return false;
    }
}
