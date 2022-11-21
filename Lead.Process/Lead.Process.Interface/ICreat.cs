
using Lead.Proxy;

namespace Lead.Process.Interface
{
    public interface ICreatPorcess
    {
        string Name { get; }

        IProcess CreatInstance(ProxyData Data);
    }
}
