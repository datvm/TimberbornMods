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
            var entity = curr.GetEntity();
            ids.Add(entity.EntityId);

            dst[i] = new(
                entity.Http(),
                GetKind(curr),
                GetColor(curr),
                
                (HttpAutomatorState)(int)curr.State,
                curr.IsCyclicOrBlocked,
                
                [..curr.InputConnections.Select(conn => new HttpAutomationInput(
                    conn.Transmitter?.GetEntityId(),
                    (HttpAutomationConnectionState)(int)conn.State
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
