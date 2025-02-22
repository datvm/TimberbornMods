namespace BuffDebuff;

public interface IBuffTarget : IBuffEntity
{
    void UpdateTargets();

    bool TargetsChanged { get; }
    IEnumerable<BuffableComponent> Targets { get; }

    void Init();
    void CleanUp();
}
