namespace ModdableWeathers.Services;

public class SoakEffectApplierService
{

    public bool ApplyingToOutsiders { get; private set; }
    public event Action<bool>? OnApplyingToOutsidersChanged;

    public void StartApplying()
    {
        ApplyingToOutsiders = true;
        OnApplyingToOutsidersChanged?.Invoke(ApplyingToOutsiders);
    }

    public void StopApplying()
    {
        ApplyingToOutsiders = false;
        OnApplyingToOutsidersChanged?.Invoke(ApplyingToOutsiders);
    }

}
