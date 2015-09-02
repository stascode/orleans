﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;



namespace UnitTestGrainInterfaces
{
    public interface IDeadlockNonReentrantGrain : IGrainWithIntegerKey
    {
        Task CallNext_1(List<Tuple<long, bool>> callChain, int currCallIndex);
        Task CallNext_2(List<Tuple<long, bool>> callChain, int currCallIndex);
    }

    public interface IDeadlockReentrantGrain : IGrainWithIntegerKey
    {
        Task CallNext_1(List<Tuple<long, bool>> callChain, int currCallIndex);
        Task CallNext_2(List<Tuple<long, bool>> callChain, int currCallIndex);
    }
}
