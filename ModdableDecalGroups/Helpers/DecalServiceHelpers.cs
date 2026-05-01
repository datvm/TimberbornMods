namespace Timberborn.DecalSystem
{
    public static class DecalServiceHelpers
    {

        extension(IDecalService decalService)
        {

            public DecalSpec GetSpec(Decal decal)
                => ((DecalService)decalService)._decalCategories[decal.Category]._categorySpecs[decal.Id];

        }

    }
}

namespace Timberborn.Localization
{
    public static class DecalServiceHelpers
    {
        extension(ILoc t)
        {

            public string TDecal(string type) => t.T("LV.MDG.Type." + type);

        }
    }
}