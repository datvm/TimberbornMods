namespace UnlockableRecipe;

/// <summary>
/// Implement for simple blockers that are just locked/unlocked through <see cref="DefaultRecipeLockerController" />
/// </summary>
public interface IDefaultRecipeLocker
{

    /// <summary>
    /// The IDs of the recipes that should be locked/unlocked by this blocker.
    /// </summary>
    public ImmutableHashSet<string> MayLockRecipeIds { get; }

    /// <summary>
    /// Give the reason why a recipe is locked. This is only called if the recipe is locked so you should always return a reason.
    /// </summary>
    /// <param name="id">The ID of the recipe to check. Only IDs that are in <see cref="MayLockRecipeIds" /> are given to this function.</param>
    /// <returns>The reason why the recipe is blocked to show to user.</returns>
    public string GetLockReasonFor(string id, ILoc t);

}

/// <summary>
/// Implement for blockers that need to block recipes based on more complex logic.
/// </summary>
public interface ICustomRecipeLocker
{

    /// <summary>
    /// The IDs of the recipes that should be locked/unlocked by this blocker.
    /// </summary>
    public ImmutableHashSet<string> MayLockRecipeIds { get; }

    /// <summary>
    /// Return the reason a recipe should be locked. Return null if the recipe is unlocked.
    /// </summary>
    /// <param name="id">The ID of the recipe to check. Only IDs that are in <see cref="MayLockRecipeIds" /> are given to this function.</param>
    /// <param name="t">The localization to use for the reason.</param>
    /// <returns>The reason why the recipe is locked to show to user. Null if the recipe is unlocked.</returns>
    /// <remarks>If <paramref name="t"/> is null, the caller is only interested in whether the recipe is locked or not,
    /// return an empty string or non-null value if it's locked.</remarks>
    public string? ShouldLockRecipe(string id, ILoc? t);

}
