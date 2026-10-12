using System;
namespace Bindito.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ContextAttribute : Attribute { public ContextAttribute(string context) { } }
    public interface IContainer
    {
        T GetInstance<T>();
    }

    public abstract class Configurator { public virtual void Configure() { } }
}
namespace Bindito.Core.Internal
{
    public enum Scope { Singleton, Transient }
}
