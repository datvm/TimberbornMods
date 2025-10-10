namespace ModdableTimberborn.Helpers;

public static class CommonExtensions
{

    public static TComp? GetComponentOrNullFast<TComp>(this BaseComponent component)
        where TComp : BaseComponent
    {
        var result = component.GetComponentFast<TComp>();
        return result ? result : null;
    }

    public static BonusManager GetBonusManager<T>(this T component)
        where T : BaseComponent
        => component.GetComponentFast<BonusManager>();

    public static BonusTrackerComponent GetBonusTracker<T>(this T component)
        where T : BaseComponent
        => component.GetComponentFast<BonusTrackerComponent>();

    public static PersistentBonusTrackerComponent GetPersistentBonusTracker<T>(this T component)
        where T : BaseComponent
        => component.GetComponentFast<PersistentBonusTrackerComponent>();

    public static Lazy<Blueprint> ToLazyBlueprint<T>(this T spec) where T : ComponentSpec => new(() => spec.ToBlueprint());
    public static Blueprint ToBlueprint<T>(this T spec) where T : ComponentSpec => new([spec], []);
    public static T[] GetLazySpecs<T>(this SpecService specService) where T : ComponentSpec 
        => [.. specService._cachedBlueprints[typeof(T)].Select(q => q.Value.GetSpec<T>())];

    public static bool IsGameContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.Game);
    public static bool IsMenuContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.MainMenu);
    public static bool IsBootstrapperContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.Bootstrapper);
    public static bool IsMapEditorContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.MapEditor);
    public static bool IsGameplayContext(this ConfigurationContext context) => context.IsGameContext() || context.IsMapEditorContext();

    public static BonusSpec ToBonusSpec(this BonusType t, float multiplierDelta) => new(t.ToString(), multiplierDelta);

    public static bool IsBot(this CharacterType c) => c.HasFlag(CharacterType.Bot);
    public static bool IsBeaver(this CharacterType c) => (c & CharacterType.Beaver) != 0;
    public static CharacterType GetCharacterType<T>(this T comp) where T : BaseComponent
    {
        if (comp.GetComponentFast<BotSpec>())
        {
            return CharacterType.Bot;
        }

        if (comp.GetComponentFast<AdultSpec>())
        {
            return CharacterType.AdultBeaver;
        }

        if (comp.GetComponentFast<ChildSpec>())
        {
            return CharacterType.ChildBeaver;
        }

        return CharacterType.Unknown;
    }
}