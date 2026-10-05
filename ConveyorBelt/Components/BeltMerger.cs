namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltMergerSpec))]
public class BeltMerger(BeltRegistry registry) : BeltJunction
{
    public SimMerger Sim { get; } = new();

    protected override void FillPorts(List<BeltPort> localInputs, List<BeltPort> localOutputs)
        => BeltLayout.FillMerger(localInputs, localOutputs);

    public override bool TryProvide(BeltApproach approach, SimBelt? upstream, out ISimLink link)
    {
        link = SimLinks.None;
        if (upstream is not { Plain: true } || !AcceptsInput(approach.From))
        {
            return false;
        }

        Sim.Remember(upstream);
        link = SimLinks.ToBelt(Sim.Belt);
        return true;
    }

    protected override void Entered() => registry.Register(this);

    protected override void Exited() => registry.Unregister(this);
}
