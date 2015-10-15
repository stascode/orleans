// This class encapsulates tasks as promises, so that the javascript code can be written as if using promises. 
// Orleans returns Tasks for every method call, that task can be passed to the constructor here and a Promise can be constructed
// Chaining and joining promises is allowed. 
// Visit http://promisesaplus.com/ for the details on Promises proposal for ECMAScript.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainCollection1
{
    // A cheap Promise implementation, some functionality missing, will add on need basis. 

    public class Promise
    {
        private enum _taskType { value, nothing };
        private int _tType;
        private Task<object> _baseTask;
        private Task _baseVoidTask;
        public Task<object> BaseTask { get { return _baseTask; } }
        public Task BaseVoidTask { get { return _baseVoidTask; } }

        private bool _parentRejected = false;

        public Promise(dynamic a)
        {
            _tType = (int)_taskType.value;
            _baseTask = Task.Factory.StartNew<object>(() => { return a(); });
        }

        public Promise(Task<object> t)
        {
            _tType = (int)_taskType.value;
            _baseTask = t;
        }

        public Promise(Task t)
        {
            _tType = (int)_taskType.nothing;
            _baseVoidTask = t;
        }

        public async Task<object> Result()
        {
            if (_baseTask == null)
            {
                throw new Exception("Promise doesn't not hold any value.");
            }
            return await _baseTask;
        }

        public Promise then(dynamic onFulfilled = null, dynamic onRejected = null)
        {
            Promise retVal = null;
            try
            {
                if (!_parentRejected)
                {
                    if (onFulfilled != null)
                    {
                        if (_tType == 0)
                        {
                            retVal = new Promise(_baseTask.ContinueWith((task) => { return onFulfilled(task); }));                            
                        }
                        else
                        {
                            retVal = new Promise(_baseVoidTask.ContinueWith((task) => { return onFulfilled(task); }));
                        }
                    }
                }
                else
                {
                    retVal = new Promise(Task.FromResult<object>("The Parent promise was rejected.")) { _parentRejected = true };
                }
            }
            catch (Exception e)
            {
                if (onRejected != null)
                {
                    onRejected(Task.FromResult<object>(e.Message));
                }
                retVal = new Promise(Task.FromResult<object>(e.Message)) { _parentRejected = true };
            }
            return retVal;
        }

        public static Promise join(dynamic promises)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < promises.length; i++)
            {    
                var promise = Convert.ChangeType(promises[i], typeof(Promise));
                if (promise.BaseTask == null)
                {
                    tasks.Add(promise.BaseVoidTask);
                }
                else
                {
                    tasks.Add(promise.BaseTask);
                }
            }

            return new Promise(Task.WhenAll(tasks));
        }
    }
}