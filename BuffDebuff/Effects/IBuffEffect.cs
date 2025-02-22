namespace BuffDebuff;

public interface IBuffEffect : IBuffEntity
{

    string Description { get; }

    void Init();
    void CleanUp();

    void UpdateEffect();

}
