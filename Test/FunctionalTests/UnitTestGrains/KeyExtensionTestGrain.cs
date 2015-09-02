using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;

using UnitTestGrainInterfaces;

namespace UnitTestGrains
{
    internal class KeyExtensionTestGrain : Grain, IKeyExtensionTestGrain
    {
        public Task<IKeyExtensionTestGrain> GetGrainReference()
        {
            return Task.FromResult(this.AsReference<IKeyExtensionTestGrain>());
        }

        public Task<ActivationId> GetActivationId()
        {
            return Task.FromResult(Data.ActivationId);
        }
    }
}
