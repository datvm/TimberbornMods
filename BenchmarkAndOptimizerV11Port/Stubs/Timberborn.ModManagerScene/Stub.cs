namespace Timberborn.ModManagerScene
{
    public interface IModStarter
    {
        void StartMod(IModEnvironment modEnvironment);
    }

    public interface IModEnvironment
    {
        string ModPath { get; }
    }
}
