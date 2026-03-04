namespace MoreHttpApi.Shared;

public record HttpEntityModel(Guid EntityId, HttpEntityState State);

public enum HttpEntityState
{
    Uninitialized,
    Initializing,
    Initialized,
    Deleted
}