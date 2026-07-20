namespace ModdableRecipes.UI;

[BindSingleton]
public class ModdableRecipeUIController(
    ModdableRecipeLockService locker,
    ILoc t,
    IAssetLoader assets,
    DialogService diag
) : ILoadableSingleton
{

    public Sprite LockIcon { get; private set; } = null!;
    public RecipeDropdownItemUIInfo None { get; private set; }

    public void Load()
    {
        LockIcon = assets.Load<Sprite>(@"ui/images/game/lock-icon-yellow");
        None = new(null, t.T(ManufactoryDropdownProvider.NoRecipeItemLocKey), null, ModdableRecipeLockStatus.Unlocked);
    }

    public RecipeDropdownItemUIInfo? GetRecipeInfo(RecipeSpec recipe, ModdableRecipeBuilding? building = null)
    {
        var status = locker.GetLockStatus(recipe.Id, building);

        if (status == ModdableRecipeLockStatus.Hidden)
        {
            return null;
        }

        var title = status switch
        {
            ModdableRecipeLockStatus.Unlocked => t.T(recipe.DisplayLocKey),
            ModdableRecipeLockStatus.Censor => t.T("LV.MRec.LockedCensor"),
            ModdableRecipeLockStatus.LockedVisible => t.T("LV.MRec.Locked", t.T(recipe.DisplayLocKey)),
            _ => throw new ArgumentOutOfRangeException()
        };

        var icon = status == ModdableRecipeLockStatus.Unlocked ? recipe.Icon.Asset : LockIcon;

        return new(recipe, title, icon, status);
    }

    public bool OnLockedRecipeSelected(RecipeDropdownItemUIInfo item, ModdableRecipeBuilding? building = null)
    {
        var isLocked = locker.IsLocked(item.Spec!.Id, building, out var reason);
        if (!isLocked) { return false; }

        locker.ReselectManufactory();

        diag.Alert(reason!);

        return true;
    }

}
