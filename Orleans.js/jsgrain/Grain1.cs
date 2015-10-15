using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Providers;
using Orleans.Concurrency;
using demo.interfaces;
using Jurassic;
using Jurassic.Library;
using CS = Microsoft.ClearScript;
using CSV8 = Microsoft.ClearScript.V8;
using System.Collections;
using Microsoft.CSharp.RuntimeBinder;

namespace GrainCollection1
{
    /// <summary>
    /// Orleans grain implementation class Grain1.
    /// </summary>
    public interface JSSource : Orleans.IGrainState
    {
        string jssource { get; set; }
        bool isfile { get; set; }
    }

    [StorageProvider(ProviderName = "MemoryStore")]
    [Reentrant]
    public class Grain1 : Orleans.GrainBase<JSSource>, IGrain1
    {
        private CSV8.V8ScriptEngine V8Engine;

        private List<Task<object>> tasks;
        private List<string> callbacks;
        private List<string> callBackArgsList;

        private Task<object> callbackTask;

        public override async Task ActivateAsync()
        {
            await Setup();
        }

        private async Task Setup()
        {
            if (State.jssource == null)
            {
                State.isfile = true;
                State.jssource = @"F:\Orleans-Shrainik\Shrainik\Orleans.js\jsgrain\cellgrain.js";
                await State.WriteStateAsync();
            }

            if (callbacks == null)
            {
                callbacks = new List<string>();
            }

            if (tasks == null)
            {
                tasks = new List<Task<object>>();
            }

            if (callBackArgsList == null)
            {
                callBackArgsList = new List<string>();
            }

            await State.ReadStateAsync();
            V8Engine = new CSV8.V8ScriptEngine { AccessContext = typeof(Grain1) };
            V8Engine.AddHostType("Promise", typeof(Promise));
            V8Engine.AddHostType("Console", typeof(Console));
            V8Engine.AddHostObject("lib", CS.HostItemFlags.GlobalMembers, new CS.HostTypeCollection("mscorlib"));

            V8Engine.AddHostObject("CallMethodOnOtherGrain", new Func<int, string, string, Promise>(CallMethodOnOtherGrain));
            V8Engine.AddHostObject("ExecuteFunctionOnOtherGrain", new Func<int, string, string, bool>((grainid, funcName, args) =>
            {
                ExecuteFunctionOnOtherGrain(grainid, funcName, args);
                return true;
            }));
            V8Engine.AddHostObject("GetKey", new Func<long>(this.GetPrimaryKeyLong));
            V8Engine.Execute("var myId = GetKey()");
            V8Engine.Execute("var Callback = System.Action(System.Threading.Tasks.Task(System.Object));");
            V8Engine.AddHostObject("host", new CS.HostFunctions());

            V8Engine.AddHostObject("CallMethodOnOtherGrainNew", new Func<int, string, string, string, string, bool>(CallMethodOnOtherGrainWithCallback));
            V8Engine.AddHostObject("CallMethod", new Func<int, string, string, dynamic, dynamic, bool>((grainid, funcName, args, callback, callbackArgs) =>
            {
                CallMethod(grainid, funcName, args, callback, callbackArgs);
                return true;
            }));

            if (State.isfile)
            {
                V8Engine.Execute(File.ReadAllText(State.jssource));
            }
            else
            {
                V8Engine.Execute(State.jssource);
            }
        }

        // Intergrain communication, Method 1: Marshalling Tasks as Promises. 
        private Promise CallMethodOnOtherGrain(int grainid, string funcName, string args)
        {
            IGrain1 g = Grain1Factory.GetGrain(grainid);
            return new Promise(g.CallFunction(funcName, args));
        }

        private void ExecuteFunctionOnOtherGrain(int grainid, string funcName, string args)
        {
            IGrain1 g = Grain1Factory.GetGrain(grainid);
            g.ExecuteJSFunction(funcName, args);
        }

        // Intergrain communication, Method 2: Maintaining a callback stack.
        private bool CallMethodOnOtherGrainWithCallback(int grainid, string funcName, string args, string callback, string callbackArgs)
        {
            IGrain1 g = Grain1Factory.GetGrain(grainid);
            tasks.Add(g.CallFunctionCallback(funcName, args));
            callbacks.Add(callback);
            callBackArgsList.Add(callbackArgs);
            return true;
        }

        // Intergrain communication, Method 2: Maintaining await and callback.
        private async Task<bool> CallMethod(int grainid, string funcName, string args, dynamic callback, dynamic callbackArgs)
        {
            IGrain1 g = Grain1Factory.GetGrain(grainid);
            object result = await g.CallFunctionCallback(funcName, args);
            string resultarg = "";
            if (result is bool)
            {
                resultarg = result.ToString().ToLower();
            }
            else
            {
                resultarg = result.ToString();
            }
            callback(result, callbackArgs);
            //string evalString = string.Format("{0}({1},{2});", callback, resultarg, callbackArgs);
            //V8Engine.Evaluate(evalString);
            return true;
        }

        public async Task Init(string jsSource, bool isfile = false)
        {
            State.isfile = isfile;
            State.jssource = jsSource;
            await State.WriteStateAsync();
            await Setup();
        }

        public Task<object> CallFunction(string funcName, string args)
        {
            string evalString = string.Format("{0}({1});", funcName, args);
            object result = V8Engine.Evaluate(evalString);
            while (true)
            {
                if (callbackTask == null)
                    break;
                Task<object> toReturn = callbackTask;
                callbackTask = null;
                return toReturn;
            }
            return Task.FromResult<object>(result);
        }

        public Task ExecuteJSFunction(string funcName, string args)
        {
            string evalString = string.Format("{0}({1});", funcName, args);
            V8Engine.Execute(evalString);
            return TaskDone.Done;
        }

        public async Task<object> CallFunctionCallback(string funcName, string args)
        {
            string evalString = string.Format("{0}({1});", funcName, args);
            object result = V8Engine.Evaluate(evalString);
            while (true)
            {
                if (tasks.Count == 0)
                    break;
                object temp = await tasks[0];
                string resultarg = "";
                if (temp is bool)
                {
                    resultarg = temp.ToString().ToLower();
                }
                else
                {
                    resultarg = temp.ToString();
                }
                string call = callBackArgsList[0] == ""
                    ? string.Format("{0}({1});", callbacks[0], resultarg)
                    : string.Format("{0}({1},{2});", callbacks[0], resultarg, callBackArgsList[0]);
                result = V8Engine.Evaluate(call);
                tasks.RemoveAt(0);
                callbacks.RemoveAt(0);
                callBackArgsList.RemoveAt(0);
            }
            if (result is CS.Undefined || result is Undefined)
                return true;
            if (result is IEnumerable)
            {

            }
            var dResult = result as dynamic;
            if (!(dResult is bool || dResult is int || dResult is string || dResult is float || dResult is double || dResult is short || dResult is Int32 || dResult is long))
            {
                if (dResult.length > 0)
                {
                    var returnArr = new object[dResult.length];
                    for (int i = 0; i < dResult.length; i++)
                    {
                        returnArr[i] = dResult[i];
                    }
                    return returnArr;
                }
            }
            return result;
        }

        public Task<object> CallAsyncFunction(string funcName, string args)
        {
            return Task.Factory.StartNew<object>(() =>
            {
                string evalString = string.Format("{0}({1});", funcName, args);
                object result = V8Engine.Evaluate(evalString);
                return result;
            });
        }

        public Task<string> SampleGrainMethod(string input)
        {
            return Task.FromResult("Hi, This works!\n" + "You Said: " + input);
        }
    }
}
