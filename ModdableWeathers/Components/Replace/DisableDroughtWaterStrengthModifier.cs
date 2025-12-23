
namespace ModdableWeathers.Components.Replace;

[HasPatch]
[BypassMethods([
    nameof(Awake),
    nameof(InitializeEntity),
    nameof(Enable),
    nameof(Disable),
])]
public class DisableDroughtWaterStrengthModifier() :
    DroughtWaterStrengthModifier(null, null, null, null, null, null, null)
{
}
