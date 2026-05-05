namespace DynamicTailsBanners.Services;

public interface IConnectedDynamicDecal : IDynamicDecalProvider
{
    int ExpectedConnectionCount { get; }
}
