using demo.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainCollection1
{
    public class OrleansDotNet
    {
        public static async Task<object> CallMethodOnOtherGrain(int grainId, string funcName, object args)
        {
            IGrain1 g = Grain1Factory.GetGrain(grainId);
            object result = await g.CallFunctionCallback(funcName, args.ToString());
            return result;
        }
    }
}
