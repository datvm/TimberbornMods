namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltLiftSpec))]
public class BeltLift(BeltRegistry registry) : BeltJunction
{
    SimLift? sim;

    public SimLift Sim => sim ??= new(SendingOut);

    public bool SendingOut => GetComponent<BeltLiftSpec>().Out;

    protected override void FillPorts(List<BeltPort> localInputs, List<BeltPort> localOutputs)
        => BeltLayout.FillLift(SendingOut, localInputs, localOutputs);

    public override bool TryProvide(BeltApproach approach, SimBelt? upstream, out ISimLink link)
    {
        link = SimLinks.None;
        if (upstream is not { Plain: true } || !AcceptsInput(approach.From))
        {
            return false;
        }

        if (SendingOut)
        {
            Sim.Remember(upstream);
        }

        link = SimLinks.ToBelt(Sim.Belt);
        return true;
    }

    protected override void Entered() => registry.Register(this);

    protected override void Exited() => registry.Unregister(this);
}
