namespace UnlockableRecipe.InferiorTreating;

public class InferiorTreatingLocker : IDefaultRecipeLocker
{
    public ImmutableHashSet<string> MayLockRecipeIds { get; } = [UnlockableRecipeModUtils.InferiorTreatedPlankRecipeId];

    public string GetLockReasonFor(string id, ILoc t)
    {
        return id switch
        {
            UnlockableRecipeModUtils.InferiorTreatedPlankRecipeId => t.T("LV.UR.InferiorTreatingLock", t.T("LV.UR.InferiorTreating")),
            _ => throw this.ThrowUnknownRecipeId(id),
        };
    }

}
