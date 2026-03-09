namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class CharacterHandler(
    BeaverPopulation beavers,
    EventBus eb
) : IMoreHttpApiHandler, ILoadableSingleton
{
    public string Endpoint => "characters";

    readonly HashSet<Bot> bots = [];

    public void Load()
    {
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
            _ => false,
        };

    public async Task<HttpPopulation> ListCharactersAsync()
    {
        List<HttpCharacter> 
            adults = [],
            children = [],
            bots = [];

        foreach (var b in beavers._beaverCollection.Adults)
        {
            AddCharacter(b, CharacterType.Adult, adults);
        }

        foreach (var b in beavers._beaverCollection.Children)
        {
            AddCharacter(b, CharacterType.Child, children);
        }

        foreach (var b in this.bots)
        {
            AddCharacter(b, CharacterType.Bot, bots);
        }

        return new([..adults], [..children], [..bots]);

        static void AddCharacter(BaseComponent comp, CharacterType characterType, List<HttpCharacter> list)
        {
            var citizen = comp.GetComponent<Citizen>();
            var progress = characterType switch
            {
                CharacterType.Adult => comp.GetComponent<LifeProgressor>().LifeProgress,
                CharacterType.Child => comp.GetComponent<Child>().GrowthProgress,
                CharacterType.Bot => comp.GetComponent<Deteriorable>().DeteriorationProgress,
                _ => throw new InvalidOperationException(),
            };

            

            Guid? dc = citizen.HasAssignedDistrict ? citizen.AssignedDistrict.GetEntityId() : null;
            Guid? dwelling = null;
            Guid? workplace = null;
            
            if (characterType != CharacterType.Bot)
            {
                var dweller = comp.GetComponent<Dweller>();
                if (dweller.HasHome)
                {
                    dwelling = dweller.Home.GetEntityId();
                }                
            }

            if (characterType != CharacterType.Child)
            {
                var worker = comp.GetComponent<Worker>();
                if (worker.Workplace)
                {
                    workplace = worker.Workplace.GetEntityId();
                }
            }

            var bonuses = comp.GetComponent<BonusManager>()._bonuses
                .Select(kv => (kv.Key, kv.Value.ClampedValue));

            list.Add(new(
                comp.GetComponent<EntityComponent>().Http(),
                characterType,
                comp.GetComponent<NamedEntity>().Http(),
                progress,
                dwelling,
                workplace,
                dc,
                [..bonuses]
            ));
        }

    }

    
}
