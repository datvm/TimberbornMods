namespace MoreHttpApiAutomations.Handlers;

public class AutomationHandler(
    AutomatorRegistry registry,
    BuildingHandler buildingHandler
) : IMoreHttpApiHandler
{
    public string Endpoint => "automations";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetAutomationMapAsync),
            _ => false,
        };

    public async Task<HttpAutomationMap> GetAutomationMapAsync()
    {
        var src = registry.Automators;
        var dst = new HttpAutomator[src.Count];
        List<Guid> ids = [];

        for (int i = 0; i < src.Count; i++)
        {
            var curr = src[i];
            var automatable = curr.GetComponent<Automatable>();

            var entity = curr.GetEntity();
            ids.Add(entity.EntityId);

            dst[i] = new(
                entity.Http(),
                GetKind(curr),
                GetColor(curr),
                GetAutomatorState(curr.State),
                curr.IsCyclicOrBlocked,
                automatable ? (HttpAutomationConnectionState)(int)automatable.State : null,

                [..curr.InputConnections.Select(conn => new HttpAutomationInput(
                    conn.Transmitter?.GetEntityId(),
                    GetConnectionState(conn.State)
                ))]
            );
        }

        var buildings = buildingHandler.GetBuildings(ids);
        return new(dst, buildings);
    }

    static HttpColor GetColor(Automator automator)
    {
        var comp = automator.GetComponent<CustomizableIlluminator>();
        return (comp ? comp.CustomColor : Color.white).Http();
    }

    static HttpAutomatorState GetAutomatorState(Enum state)
        => state.ToString() switch
        {
            nameof(HttpAutomatorState.On) => HttpAutomatorState.On,
            nameof(HttpAutomatorState.Error) => HttpAutomatorState.Error,
            _ => HttpAutomatorState.Off,
        };

    static HttpAutomationConnectionState GetConnectionState(Enum state)
        => state.ToString() switch
        {
            nameof(HttpAutomationConnectionState.On) => HttpAutomationConnectionState.On,
            nameof(HttpAutomationConnectionState.Disconnected) => HttpAutomationConnectionState.Disconnected,
            _ => HttpAutomationConnectionState.Off,
        };

    static HttpAutomatorKind GetKind(Automator automator)
    {
        if (automator.IsTerminal)
        {
            return HttpAutomatorKind.Terminal;
        }
        else if (automator.IsCombinationalTransmitter)
        {
            return HttpAutomatorKind.CombinationalTransmitter;
        }
        else if (automator.IsSequentialTransmitter)
        {
            return HttpAutomatorKind.SequentialTransmitter;
        }
        else if (automator.IsSamplingTransmitter)
        {
            return HttpAutomatorKind.SamplingTransmitter;
        }

        return HttpAutomatorKind.Transmitter;
    }

}
