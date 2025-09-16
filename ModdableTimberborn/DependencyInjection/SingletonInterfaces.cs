namespace ModdableTimberborn.DependencyInjection;

public interface ILoadableSingletonFrontRunner<TService>
    where TService : ILoadableSingleton
{
    void FrontRun(TService service);
}

public interface ILoadableSingletonTailRunner<TService>
    where TService : ILoadableSingleton
{
    void TailRun(TService service);
}

public interface IPostLoadableSingletonFrontRunner<TService>
    where TService : IPostLoadableSingleton
{
    void FrontRun(TService service);
}

public interface IPostLoadableSingletonTailRunner<TService>
    where TService : IPostLoadableSingleton
{
    void TailRun(TService service);
}
