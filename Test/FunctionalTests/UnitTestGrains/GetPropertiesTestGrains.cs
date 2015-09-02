using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestGrainInterfaces;
using Orleans;
using Orleans.CodeGeneration;

namespace UnitTestGrains
{
    public class ComplexGrainState : GrainState
    {
        public ComplicatedTestType<int> FldInt { get; set; }
        public ComplicatedTestType<string> FldStr { get; set; }
    }

    [Orleans.Providers.StorageProvider(ProviderName = "MemoryStore")]
    public class ComplexGrain : Grain<ComplexGrainState>, IComplexGrain
    {
        public Task SeedFldInt(int i)
        {
            State.FldInt.InitWithSeed(i);
            return TaskDone.Done;
        }
        public Task SeedFldStr(string s)
        {
            State.FldStr.InitWithSeed(s);
            return TaskDone.Done;
        }
        public override Task OnActivateAsync()
        {
            State.FldInt = new ComplicatedTestType<int>();
            State.FldStr = new ComplicatedTestType<string>();
            return TaskDone.Done;
        }

        public Task<ComplicatedTestType<int>> GetFldInt()
        {
           return Task.FromResult(State.FldInt);
        }

        public Task<ComplicatedTestType<string>> GetFldStr()
        {
            return Task.FromResult(State.FldStr);
        }
    }
    public class LinkedListGrainState : GrainState
    {
        public ILinkedListGrain Next { get; set; }
        public int Value { get; set; }
    }

    [Orleans.Providers.StorageProvider(ProviderName = "MemoryStore")]
    public class LinkedListGrain : Grain<LinkedListGrainState>, ILinkedListGrain
    {
        public Task SetValue(int v)
        {
            State.Value = v;
            return TaskDone.Done;
        }
        public Task SetNext(ILinkedListGrain next)
        {
            State.Next = next;
            return TaskDone.Done;
        }

        public Task<ILinkedListGrain> GetNext()
        {
            return Task.FromResult(State.Next);
        }

        public Task<int> GetValue()
        {
            return Task.FromResult(State.Value);
        }
    }

}
