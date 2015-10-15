using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using demo.interfaces;

namespace frontend.Controllers
{
    public class CallMethodController : ApiController
    {
        public async Task<object> Get(long grainid, string methodName,  string args)
        {
            try
            {
                IGrain1 g = Grain1Factory.GetGrain(grainid);
                object o = await g.CallFunctionCallback(methodName, args);
                return o;
            }
            catch (Exception e)
            {
                throw e;
            }            
        }
    }
}
