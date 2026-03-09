namespace AutomationArrays.Components;

public interface IArrayTransmitter : ITransmitter
{

    IReadOnlyList<bool> States { get; } 
    

}
