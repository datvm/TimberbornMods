namespace SyncableSettings.Models;

public record PlayerPrefEntry(string Key, string Value, PlayerPrefEntryType Type)
{
}

public enum PlayerPrefEntryType
{
    String,
    Int,
    Float,
}