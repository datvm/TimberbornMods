namespace AutomationArrays.Services;

public static class RelayHubEvaluator
{

    public static bool Evaluate(RelayHub hub, List<bool> reuseInput, List<bool> arrayOutput)
    {
        GatherInputs(hub, reuseInput);
        EvaluateArrayMode(hub, reuseInput, arrayOutput);
        var singleResult = EvaluateSingleMode(hub, reuseInput);

        reuseInput.Clear();

        return singleResult;
    }

    static void GatherInputs(RelayHub hub, List<bool> inputs)
    {
        inputs.Clear();

        foreach (var conn in hub.Connections)
        {
            if (conn.State == ConnectionState.Disconnected)
            {
                inputs.Add(false);
                continue;
            }

            var trans = conn.Transmitter;

            if (trans.IsArrayTransmitter(out var arrTrans))
            {
                inputs.AddRange(arrTrans.States);
            }
            else
            {
                inputs.Add(conn.BooleanState);
            }
        }
    }

    static bool EvaluateSingleMode(RelayHub hub, List<bool> input)
    {
        var len = input.Count;
        var notMode = hub.SingleModeNot;
        if (len == 0) { return notMode; }

        var result = EvaluateWithoutNot();
        return notMode ? !result : result;

        bool EvaluateWithoutNot()
        {
            var parameters = hub.SingleParameters;
            var mode = hub.SingleMode;

            switch (mode)
            {
                case RelayHubSingleMode.And:
                    for (int i = 0; i < len; i++)
                    {
                        if (!input[i])
                        {
                            return false;
                        }
                    }
                    return true;
                case RelayHubSingleMode.Or:
                    for (int i = 0; i < len; i++)
                    {
                        if (input[i])
                        {
                            return true;
                        }
                    }
                    return false;
                case RelayHubSingleMode.Index:
                    var index = parameters[0];
                    return index >= 0 && index < len && input[index];
                default: // Compare modes, MSB = index 0
                    var firstBit = GetFirstDifferentBit();
                    var eq = firstBit == null;
                    var a = firstBit?.Item1 ?? default;

                    return mode switch
                    {
                        RelayHubSingleMode.Eq => eq,
                        RelayHubSingleMode.Gt => !eq && a,
                        RelayHubSingleMode.Gte => eq || a,
                        RelayHubSingleMode.Lt => !eq && !a,
                        RelayHubSingleMode.Lte => eq || !a,
                        _ => throw mode.ThrowInvalid(),
                    };
            }

            (bool, bool)? GetFirstDifferentBit() // Assume working on two unsigned bytes, split and pad if odd: [a0, a1, a2, b0, b1, 0]
            {
                var mid = (len + 1) / 2;
                for (int i = mid - 1; i >= 0; i--)
                {
                    var a = input[i];
                    var b = mid + i < len && input[mid + i];

                    if (a != b) { return (a, b); }
                }

                return null;
            }
        }
    }

    static void EvaluateArrayMode(RelayHub hub, List<bool> input, List<bool> arrayOutput)
    {
        var parameters = hub.ArrayParameters;
        var mode = hub.ArrayMode;
        var inputLen = input.Count;

        EnsureOutputLength(inputLen, mode, parameters, arrayOutput);
        var outputLen = arrayOutput.Count;

        switch (mode)
        {
            case RelayHubArrayMode.Passthrough:
                FillExact();
                break;
            case RelayHubArrayMode.Reverse:
                for (int i = 0; i < inputLen; i++)
                {
                    arrayOutput[i] = input[inputLen - 1 - i];
                }
                break;
            case RelayHubArrayMode.Repeat:
                var index = 0;
                var repeatCount = parameters[0];
                for (int i = 0; i < repeatCount; i++)
                {
                    for (int j = 0; j < inputLen; j++)
                    {
                        arrayOutput[index++] = input[j];
                    }
                }
                break;
            case RelayHubArrayMode.Slice:
                var from = Math.Max(0, parameters[0]);
                var to = Math.Min(from + parameters[1], inputLen);

                for (int i = from; i < to; i++)
                {
                    arrayOutput[i - from] = input[i];
                }

                if (to - from < outputLen)
                {
                    for (int i = Math.Max(0, to - from); i < outputLen; i++)
                    {
                        arrayOutput[i] = false;
                    }
                }

                break;
            case RelayHubArrayMode.Shift:
                var shiftAmount = parameters[1];
                if (shiftAmount == 0)
                {
                    FillExact();
                }
                else if (shiftAmount >= inputLen)
                {
                    FillEmpty();
                }
                else
                {
                    var direction = parameters[0] == 0 ? 1 : -1; // 0 for left, 1 for right
                    for (int i = 0; i < outputLen; i++)
                    {
                        var fromIndex = i + direction * shiftAmount;
                        arrayOutput[i] = fromIndex >= 0 && fromIndex < inputLen && input[fromIndex];
                    }
                }

                break;
            case RelayHubArrayMode.And:
                SplitAndFill((a, b) => a && b);
                break;
            case RelayHubArrayMode.Or:
                SplitAndFill((a, b) => a || b);
                break;
            case RelayHubArrayMode.Xor:
                SplitAndFill((a, b) => a ^ b);
                break;
            default:
                throw hub.ArrayMode.ThrowInvalid();
        }

        if (hub.ArrayModeNot)
        {
            for (int i = 0; i < outputLen; i++)
            {
                arrayOutput[i] = !arrayOutput[i];
            }
        }

        // Split at middle and pad zero if non-even ([a0, a1, a2, b0, b1, 0])
        void SplitAndFill(Func<bool, bool, bool> operation)
        {
            var mid = (inputLen + 1) / 2;
            for (int i = 0; i < mid; i++)
            {
                var a = input[i];
                var b = mid + i < inputLen && input[mid + i];
                arrayOutput[i] = operation(a, b);
            }
        }

        void FillExact()
        {
            for (int i = 0; i < outputLen; i++)
            {
                arrayOutput[i] = input[i];
            }
        }

        void FillEmpty()
        {
            for (int i = 0; i < outputLen; i++)
            {
                arrayOutput[i] = false;
            }
        }
    }

    static void EnsureOutputLength(int inputLength, RelayHubArrayMode mode, int[] parameters, List<bool> arrayOutput)
    {
        var expectingLength = GetArrayOutputLength(inputLength, mode, parameters);
        if (arrayOutput.Count != expectingLength)
        {
            if (arrayOutput.Count > expectingLength)
            {
                arrayOutput.RemoveRange(expectingLength, arrayOutput.Count - expectingLength);
            }
            else
            {
                arrayOutput.Capacity = expectingLength;
                var adding = expectingLength - arrayOutput.Count;
                for (int i = 0; i < adding; i++)
                {
                    arrayOutput.Add(default);
                }
            }
        }
    }

    static int GetArrayOutputLength(int inputLength, RelayHubArrayMode mode, int[] parameters) => mode switch
    {
        RelayHubArrayMode.Passthrough or
        RelayHubArrayMode.Reverse or
        RelayHubArrayMode.Shift => inputLength,
        RelayHubArrayMode.Repeat => inputLength * parameters[0],
        RelayHubArrayMode.Slice => parameters[1],
        RelayHubArrayMode.And or
        RelayHubArrayMode.Or or
        RelayHubArrayMode.Xor => (inputLength + 1) / 2,
        _ => throw mode.ThrowInvalid(),
    };

    public static string[] GetParameters(RelayHubSingleMode mode) => mode switch
    {
        RelayHubSingleMode.Index => ["LV.AA.RelayHub.Parameter.Index"],
        RelayHubSingleMode.And or
        RelayHubSingleMode.Or or
        RelayHubSingleMode.Eq or
        RelayHubSingleMode.Gt or
        RelayHubSingleMode.Gte or
        RelayHubSingleMode.Lt or
        RelayHubSingleMode.Lte => [],
        _ => throw mode.ThrowInvalid(),
    };

    public static string[] GetParameters(RelayHubArrayMode mode) => mode switch
    {
        RelayHubArrayMode.Slice => ["LV.AA.RelayHub.Parameter.Start", "LV.AA.RelayHub.Parameter.Length"],
        RelayHubArrayMode.Shift => ["LV.AA.RelayHub.Parameter.ShiftDirection", "LV.AA.RelayHub.Parameter.ShiftAmount"],
        RelayHubArrayMode.Repeat => ["LV.AA.RelayHub.Parameter.RepeatCount"],
        RelayHubArrayMode.And or
        RelayHubArrayMode.Or or
        RelayHubArrayMode.Xor or
        RelayHubArrayMode.Passthrough or
        RelayHubArrayMode.Reverse => [],
        _ => throw mode.ThrowInvalid(),
    };

}
