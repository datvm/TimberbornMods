namespace UnlockableRecipe;

public class RecipeLockerUi(
    IAssetLoader assets,
    DialogBoxShower diagShower
) : ILoadableSingleton, IUnloadableSingleton
{
    internal static RecipeLockerUi? Instance { get; private set; }
    public Sprite LockIcon { get; private set; } = null!;

    public void Load()
    {
        Instance = this;
        LoadLockIcon();    
    }

    void LoadLockIcon()
    {
        LockIcon = assets.Load<Sprite>(@"ui/images/game/lock-icon-yellow");
    }

    public void ShowLockedRecipeError(string reason)
    {
        diagShower.Create()
            .SetMessage(reason)
            .Show();
    }

    public void Unload()
    {
        Instance = null;
    }
}
