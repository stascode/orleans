using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Placement;


namespace UnitTestGrainInterfaces
{
    internal interface IPlacementTestGrain : IGrainWithGuidKey
    {
        Task<IPEndPoint> GetEndpoint();
        Task<string> GetRuntimeInstanceId();
        Task<ActivationId> GetActivationId();
        Task StartLocalGrains(List<Guid> keys);
        Task<Guid> StartPreferLocalGrain(Guid key);
        Task<List<IPEndPoint>> SampleLocalGrainEndpoint(Guid key, int sampleSize);
        Task Nop();
        Task LatchOverloaded();
        Task UnlatchOverloaded();
        Task LatchCpuUsage(float value);
        Task UnlatchCpuUsage();
        Task<SiloAddress> GetLocation();
    }

    internal interface IActivationCountBasedPlacementTestGrain : IPlacementTestGrain
    { }

    internal interface IRandomPlacementTestGrain : IPlacementTestGrain
    { }

    internal interface IPreferLocalPlacementTestGrain : IPlacementTestGrain
    { }

    internal interface ILocalPlacementTestGrain : IPlacementTestGrain
    { }

    //----------------------------------------------------------//
    // Interfaces for LocalContent grain case, when grain is activated on every silo by bootstrap provider.

    public interface ILocalContentGrain : IGrainWithGuidKey
    {
        Task Init();                            // a dummy call to just activate this grain.
        Task<object> GetContent();
    }

    public interface ITestContentGrain : IGrainWithIntegerKey
    {
        Task<string> GetRuntimeInstanceId();    // just for test
        Task<object> FetchContentFromLocalGrain();
    }

}
