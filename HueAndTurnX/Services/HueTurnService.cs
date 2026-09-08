
namespace HueAndTurnX.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class HueTurnService(
    ColorHighlighter colorHighlighter,
    TransparentShaderService transparentShaderService
)
{

    public void Apply(HueTurnComponent component)
    {
        ApplyPosition(component);
        ApplyColor(component);
    }

    public void ApplyPosition(HueTurnComponent htComp)
    {
        htComp.ResetToOriginalPositioning();

        var positioning = htComp.Positions;
        var rotation = WorldOrDefault(positioning.Rotation);
        var translation = WorldOrDefault(positioning.Translation);
        var scale = WorldOrDefault(positioning.Scale, Vector3.one);

        var size = CoordinateSystem.GridToWorld(htComp.Size);
        translation.Scale(size);

        var t = htComp.Transform;
        // Rotate
        t.Rotate(rotation, Space.Self);

        // Translate
        t.Translate(translation, Space.World);

        // Scale
        htComp.ScaleTo(scale);

        // Helper methods
        static Vector3 WorldOrDefault(Vector3? value, Vector3 def = default) => value is null ? def : CoordinateSystem.GridToWorld(value.Value);
    }

    public void ApplyColor(HueTurnComponent htComp)
    {
        var comp = htComp.Colors;
        var color = comp.Color;
        var alpha = comp.Transparency;

        if (color.HasValue)
        {
            colorHighlighter.SetColor(htComp, color.Value);
        }
        else
        {
            colorHighlighter.ResetColor(htComp);
        }

        if (alpha is null || alpha == 1)
        {
            RestoreOriginalMaterials(htComp);
        }
        else
        {
            var a = alpha.Value;

            if (htComp.ReplacedMaterials is null)
            {
                htComp.OriginalSharedMaterials = [.. htComp.Renderers.Select(r => r ? r.sharedMaterials : [])];
                htComp.ReplacedMaterials = transparentShaderService.ReplaceRenderersMaterials(
                    htComp.Renderers,
                    replaceMaterials: true,
                    replaceSharedMaterials: false,
                    replaceEnv: true,
                    replaceTerrain: false);
            }

            if (htComp.ReplacedMaterials.Length == 0)
            {
                RestoreOriginalMaterials(htComp);
                return;
            }

            foreach (var m in htComp.ReplacedMaterials)
            {
                if (!m) { continue; }

                m.SetEnvironmentAlpha(a);
            }
        }
    }

    static void RestoreOriginalMaterials(HueTurnComponent htComp)
    {
        var originals = htComp.OriginalSharedMaterials;
        if (originals is null) { return; }

        var renderers = htComp.Renderers;
        var count = Math.Min(renderers.Length, originals.Length);
        for (var i = 0; i < count; i++)
        {
            var renderer = renderers[i];
            if (!renderer) { continue; }

            var instanced = renderer.sharedMaterials;
            renderer.sharedMaterials = originals[i];

            foreach (var m in instanced)
            {
                if (!m) { continue; }
                if (originals[i].Contains(m)) { continue; }

                Object.Destroy(m);
            }
        }

        htComp.ReplacedMaterials = null;
        htComp.OriginalSharedMaterials = null;
    }

}
