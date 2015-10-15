using demo.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace frontend.Controllers
{
    public class InitController : ApiController
    {
        public async void Put(int id, [FromBody]string value)
        {
            //string json = grainCode;
            //var task = this.Request.Content.ReadAsFormDataAsync();
            //task.Wait();
            //var grainCode = task.Result;
            await Grain1Factory.GetGrain(id).Init(value, false);
        }
    }
}
