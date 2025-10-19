namespace ColorfulZipline.Components;

public class ZiplineTowerColorComponent : BaseComponent, IPostInitializableEntity
{

#nullable disable
    public ZiplineTower ZiplineTower { get; private set; }
    ZiplineColoringService ziplineColoringService;
#nullable enable

    [Inject]
    public void Inject(ZiplineColoringService ziplineColoringService)
    {
        this.ziplineColoringService = ziplineColoringService;
    }

    public ZiplineCableColor GetColor(int index)
    {
        var target = ZiplineTower.ConnectionTargets[index];
        if (!target) { return ZiplineCableColor.Default; }

        return ziplineColoringService.GetColor(new(ZiplineTower, target));
    }

    public void SetColor(int index, ZiplineCableColor color)
    {
        var target = ZiplineTower.ConnectionTargets[index];
        if (!target)
        {
            throw new InvalidOperationException("Invalid target");
        }

        var key = CableKey.Create(ZiplineTower, target);
        ziplineColoringService.SetColor(key, color);
    }

    public void SetAllColor(ZiplineCableColor color)
    {
        var t1 = ZiplineTower;
        foreach (var t2 in ZiplineTower.ConnectionTargets)
        {
            if (!t2) { continue; }

            var pair = CableKey.Create(t1, t2);
            ziplineColoringService.SetColor(pair, color);
        }
    }

    public void Awake()
    {
        ZiplineTower = GetComponentFast<ZiplineTower>();
    }

    public void PostInitializeEntity()
    {
        var t1 = ZiplineTower;
        var targets = t1.ConnectionTargets;
        if (targets.Count == 0) { return; }

        foreach (var t2 in targets)
        {
            ziplineColoringService.ApplyColor(CableKey.Create(t1, t2));
        }
    }
}
