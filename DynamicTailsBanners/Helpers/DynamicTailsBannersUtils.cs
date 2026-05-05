namespace DynamicTailsBanners.Services;

public static class DynamicTailsBannersUtils
{

    extension<T>(T comp) where T : BaseComponent
    {
        public void RefreshDecalTexture() => comp.GetComponent<DynamicBuildingDecal>().ShowTexture();
        public DynamicDecalOption GetDecalOptions() => comp.GetComponent<DynamicDecalOption>();
    }

}
