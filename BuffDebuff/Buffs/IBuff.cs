namespace BuffDebuff;

public interface IBuff : IBuffEntity
{
    string Name { get; }
    string Description { get; }
}