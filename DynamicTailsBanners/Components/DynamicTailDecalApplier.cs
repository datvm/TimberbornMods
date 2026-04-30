namespace DynamicTailsBanners.Components;

[AddTemplateModule2(typeof(TailDecalApplier))]
public class DynamicTailDecalApplier(DynamicDecalService service) : BaseComponent, IAwakableComponent
{

#nullable disable
    TailDecalApplier applier;
#nullable enable

    IDynamicTailDecalProvider? decalProvider;

    public void Awake()
    {
        applier = GetComponent<TailDecalApplier>();
    }

    public bool ShowTexture()
    {
        if (decalProvider is null) { return false; }

        var texture = decalProvider.GetTexture(this);
        applier._tailDecalTextureSetter.SetTexture(texture);
        return true;
    }

    public void OnDecalApplied(Decal decal)
    {
        if (decal.Id == decalProvider?.Id) { return; }

        decalProvider?.Unregister(this);

        decalProvider = service.GetTail(decal.Id);
        decalProvider?.Register(this);
    }

}
