namespace ModdableTimberborn.Helpers;

partial class CommonExtensions
{
    public static bool IsBot(this CharacterType c) => c == CharacterType.Bot;
    public static bool IsBeaver(this CharacterType c) => c == CharacterType.AdultBeaver || c == CharacterType.ChildBeaver;
    public static bool IsWorker(this CharacterType c) => c == CharacterType.AdultBeaver || c == CharacterType.Bot;
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

    public static bool IsBuilder([NotNullWhen(true)] this Worker? worker)
        => worker && worker.Workplace.IsBuilderWorkplace();
    public static bool IsBuilderWorkplace([NotNullWhen(true)] this Workplace? workplace)
        => workplace && (workplace.GetComponentFast<DistrictCenterSpec>() || workplace.GetComponentFast<BuilderHubSpec>());

    public static BonusManager GetBonusManager<T>(this T component)
        where T : BaseComponent
        => component.GetComponentFast<BonusManager>();

    public static BonusTrackerComponent GetBonusTracker<T>(this T component)
        where T : BaseComponent
        => component.GetComponentFast<BonusTrackerComponent>();

    public static PersistentBonusTrackerComponent GetPersistentBonusTracker<T>(this T component)
        where T : BaseComponent
        => component.GetComponentFast<PersistentBonusTrackerComponent>();

}
