namespace AutomationArrays.Components;

[AddTemplateModule2(typeof(RelayHubSpec))]
public class SegmentedIlluminator(IlluminationService illuminationService) : BaseComponent, IPreviewStateListener
{
    public const float DefaultStrength = 1f;

    class SegmentInfo(GameObject go, MeshRenderer[] renderers, Material[] materials, Color color)
    {
        public GameObject GameObject { get; } = go;
        public Material[] Materials { get; } = materials;
        public MeshRenderer[] Renderers { get; } = renderers;
        public Color Color { get; set; } = color;
        public float Strength { get; set; } = DefaultStrength;
        public bool On { get; set; }
    }

    SegmentInfo[] segments = null!;

    public void InitializeSegments(GameObject[] segmentRoots)
    {
        var c = illuminationService.DefaultColor;

        segments = [.. segmentRoots.Select(go => {
            var renderers = go.GetComponentsInChildren<MeshRenderer>();
            List<Material> materials = [];

            foreach (var r in renderers)
            {
                materials.AddRange(r.materials);
            }

            return new SegmentInfo(go, renderers, [..materials], c);
        })];
    }

    public void OnEnterPreviewState() => SetAllSegments(on: false);

    public void SetAllSegments(Color? color = null, float? strength = null, bool? on = null)
    {
        for (int i = 0; i < segments.Length; i++)
        {
            SetSegment(i, color, strength, on);
        }
    }

    public void SetSegment(int index, Color? color = null, float? strength = null, bool? on = null)
    {
        var seg = segments[index];

        if (color.HasValue)
        {
            var c = color.Value;

            if (c != seg.Color)
            {
                seg.Color = c;
                ColorMaterials(seg.Materials, c);
            }
        }

        var changed = false;
        if (strength.HasValue && seg.Strength != strength.Value)
        {
            seg.Strength = strength.Value;
            changed = true;
        }

        if (on.HasValue && seg.On != on.Value)
        {
            seg.On = on.Value;
            changed = true;
        }

        if (!changed) { return; }
        MaterialLightingEnabler.SetStrengthInRenderers(seg.Renderers, seg.On ? seg.Strength : 0);
    }

    void ColorMaterials(Material[] materials, Color c)
    {
        foreach (var m in materials)
        {
            m.SetColor(MaterialColorer.LightingColorProperty, c);
        }
    }

}
