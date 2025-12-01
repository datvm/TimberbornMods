namespace ScientificProjects.Services.BaseMod;

public class FactionUpgradeRecipeModifier(
    ScientificProjectUnlockRegistry unlocks
) : BaseSpecTransformer<RecipeSpec>, ILoadableSingleton, ISpecModifier
{

    bool ISpecModifier.ShouldRun => ScientificProjectsUtils.HasFtUpgrade || ScientificProjectsUtils.HasItUpgrade;
    bool hasFt, hasIt;

    public void Load()
    {
        hasFt = ScientificProjectsUtils.HasFtUpgrade = unlocks.Contains(ScientificProjectsUtils.FtPlankUpgradeId);
        hasIt = ScientificProjectsUtils.HasItUpgrade = unlocks.Contains(ScientificProjectsUtils.ItSmelterUpgradeId);
    }

    public override RecipeSpec? Transform(RecipeSpec s)
    {
        if ((hasFt && s.Id == "TreatedPlank")
            || (hasIt && s.Id == "MetalBlock"))
        {
            return s with
            {
                Products = [.. s.Products.Select(
                    p => p with { Amount = p.Amount * 2, }
                )],
            };
        }
        else
        {
            return null;
        }
    }
}
