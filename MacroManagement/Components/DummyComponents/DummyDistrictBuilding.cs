namespace MacroManagement.Components.DummyComponents;

public class DummyDistrictBuilding : DistrictBuilding, IDummyComponent<DummyDistrictBuilding, DistrictBuilding>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public void Init(DistrictBuilding original)
    {
        District = original.District;
        InstantDistrict = original.InstantDistrict;
        ConstructionDistrict = original.ConstructionDistrict;
    }

}
