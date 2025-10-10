namespace ScientificProjects.Services.BaseMod;

public class FactionUpgradeRecipeModifier(
    ScientificProjectUnlockRegistry unlocks
) : BaseSpecModifier<RecipeSpec>
{

    protected override IEnumerable<RecipeSpec> Modify(IEnumerable<RecipeSpec> specs)
    {
        var hasFt = ScientificProjectsUtils.HasFtUpgrade = unlocks.Contains(ScientificProjectsUtils.FtPlankUpgradeId);
        var hasIt = ScientificProjectsUtils.HasItUpgrade = unlocks.Contains(ScientificProjectsUtils.ItSmelterUpgradeId);

        foreach (var s in specs)
        {
            if ((hasFt && s.Id == "TreatedPlank")
                || (hasIt && s.Id == "MetalBlock"))
            {
                yield return s with
                {
                    Products = [.. s.Products.Select(
                        p => p with { Amount = p.Amount * 2, }
                    )],
                };
            }
            else
            {
                yield return s;
            }
        }
    }
}