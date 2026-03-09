namespace AutomationArrays.Helpers;

public static class AutomationHelpers
{

    extension(Automator automator)
    {
        public bool IsArrayTransmitter([NotNullWhen(true)] out IArrayTransmitter? transmitter)
            => (transmitter = automator as IArrayTransmitter) is not null;
    }

    extension<T>(T value) where T : Enum
    {

        public ArgumentOutOfRangeException ThrowInvalid()
        {
            throw new ArgumentOutOfRangeException($"Value {value} is not valid for enum type {typeof(T).Name}");
        }

    }

}
