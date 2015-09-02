using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.CodeGeneration;
using Orleans.Providers;

namespace MultifacetGrain
{
    public class MultifacetTestGrainState : GrainState
    {
        public int Value { get; set; }
    }

    [StorageProvider(ProviderName = "MemoryStore")]
    public class MultifacetTestGrain : Grain<MultifacetTestGrainState>, IMultifacetTestGrain
    {
        
        public string GetRuntimeInstanceId()
        {
            return this.RuntimeIdentity;
        }

        #region IMultifacetWriter Members

        public Task SetValue(int x)
        {
            State.Value = x;
            return TaskDone.Done;
        }

        #endregion

        #region IMultifacetReader Members

        Task<int> IMultifacetReader.GetValue()
        {
            return Task.FromResult(State.Value);
        }

        #endregion
    }
}
