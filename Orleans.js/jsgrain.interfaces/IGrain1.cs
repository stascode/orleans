using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;

namespace demo.interfaces
{
    /// <summary>
    /// Orleans grain communication interface IGrain1
    /// </summary>
    public interface IGrain1 : Orleans.IGrain
    {
        Task Init(string jsSource, bool isfile = false);

        Task<object> CallFunction(string funcName, string args);

        Task<object> CallFunctionCallback(string funcName, string args);

        Task<object> CallAsyncFunction(string funcName, string args);
        
        Task ExecuteJSFunction(string funcName, string args);

        Task<string> SampleGrainMethod(string input);
    }
}
