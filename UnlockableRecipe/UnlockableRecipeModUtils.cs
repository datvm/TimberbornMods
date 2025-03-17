namespace UnlockableRecipe
{
    public static class UnlockableRecipeModUtils
    {

        public const string InferiorTreatedPlankRecipeId = "InferiorTreatedPlank";
        public static readonly ImmutableHashSet<string> TapperShackPrefabNames = ["TappersShack.Folktails", "TappersShack.IronTeeth"];

        public static ArgumentOutOfRangeException ThrowUnknownRecipeId(this IDefaultRecipeLocker _, string id) => ThrowUnknownRecipeId(id);
        public static ArgumentOutOfRangeException ThrowUnknownRecipeId(this ICustomRecipeLocker _, string id) => ThrowUnknownRecipeId(id);
        public static ArgumentOutOfRangeException ThrowUnknownRecipeId(string id)
        {
            return new ArgumentOutOfRangeException(nameof(id), "Unknown recipe ID: " + id);
        }

    }
}
