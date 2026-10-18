namespace ThePostOffice.Services;

[BindSingleton]
public class PostOfficeConfigService(ISpecService specs) : ILoadableSingleton
{
    PostOfficeConfigSpec spec = null!;

    public int ParcelWeight => spec.ParcelWeight;

    public void Load()
    {
        spec = specs.GetSingleSpec<PostOfficeConfigSpec>();
    }

}
