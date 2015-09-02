using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace UnitTestGrainInterfaces
{
    public interface IRequestContextTestGrain : IGrainWithIntegerKey
    {
        Task<string> TraceIdEcho();

        Task<string> TraceIdDoubleEcho();

        Task<string> TraceIdDelayedEcho1();

        Task<string> TraceIdDelayedEcho2();

        Task<Guid> E2EActivityId();
    }

    public interface IRequestContextTaskGrain : IGrainWithIntegerKey
    {
        Task<string> TraceIdEcho();

        Task<string> TraceIdDoubleEcho();

        Task<string> TraceIdDelayedEcho1();

        Task<string> TraceIdDelayedEcho2();

        Task<string> TraceIdDelayedEchoAwait();

        Task<string> TraceIdDelayedEchoTaskRun();

        Task<Guid> E2EActivityId();

        Task<Tuple<string, string>> TestRequestContext();
    }

    public interface IRequestContextProxyGrain : IGrainWithIntegerKey
    {
        Task<Guid> E2EActivityId();
    }
}
