using System.Threading.Tasks;
using Orleans;

namespace UnitTestGrainInterfaces
{
    public interface IExtensionTestGrain : IGrainWithIntegerKey
    {
        Task InstallExtension(string name);

        Task RemoveExtension();
    }

    public interface IGenericExtensionTestGrain<in T> : IGrainWithIntegerKey
    {
        Task InstallExtension(T name);

        Task RemoveExtension();
    }
}