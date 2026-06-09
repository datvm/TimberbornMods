namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class SetCustomParameterHandler : NodeHandlerBase<SetCustomParameterData>
{
    public override string ForType => "SetCustomParameter";

    protected override string? InternalHandleNode(SetCustomParameterData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var name = controller.FormatText(data.Name);
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException($"Event {controller.Event.Id}, node {node.Id}: custom parameter name cannot be empty.");
        }

        controller.CurrentRecord.CustomParameters[name] = Compute(data, node, controller);
        return node.NextNodeId;
    }

    static string Compute(SetCustomParameterData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        return data.Type switch
        {
            CustomParameterValueType.String => ComputeString(data, node, controller),
            CustomParameterValueType.Int => FormatInt(ComputeNumber(data, node, controller)),
            CustomParameterValueType.Float => FormatFloat(ComputeNumber(data, node, controller)),
            _ => throw InvalidType(data, node, controller),
        };
    }

    static string ComputeString(SetCustomParameterData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (data.Operation != CustomParameterOperation.Add)
        {
            throw new InvalidOperationException($"Event {controller.Event.Id}, node {node.Id}: operation {data.Operation} is invalid for string custom parameters.");
        }

        var value1 = controller.FormatText(data.Value1) ?? "";
        var value2 = data.Value2 is null ? "" : controller.FormatText(data.Value2);
        return value1 + value2;
    }

    static float ComputeNumber(SetCustomParameterData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var value1 = controller.FormatTextFloat(data.Value1);

        return data.Operation switch
        {
            CustomParameterOperation.Add => data.Value2 is null ? value1 : value1 + GetValue2(),
            CustomParameterOperation.Sub => value1 - GetRequiredValue2(),
            CustomParameterOperation.Mul => value1 * GetRequiredValue2(),
            CustomParameterOperation.Div => value1 / GetRequiredValue2(),
            CustomParameterOperation.Round => Round(value1),
            CustomParameterOperation.Ceil => Ceil(value1),
            CustomParameterOperation.Floor => Floor(value1),
            _ => throw InvalidOperation(data, node, controller),
        };

        float GetValue2() => controller.FormatTextFloat(data.Value2);

        float GetRequiredValue2()
        {
            if (data.Value2 is null)
            {
                throw new InvalidOperationException($"Event {controller.Event.Id}, node {node.Id}: operation {data.Operation} requires Value2.");
            }

            return GetValue2();
        }

        float Round(float value)
        {
            ValidateNoValue2(data, node, controller);
            return Mathf.Round(value);
        }

        float Ceil(float value)
        {
            ValidateNoValue2(data, node, controller);
            return Mathf.Ceil(value);
        }

        float Floor(float value)
        {
            ValidateNoValue2(data, node, controller);
            return Mathf.Floor(value);
        }
    }

    static void ValidateNoValue2(SetCustomParameterData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (data.Value2 is not null)
        {
            throw new InvalidOperationException($"Event {controller.Event.Id}, node {node.Id}: operation {data.Operation} cannot have Value2.");
        }
    }

    static string FormatInt(float value) => Mathf.FloorToInt(value).ToString();
    static string FormatFloat(float value) => value.ToString("F2");

    static InvalidOperationException InvalidType(SetCustomParameterData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
        => new($"Event {controller.Event.Id}, node {node.Id}: unsupported custom parameter type {data.Type}.");

    static InvalidOperationException InvalidOperation(SetCustomParameterData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
        => new($"Event {controller.Event.Id}, node {node.Id}: unsupported custom parameter operation {data.Operation}.");
}
