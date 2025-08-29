namespace BuildingHP.UI;

public class BuildingHPFragmentMover(
#pragma warning disable CS9113 // Just for DI
    IEntityPanel _,
#pragma warning restore CS9113 // Parameter is unread.
    BuildingHPFragment buildingHPFragment
) : ILoadableSingleton
{

    public void Load()
    {
        var first = buildingHPFragment.Panel.parent.Children().First();
        buildingHPFragment.Panel.InsertSelfBefore(first);
    }

}
