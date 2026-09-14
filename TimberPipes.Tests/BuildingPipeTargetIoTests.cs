namespace TimberPipes.Tests;

public class BuildingPipeTargetIoTests
{
    [Fact]
    public void AnyFaceAllowsEveryApproach()
    {
        var approach = new PipePortDefinition(new Vector3Int(1, 0, 0), Direction3D.Down);
        Assert.True(BuildingPipeTargetIo.FaceAllowed(true, [], approach, give: true));
        Assert.True(BuildingPipeTargetIo.FaceAllowed(true, [], approach, give: false));
    }

    [Fact]
    public void SpecAllowsOnlyMatchingInOrOutFace()
    {
        BuildingTargetPort[] ports =
        [
            new(new Vector3Int(0, 0, 0), Direction3D.Down, PipePortState.OpenIn),
            new(new Vector3Int(0, 0, 0), Direction3D.Up, PipePortState.OpenOut),
        ];

        Assert.True(BuildingPipeTargetIo.FaceAllowed(
            false,
            ports,
            new(new Vector3Int(0, 0, 0), Direction3D.Down),
            give: true));
        Assert.False(BuildingPipeTargetIo.FaceAllowed(
            false,
            ports,
            new(new Vector3Int(0, 0, 0), Direction3D.Down),
            give: false));
        Assert.True(BuildingPipeTargetIo.FaceAllowed(
            false,
            ports,
            new(new Vector3Int(0, 0, 0), Direction3D.Up),
            give: false));
        Assert.False(BuildingPipeTargetIo.FaceAllowed(
            false,
            ports,
            new(new Vector3Int(0, 0, 0), Direction3D.Left),
            give: true));
    }

    [Fact]
    public void EmptySpecPortsConnectNowhere()
    {
        var approach = new PipePortDefinition(new Vector3Int(0, 0, 0), Direction3D.Down);
        Assert.False(BuildingPipeTargetIo.FaceAllowed(false, [], approach, give: true));
        Assert.False(BuildingPipeTargetIo.FaceAllowed(false, [], approach, give: false));
    }

    [Fact]
    public void OpenFaceAllowsFillAndExtract()
    {
        BuildingTargetPort[] ports =
        [
            new(new Vector3Int(2, 1, 0), Direction3D.Left, PipePortState.Open),
        ];
        var approach = new PipePortDefinition(new Vector3Int(2, 1, 0), Direction3D.Left);
        Assert.True(BuildingPipeTargetIo.FaceAllowed(false, ports, approach, give: true));
        Assert.True(BuildingPipeTargetIo.FaceAllowed(false, ports, approach, give: false));
    }
}
