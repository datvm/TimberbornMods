namespace TImprove4Ui.Services;

public class ShadowService(
    MSettings s,
    Sun sun
) : ILoadableSingleton
{
    LightShadows original;

    public void Load()
    {
        s.DisableShadows.ValueChanged += DisableShadows_ValueChanged;

        if (s.DisableShadows.Value)
        {
            DisableShadows_ValueChanged(this, true);
        }
    }

    void DisableShadows_ValueChanged(object _, bool disableShadow)
    {
        if (disableShadow)
        {
            if (original == default)
            {
                original = sun._sun.shadows;
            }
            sun._sun.shadows = LightShadows.None;
        }
        else
        {
            sun._sun.shadows = original;
        }
    }

}
