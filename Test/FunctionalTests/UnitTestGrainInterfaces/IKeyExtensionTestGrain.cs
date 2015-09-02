using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;


namespace UnitTestGrainInterfaces
{
    internal interface IKeyExtensionTestGrain : IGrainWithGuidCompoundKey
    {
        Task<IKeyExtensionTestGrain> GetGrainReference();
        Task<ActivationId> GetActivationId();
    }
}
