namespace ThePostOffice.Components;

public interface IPostOfficeJob
{
    bool IsRunnable { get; }

    void Progress(float hours);
}
