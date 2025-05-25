namespace Hospital.Components;

public class HospitalComponent : BaseComponent
{

#nullable disable
    BlockObjectSpec blockObjectSpec;
    HospitalMaterialService hospitalMaterialService;
#nullable enable

    [Inject]
    public void Inject(HospitalMaterialService hospitalMaterialService)
    {
        this.hospitalMaterialService = hospitalMaterialService;
    }

    public void Awake()
    {
        blockObjectSpec = GetComponentFast<BlockObjectSpec>();
    }

    public void Start()
    {
        AttachHospitalIcon();
    }

    void AttachHospitalIcon()
    {
        var parent = TransformFast;
        var height = blockObjectSpec.BlocksSpec.Size.z;

        hospitalMaterialService.CreateIconPart(false, parent, height);
        hospitalMaterialService.CreateIconPart(true, parent, height);
    }

}
