using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace UnitTestGrainInterfaces
{
    public interface IClientAddressableTestConsumer : IGrainWithIntegerKey
    {
        Task<int> PollProducer();
        Task Setup();
    }
}
