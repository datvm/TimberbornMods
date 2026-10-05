namespace ConveyorBelt.Algorithm;

interface IInputTurn
{
    IReadOnlyList<SimBelt> Inputs { get; }

    void Rotate(List<SimBelt> order);
}

sealed class InputFan
{
    readonly List<SimBelt> inputs = [];
    readonly List<int> indices = [];

    public IReadOnlyList<SimBelt> Inputs => inputs;

    public void Clear() => inputs.Clear();

    public void Remember(SimBelt belt)
    {
        if (!belt.Plain || inputs.Contains(belt))
        {
            return;
        }

        inputs.Add(belt);
    }

    public void Rotate(List<SimBelt> order)
    {
        if (inputs.Count < 2)
        {
            return;
        }

        indices.Clear();
        for (var i = 0; i < order.Count; i++)
        {
            if (inputs.Contains(order[i]))
            {
                indices.Add(i);
            }
        }

        if (indices.Count < 2)
        {
            return;
        }

        var first = order[indices[0]];
        for (var i = 0; i < indices.Count - 1; i++)
        {
            order[indices[i]] = order[indices[i + 1]];
        }

        order[indices[^1]] = first;
    }
}

public sealed class SimMerger : IInputTurn
{
    readonly InputFan fan = new();

    public SimBelt Belt { get; } = new() { ItemsPerHour = BeltRates.Junction, Plain = false };

    public bool Running
    {
        get => Belt.Running;
        set => Belt.Running = value;
    }

    public IReadOnlyList<SimBelt> Inputs => fan.Inputs;

    public void ClearInputs() => fan.Clear();

    public void Remember(SimBelt belt) => fan.Remember(belt);

    public void Rotate(List<SimBelt> order) => fan.Rotate(order);
}

public sealed class SimSplitter : ISplitHost
{
    readonly List<SmartSplitPort> ports = [];

    public SimSplitter(IReadOnlyList<SmartSplitPort> splitPorts)
    {
        ports.AddRange(splitPorts);
    }

    public SimBelt Belt { get; } = new() { ItemsPerHour = BeltRates.Junction, Plain = false };

    public bool Running
    {
        get => Belt.Running;
        set => Belt.Running = value;
    }

    public int Cursor { get; set; }
    public IReadOnlyList<SmartSplitPort> Ports => ports;

    public void SetPort(int index, SmartSplitPort port)
    {
        if (index < 0 || index >= ports.Count)
        {
            return;
        }

        ports[index] = port;
    }

    public void Attach(IReadOnlyList<ISimLink> outputs) => Belt.Output = new SplitLink(this, outputs.ToArray());
}

public sealed class SimLift(bool sendingOut) : ISplitHost, IInputTurn
{
    readonly InputFan fan = new();
    readonly List<SmartSplitPort> ports = [SmartSplitPort.AnyPort, SmartSplitPort.AnyPort];

    public SimBelt Belt { get; } = new() { ItemsPerHour = BeltRates.Junction, Plain = false };
    public bool SendingOut { get; } = sendingOut;

    public bool Running
    {
        get => Belt.Running;
        set => Belt.Running = value;
    }

    public IReadOnlyList<SimBelt> Inputs => fan.Inputs;
    public int Cursor { get; set; }
    public IReadOnlyList<SmartSplitPort> Ports => ports;

    public void ClearInputs() => fan.Clear();

    public void Remember(SimBelt belt) => fan.Remember(belt);

    public void AttachOut(ISimLink output) => Belt.Output = output;

    public void AttachSplit(IReadOnlyList<ISimLink> outputs) => Belt.Output = new SplitLink(this, outputs.ToArray());

    public void Rotate(List<SimBelt> order)
    {
        if (!SendingOut)
        {
            return;
        }

        fan.Rotate(order);
    }
}
