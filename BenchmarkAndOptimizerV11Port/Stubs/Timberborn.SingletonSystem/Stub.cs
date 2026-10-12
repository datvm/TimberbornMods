namespace Timberborn.SingletonSystem
{
    public interface ILoadableSingleton
    {
        void Load();
    }

    public interface IUnloadableSingleton
    {
        void Unload();
    }

    public interface IPostLoadableSingleton
    {
        void PostLoad();
    }

    public interface IUpdatableSingleton
    {
        void UpdateSingleton();
    }
}
