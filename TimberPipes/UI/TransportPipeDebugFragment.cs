namespace TimberPipes.UI;

[BindSingleton]
class TransportPipeDebugFragment(
    DebugFragmentFactory fragments,
    PipeRegistry registry
) : IEntityPanelFragment
{
    const string DefaultGoodId = "Water";

    VisualElement root = null!;
    Label text = null!;
    BuildingPipe? pipe;

    public VisualElement InitializeFragment()
    {
        root = fragments.Create("TransportPipe", new DebugFragmentButton(Fill, "Fill"));
        text = root.Q<Label>("Text");
        text.style.whiteSpace = WhiteSpace.Normal;
        return root;
    }

    public void ShowFragment(BaseComponent entity)
    {
        var found = entity.GetComponent<BuildingPipe>();
        pipe = found && found.IsTransportPipe ? found : null;
        UpdateFragment();
    }

    public void ClearFragment()
    {
        pipe = null;
        UpdateFragment();
    }

    public void UpdateFragment()
    {
        if (pipe is not { } current)
        {
            root.ToggleDisplayStyle(false);
            return;
        }

        root.ToggleDisplayStyle(true);
        text.text = FormatDebug(current);
    }

    void Fill()
    {
        if (pipe is not { } current)
        {
            return;
        }

        if (!current.IsContaminated)
        {
            current.AssignFluidId(current.NetworkGoodId ?? current.Graph?.FluidGoodId ?? DefaultGoodId);
        }

        current.SetVolume(BuildingPipe.MaxWaterHeight);
    }

    string FormatDebug(BuildingPipe pipe)
    {
        var text = $"{pipe.Coordinates}  V={pipe.FluidHeight:0.00}  h={pipe.Head:0.00}  id={pipe.NetworkGoodId ?? "none"}";
        if (pipe.Graph is null)
        {
            text += "  (no graph)";
        }
        else if (pipe.Graph.Cause.HasPair)
        {
            text += $"\ncontaminated: {pipe.Graph.Cause.GoodA} + {pipe.Graph.Cause.GoodB}";
        }

        if (pipe.Ports is not { } ports)
        {
            return text;
        }

        List<PipePort> ordered = [.. ports.Values];
        ordered.Sort((a, b) => a.Direction.CompareTo(b.Direction));
        foreach (var port in ordered)
        {
            text += $"\n{port.Direction} {port.State}";
            if (!port.IsConnected)
            {
                text += "  —";
                continue;
            }

            text += port.CanOutflow ? "  out" : "  no-out";
            text += port.CanInflow ? " in" : " no-in";
            if (!registry.TryGetConnectedBuilding(port, out var other))
            {
                text += "  ?";
                continue;
            }

            text += $"  {other.Coordinates}";
            if (other.GetComponentOrNull<PipeTank>() is { } tank)
            {
                text += $"  tank V={tank.VolumeM3:0.00} id={tank.FluidGoodId ?? "empty"} h={tank.Head:0.00}";
                continue;
            }

            if (other.GetComponentOrNull<PipePump>() is { } pump)
            {
                text += $"  well V={pump.VolumeM3:0.00} id={pump.FluidGoodId ?? "empty"} h={pump.Head:0.00}";
                continue;
            }

            text += $"  V={other.FluidHeight:0.00} id={other.FluidGoodId ?? "none"} h={other.Head:0.00}";
        }

        return text;
    }
}
