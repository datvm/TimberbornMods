namespace HousePainter.Components;

[AddTemplateModule2(typeof(BuildingModelSpec))]
public class BuildingPartPainter(
    BuildingPaintService paintService
) : BaseComponent, IInitializableEntity, IPersistentEntity, IDeletableEntity, IDuplicable<BuildingPartPainter>
{
    static readonly ComponentKey SaveKey = new(nameof(BuildingPartPainter));
    static readonly PropertyKey<bool> IsPaintingEnabledKey = new("IsPaintingEnabled");
    static readonly PropertyKey<string> PaintedPartsKey = new("PaintedParts");

    public BuildingModel Model { get; private set; } = null!;

    public ImmutableArray<PaintPart> PaintableParts { get; private set; } = [];
    public bool IsPaintingEnabled { get; private set; }
    bool pendingPaintingLoad;

    readonly Dictionary<string, PaintedPart> paintedParts = [];
    public IReadOnlyDictionary<string, PaintedPart> PaintedParts => paintedParts;

    public string TemplateName { get; private set; } = "";

    public void InitializeEntity()
    {
        TemplateName = this.GetTemplateName();

        Model = GetComponent<BuildingModel>();
        TryApplyPendingLoad();
    }

    public void RequestPainting()
    {
        if (IsPaintingEnabled)
        {
            return;
        }

        var parts = paintService.EnablePainting(Model);
        if (parts.Length == 0)
        {
            return;
        }

        PaintableParts = parts;
        IsPaintingEnabled = true;

        foreach (var painted in paintedParts.Values)
        {
            if (painted.Color is not { } color)
            {
                continue;
            }

            if (paintService.TrySetPartColor(Model, painted.MaterialName, color))
            {
                PaintableParts = UpdatePaintableColor(PaintableParts, painted.MaterialName, color);
            }
        }
    }

    public void DisablePainting()
    {
        if (!IsPaintingEnabled)
        {
            return;
        }

        paintService.DisablePainting(Model);
        IsPaintingEnabled = false;
        PaintableParts = [];
        // Keep paintedParts so re-enable can restore colors.
        pendingPaintingLoad = false;
    }

    public void ApplyPaint(string name, Color color)
    {
        if (!paintedParts.TryGetValue(name, out var part))
        {
            paintedParts[name] = part = new PaintedPart(name);
        }

        part.Color = color;

        if (!IsPaintingEnabled)
        {
            return;
        }

        if (paintService.TrySetPartColor(Model, name, color))
        {
            PaintableParts = UpdatePaintableColor(PaintableParts, name, color);
        }
    }

    public void SetLabel(string name, string label)
    {
        if (!paintedParts.TryGetValue(name, out var part))
        {
            paintedParts[name] = part = new PaintedPart(name);
        }

        part.Label = label;
    }

    public void ClearPaint(string name)
    {
        if (paintedParts.TryGetValue(name, out var part))
        {
            if (part.Label is null)
            {
                paintedParts.Remove(name);
            }
            else
            {
                part.Color = null;
            }
        }

        if (!IsPaintingEnabled)
        {
            return;
        }

        if (paintService.TrySetPartColor(Model, name, color: null))
        {
            PaintableParts = UpdatePaintableColor(PaintableParts, name, Color.white);
        }
    }

    public void ToggleHighlightPart(string name, bool highlight)
    {
        if (!IsPaintingEnabled)
        {
            return;
        }

        paintService.TrySetPartHighlight(Model, name, highlight);
    }

    public void ClearPartHighlights()
    {
        if (!IsPaintingEnabled)
        {
            return;
        }

        paintService.ClearPartHighlights(Model);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!IsPaintingEnabled && paintedParts.Count == 0)
        {
            return;
        }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(IsPaintingEnabledKey, IsPaintingEnabled || pendingPaintingLoad);
        s.Set(PaintedPartsKey, JsonConvert.SerializeObject(paintedParts.Values));
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        pendingPaintingLoad = s.Get(IsPaintingEnabledKey);

        var paintedPartsJson = s.Get(PaintedPartsKey);
        var parts = JsonConvert.DeserializeObject<IEnumerable<PaintedPart>>(paintedPartsJson);
        if (parts is null)
        {
            return;
        }

        foreach (var part in parts)
        {
            paintedParts[part.MaterialName] = part;
        }
    }

    public void DeleteEntity()
    {
        if (!IsPaintingEnabled)
        {
            return;
        }

        paintService.DisablePainting(Model);
        IsPaintingEnabled = false;
        PaintableParts = [];
    }

    void TryApplyPendingLoad()
    {
        if (!pendingPaintingLoad || paintedParts.Count == 0)
        {
            return;
        }

        RequestPainting();
        if (IsPaintingEnabled)
        {
            pendingPaintingLoad = false;
        }
    }

    static ImmutableArray<PaintPart> UpdatePaintableColor(
        ImmutableArray<PaintPart> parts,
        string name,
        Color color
    )
    {
        var builder = ImmutableArray.CreateBuilder<PaintPart>(parts.Length);
        foreach (var part in parts)
        {
            builder.Add(part.MaterialName == name ? part with { Color = color } : part);
        }

        return builder.MoveToImmutable();
    }

    public void DuplicateFrom(BuildingPartPainter source)
    {
        if (source.TemplateName != TemplateName) { return; }

        paintedParts.Clear();
        foreach (var (k, v) in source.paintedParts)
        {
            paintedParts[k] = v with
            {
                Label = v.Label,
                Color = v.Color,
            };
        }

        if (source.IsPaintingEnabled)
        {
            RequestPainting();
        }
        else
        {
            DisablePainting();
        }
    }

}
