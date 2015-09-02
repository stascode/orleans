using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using UnitTests.GrainInterfaces;

namespace UnitTestGrains
{
    public interface IMasterGrain : IGrainWithIntegerKey
    {
        Task Initialize(IWorkerGrain[] workers);
        Task DoWork(int numItemsPerWorker, int itemLenght);
    }

    public interface IWorkerGrain : IGrainWithIntegerKey
    {
        Task Initialize(IAggregatorGrain aggregator);
        Task DoWork(int numItems, int itemLenght);
    }

    public interface IAggregatorGrain : IGrainWithIntegerKey
    {
        Task Initialize(int numWorkers);
        Task TakeResult(double result);
        Task<List<double>> GetResults();
    }
}
