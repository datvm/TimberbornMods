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
    {
        switch (parsedRequestPath.RemainingSegment.Length)
        {
            case 0:
                return await context.HandleAsync(ListCharactersAsync);
            default:
                return false;
        }
    }

    public async Task<List<HttpCharacter>> ListCharactersAsync()
    {
        List<HttpCharacter> characters = [];

        foreach (var b in beavers._beaverCollection.Adults)
        {
            AddCharacter(b, isBeaver: true, isChild: false);
        }

        foreach (var b in beavers._beaverCollection.Children)
        {
            AddCharacter(b, isBeaver: true, isChild: true);
        }

        foreach (var b in bots)
        {
            AddCharacter(b, false, false);
        }

        return characters;

        void AddCharacter(BaseComponent comp, bool isBeaver, bool isChild)
        {
            characters.Add(new(
                comp.GetComponent<EntityComponent>().Http(),
                isBeaver,
                isChild,
                comp.GetComponent<NamedEntity>().Http()
            ));
        }
    }

    
}
