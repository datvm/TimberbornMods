namespace ModdableTimberborn.GameStats;

public interface IGameStatProvider
{
    IEnumerable<string> AvailableStats { get; }
    Type OutputType { get; }
    object? GetStat(string statId);
    
    string GetStatFormatted(string statId) => GetStat(statId)?.ToString() ?? "N/A";
}

public interface IGameStatProvider<T> : IGameStatProvider
{
    Type IGameStatProvider.OutputType => typeof(T);
    new T? GetStat(string statId);
    object? IGameStatProvider.GetStat(string statId) => GetStat(statId);
}

public interface IIntGameStatProvider : IGameStatProvider<int>;
public interface IFloatGameStatProvider : IGameStatProvider<float>;
public interface IStringGameStatProvider : IGameStatProvider<string>;
public interface ISpriteGameStatProvider : IGameStatProvider<Sprite?>;

public interface IPercentGameStatProvider : IGameStatProvider<float>
{
    string IGameStatProvider.GetStatFormatted(string statId)
    {
        var value = GetStat(statId);
        return value >= 0 ? value.ToString("P0") : "N/A";
    }
}