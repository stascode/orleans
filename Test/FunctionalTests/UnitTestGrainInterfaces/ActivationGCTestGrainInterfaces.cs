using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

namespace UnitTestGrainInterfaces
{
    public interface IIdleActivationGcTestGrain1 : IGrainWithGuidKey
    {
        Task Nop();
    }

    public interface IIdleActivationGcTestGrain2 : IGrainWithGuidKey
    {
        Task Nop();
    }

    public interface IBusyActivationGcTestGrain1 : IGrainWithGuidKey
    {
        Task Nop();
        Task Delay(TimeSpan dt);
        Task<string> IdentifyActivation();
        Task EnableBurstOnCollection(int count);
    }

    public interface IBusyActivationGcTestGrain2 : IGrainWithGuidKey
    {
        Task Nop();
    }

    public interface IStatelessWorkerActivationCollectorTestGrain1 : IGrainWithGuidKey
    {
        Task Nop();
        Task Delay(TimeSpan dt);
        Task<string> IdentifyActivation();
    }
}
