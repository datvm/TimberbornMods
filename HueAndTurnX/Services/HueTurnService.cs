
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
            if (htComp.ReplacedMaterials is not null)
            {
                transparentShaderService.RestoreShaders(htComp.ReplacedMaterials, true, false, false);
                htComp.ReplacedMaterials = null;
            }
        }
        else
        {
            var a = alpha.Value;

            htComp.ReplacedMaterials ??= transparentShaderService.ReplaceRenderersMaterials(htComp.Renderers, true, true, true, false); ;
            if (htComp.ReplacedMaterials.Length == 0)
            {
                htComp.ReplacedMaterials = null;
                return;
            }

            foreach (var r in htComp.Renderers)
            {
                foreach (var m in r.materials.Concat(r.sharedMaterials))
                {
                    m.SetEnvironmentAlpha(a);
                }
            }
        }
    }

}
