namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class CharacterHandler(
    BeaverPopulation beavers,
    EventBus eb,
    EntityBadgeService entityBadgeService,
    ISpecService specs,
    ILoc t,
    EntityRegistry entityRegistry
) : IMoreHttpApiHandler, ILoadableSingleton
{
    public string Endpoint => "characters";

    readonly HashSet<Bot> bots = [];
    readonly FrozenSet<string>[] characterBonuses = new FrozenSet<string>[4]; // Unknown is at 0

    public void Load()
    {
        var wellbeings = specs.GetSpecs<WellbeingBonusesSubjectSpec>();
        foreach (var spec in wellbeings)
        {
            CharacterType ct;
            if (spec.HasSpec<AdultSpec>())
            {
                ct = CharacterType.Adult;
            }
            else if (spec.HasSpec<ChildSpec>())
            {
                ct = CharacterType.Child;
            }
            else if (spec.HasSpec<BotSpec>())
            {
                ct = CharacterType.Bot;
            }
            else
            {
                continue;
            }

            characterBonuses[(int)ct] = [.. spec.Bonuses];
        }

        eb.Register(this);
    }

    [OnEvent]
    public void OnCharacterCreated(CharacterCreatedEvent e)
    {
        var b = e.Character.GetComponent<Bot>();
        if (b)
        {
            bots.Add(b);
        }
    }

    [OnEvent]
    public void OnCharacterKilled(CharacterKilledEvent e)
    {
        var b = e.Character.GetComponent<Bot>();
        if (b is not null)
        {
            bots.Remove(b);
        }
    }

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(ListCharactersAsync),
            1 => await context.HandleAsync(() => GetCharacterDetailedAsync(parsedRequestPath)),
            _ => false,
        };

    public async Task<HttpPopulation> ListCharactersAsync()
    {
        List<HttpCharacter>
            adults = [],
            children = [],
            bots = [];

        HashSet<EntityComponent> relevantBuildings = [];

        var bonuses = characterBonuses[(int)CharacterType.Adult];
        foreach (var b in beavers._beaverCollection.Adults)
        {
            adults.Add(GetCharacterBasicInfo(b, CharacterType.Adult, bonuses, relevantBuildings));
        }

        bonuses = characterBonuses[(int)CharacterType.Child];
        foreach (var b in beavers._beaverCollection.Children)
        {
            children.Add(GetCharacterBasicInfo(b, CharacterType.Child, bonuses, relevantBuildings));
        }

        bonuses = characterBonuses[(int)CharacterType.Bot];
        foreach (var b in this.bots)
        {
            bots.Add(GetCharacterBasicInfo(b, CharacterType.Bot, bonuses, relevantBuildings));
        }

        return new([.. adults], [.. children], [.. bots], ParseRelevantBuildings(relevantBuildings));

    }

    public async Task<HttpCharacterDetailed> GetCharacterDetailedAsync(ParsedRequestPath parsedRequestPath)
    {
        if (!Guid.TryParse(parsedRequestPath.RemainingSegment[0], out var id))
        {
            throw new StatusCodeException(400, "Invalid character ID");
        }

        var entity = entityRegistry.GetEntity(id);
        if (!entity)
        {
            throw new StatusCodeException(404, "Character not found");
        }

        var type = GetCharacterType(entity);
        if (type == CharacterType.Unknown)
        {
            throw new StatusCodeException(400, "Entity is not a character");
        }

        HashSet<EntityComponent> buildings = [];
        var basic = GetCharacterBasicInfo(entity, type, characterBonuses[(int)type], buildings);

        var needMan = entity.GetComponent<NeedManager>();
        var needs = needMan._needs.AllNeeds
            .Select(n => new HttpCharacterNeed(n.NeedSpec.Id, n.Points, n.Enabled, n.Wellbeing))
            .ToArray();

        var carrier = entity.GetComponent<GoodCarrier>();
        var carrying = carrier && carrier.IsCarrying;
        ParsedGoodAmountSpec? carryingGood = carrying ? carrier.CarriedGoods.Http() : null;

        return new(
            basic, ParseRelevantBuildings(buildings),
            needs,
            carryingGood, carrying ? carrier.LiftingCapacity : 0
        );
    }

    static CharacterType GetCharacterType(EntityComponent comp)
    {
        if (comp.HasComponent<AdultSpec>())
        {
            return CharacterType.Adult;
        }
        else if (comp.HasComponent<ChildSpec>())
        {
            return CharacterType.Child;
        }
        else if (comp.HasComponent<BotSpec>())
        {
            return CharacterType.Bot;
        }
        else
        {
            return CharacterType.Unknown;
        }
    }

    HttpCharacter GetCharacterBasicInfo(BaseComponent comp, CharacterType characterType, FrozenSet<string> possibleBonuses, HashSet<EntityComponent> relevantBuildings)
    {
        var citizen = comp.GetComponent<Citizen>();
        var progress = characterType switch
        {
            CharacterType.Adult => comp.GetComponent<LifeProgressor>().LifeProgress,
            CharacterType.Child => comp.GetComponent<Child>().GrowthProgress,
            CharacterType.Bot => comp.GetComponent<Deteriorable>().DeteriorationProgress,
            _ => throw new InvalidOperationException(),
        };

        var character = comp.GetComponent<Character>();

        var dc = citizen.HasAssignedDistrict ? citizen.AssignedDistrict.GetEntity() : null;
        EntityComponent? dwelling = null;
        EntityComponent? workplace = null;

        if (characterType != CharacterType.Bot)
        {
            var dweller = comp.GetComponent<Dweller>();
            if (dweller.HasHome)
            {
                dwelling = dweller.Home.GetEntity();
            }
        }

        if (characterType != CharacterType.Child)
        {
            var worker = comp.GetComponent<Worker>();
            if (worker.Workplace)
            {
                workplace = worker.Workplace.GetEntity();
            }
        }

        var bonuses = comp.GetComponent<BonusManager>()._bonuses
            .Where(kv => possibleBonuses.Contains(kv.Key))
            .Select(kv => (kv.Key, kv.Value.ClampedValue))
            .ToArray();

        var avatar = entityBadgeService.GetAvatarPath(comp);

        if (dc) { relevantBuildings.Add(dc!); }
        if (dwelling) { relevantBuildings.Add(dwelling!); }
        if (workplace) { relevantBuildings.Add(workplace!); }

        return new(
            comp.GetComponent<EntityComponent>().Http(),
            characterType,
            character.FirstName,
            avatar,
            character.Age,
            progress,
            dwelling?.EntityId,
            workplace?.EntityId,
            dc?.EntityId,
            comp.GetComponent<WellbeingTracker>().Wellbeing,
            bonuses
        );
    }

    Dictionary<Guid, HttpCharacterBuilding> ParseRelevantBuildings(HashSet<EntityComponent> comps)
    {
        Dictionary<Guid, HttpCharacterBuilding> r = [];

        foreach (var c in comps)
        {
            var id = c.EntityId;
            r[id] = new(id,
                entityBadgeService.GetAvatarPath(c),
                c.GetName(t),
                c.GetLabeledName(t));
        }

        return r;
    }

}
