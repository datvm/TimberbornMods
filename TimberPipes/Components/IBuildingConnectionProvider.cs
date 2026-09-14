namespace TimberPipes.Components;

public interface IBuildingConnectionProvider
{
    IBuildingPipeConnection? TryConnecting(BuildingPipe pipe, PipePortDefinition approach, bool give);
}

public class DefaultBuildingConnectionProvider : IBuildingConnectionProvider
{
    readonly BuildingPipeTarget building;
    readonly ValvePipeService service;
    readonly bool isAlreadyPipe;
    readonly bool anyFace;
    readonly List<BuildingTargetPort> ports;

    public DefaultBuildingConnectionProvider(BuildingPipeTarget building, ValvePipeService service)
    {
        this.building = building;
        this.service = service;

        isAlreadyPipe = building.GetComponent<BuildingPipe>() is { Enabled: true }
            || building.GetComponent<PipeTank>() is { Enabled: true };

        var explicitDef = building.GetComponent<BuildingPipeTargetPortsSpec>();
        if (explicitDef is null)
        {
            anyFace = true;
            ports = [];
            return;
        }

        anyFace = false;
        ports = BuildingPipeTargetIo.TransformPorts(building.BlockObject, explicitDef.Ports);
    }

    public IBuildingPipeConnection? TryConnecting(BuildingPipe pipe, PipePortDefinition approach, bool give)
    {
        if (isAlreadyPipe)
        {
            return null;
        }

        if (!BuildingPipeTargetIo.FaceAllowed(anyFace, ports, approach, give))
        {
            return null;
        }

        DefaultBuildingPipeConnection connection = new(building, service, give);
        if (!connection.IsValid)
        {
            return null;
        }

        foreach (var _ in connection.GetLiquidIds())
        {
            return connection;
        }

        return null;
    }
}
