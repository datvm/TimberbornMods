namespace Crane.Services;

[MultiBind(typeof(IBlockObjectValidator))]
public class CraneSectionPlacementValidator(
    ILoc t,
    CraneStructureService structureService
) : IBlockObjectValidator
{
    public bool IsValid(BlockObject blockObject, out string errorMessage)
    {
        var section = blockObject.GetComponent<CraneSectionComponent>();
        if (!section)
        {
            errorMessage = "";
            return true;
        }

        if (structureService.HasCraneBelow(section))
        {
            errorMessage = "";
            return true;
        }

        errorMessage = t.T("LV.Cr.MustPlaceSectionOnCrane");
        return false;
    }
}
