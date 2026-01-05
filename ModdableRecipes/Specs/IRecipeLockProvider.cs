namespace ModdableRecipes.Specs;

public interface IRecipeLockProvider
{
    IEnumerable<RecipeLock> GetLockedRecipes();
}

public readonly record struct RecipeLock(string Id, string? Reason);
