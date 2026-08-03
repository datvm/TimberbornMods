namespace TerraceFarm.Services;

/// <summary>
/// Freeds occupation flags on crops so they can share a cell with a Terrace:
/// - Z0: drop Path/Top (terrace claims those); keep Bottom so farmhouses still find harvest targets.
/// - Z1 (tall crops): drop Floor/Bottom/Path so the terrace upper path volume can exist.
/// Paths may run through crop cells; that is intentional.
/// </summary>
[MultiBind(typeof(ITemplateModifier))]
public class CropModifier : ITemplateModifier
{
    // Keep Bottom (harvest via GetBottomObjectAt). Clear Path/Top for terrace Path+Top.
    const BlockOccupations FirstBlockMask = ~(BlockOccupations.Path | BlockOccupations.Top);

    // Thin upper stalk so terrace Z1 Floor/Bottom/Path does not conflict.
    const BlockOccupations SecondBlockMask = ~(BlockOccupations.Floor | BlockOccupations.Bottom | BlockOccupations.Path);

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        template.TransformSpec<BlockObjectSpec>(bos =>
        {
            var blocks = bos.Blocks.ToArray();
            blocks[0] = blocks[0] with
            {
                Occupations = blocks[0].Occupations & FirstBlockMask
            };

            if (blocks.Length > 1)
            {
                blocks[1] = blocks[1] with
                {
                    Occupations = blocks[1].Occupations & SecondBlockMask
                };
            }

            return bos with { Blocks = [.. blocks] };
        });

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => originalTemplateSpec.HasSpec<CropSpec>();
}
