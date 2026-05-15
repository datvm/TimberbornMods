namespace MoreOverlay.Components;

[AddTemplateModule2(typeof(BlockObject))]
public class MoreOverlayComponent(MoreOverlayService service) : TickableComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity
{

    IMoreOverlayInstance[] instances = [];
    ITickableMoreOverlayInstance[] tickables = [];
    bool hasTickable;

    public int ColorPriority { get; set; }

    public bool HasOverlay => instances.Length > 0;

    VisualElement? panel;

    public void Awake()
    {
        DisableComponent();
    }

    public void InitializeEntity()
    {
        instances = [.. service.GetProviders(this)];
        if (instances.Length == 0) { return; }

        var (panel, content) = service.CreatePanel(this);
        this.panel = panel;
        foreach (var i in instances)
        {
            i.Initialize(content);
        }

        tickables = [.. instances.OfType<ITickableMoreOverlayInstance>()];
        hasTickable = tickables.Length > 0;

        if (panel is null) { return; }

        var center = GetComponent<BlockObjectCenter>();

        var worldCenter = center.WorldCenter;
        var worldCenterGrounded = center.WorldCenterGrounded;
        
        var y = (worldCenter.y + worldCenterGrounded.y) * 0.5f;
        Vector3 anchor = new(worldCenter.x, y, worldCenter.z);
        service.AddOverlay(panel, anchor);

        service.OnOverlayToggled += OnOverlayToggled;
        if (service.IsOverlayActive)
        {
            OnOverlayToggled(this, true);
        }
    }

    void OnOverlayToggled(object sender, bool e)
    {
        if (e)
        {
            if (hasTickable)
            {
                EnableComponent();
            }
            
            foreach (var i in instances)
            {
                i.OnShow();
            }
        }
        else
        {
            DisableComponent();
            ColorPriority = 0;
            foreach (var i in instances)
            {
                i.OnHide();
            }
        }
    }

    public void DeleteEntity()
    {
        if (panel is null) { return; }

        if (service.IsOverlayActive)
        {
            OnOverlayToggled(this, false);
        }

        service.RemoveOverlay(panel);
        foreach (var i in instances)
        {
            i.Remove();
        }

        service.OnOverlayToggled -= OnOverlayToggled;
    }

    public override void Tick()
    {
        foreach (var t in tickables)
        {
            t.OnTickUpdate();
        }
    }

}
