// This file is not needed for Orleans.js. It was a part of original unit tests.
using System;
using System.Threading.Tasks;
using demo.interfaces;

namespace demo.host
{
    /// <summary>
    /// Orleans test silo host
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            //AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            //{
            //    AppDomainInitializer = InitSilo,
            //    AppDomainInitializerArguments = args,
            //});
            Console.ReadLine();
            Orleans.OrleansClient.Initialize("DevTestClientConfiguration.xml");
            demo.interfaces.IGrain1 g = demo.interfaces.Grain1Factory.GetGrain(1);
            Console.WriteLine(g.SampleGrainMethod("haha").Result);
            string a = true.ToString();
            //g.Init("");
            //Console.WriteLine(g.CallAsyncFunction("add", "5,6").Result);
            //Console.WriteLine(g.CallAsyncFunction("fact", "6").Result);
            //Console.WriteLine(g.CallAsyncFunction("fact", "6").Result);
            Console.WriteLine(g.CallFunctionCallback("callmethodonothergraintwice", "4").Result);

            int dimX = 8;
            int N = dimX * dimX;
            demo.interfaces.IGrain1[] grains = new IGrain1[N];
            Task[] tasks = new Task[N];
            bool[] states = new bool[N];
            for (int i = 0; i < N; i++)
            {
                grains[i] = Grain1Factory.GetGrain(i);
            }

            IGrain1 gameGrain = Grain1Factory.GetGrain(1000000);

            gameGrain.CallFunctionCallback("Init", "").Wait();

            int count = 0;

            while (count < 10000)
            {
                count++;
                //gameGrain.CallFunctionCallback("Tick", "").Wait();
                //gameGrain.CallFunctionCallback("UpdateStates","").Wait();

                for (int i = 0; i < N; i++)
                {
                    //tasks[i] = grains[i].CallFunctionCallback("UpdateLiveNeighbourCount", "");
                    grains[i].CallFunctionCallback("UpdateLiveNeighbourCount", "").Wait();
                }
                // Task.WaitAll(tasks);
                // Todo: move this to CellGrain as a callback

                for (int i = 0; i < N; i++)
                {
                    tasks[i] = grains[i].CallFunctionCallback("UpdateMyState", "");
                    //grains[i].CallFunctionCallback("UpdateMyState", "").Wait();
                }
                Task.WaitAll(tasks);

                for (int i = 0; i < N; i++)
                {
                    states[i] = (bool)grains[i].CallFunctionCallback("GetMyState", "").Result;
                }

                Console.Clear();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.Write("                           ");
                for (int i = 0; i < N; i++)
                {

                    Console.Write(states[i] ? "1 " : "0 ");
                    if (i % dimX == dimX - 1)
                    {
                        Console.WriteLine();
                        Console.Write("                           ");
                    }
                }
            }

            // TODO: once the previous call returns, the silo is up and running.
            //       This is the place your custom logic, for example calling client logic
            //       or initializing an HTTP front end for accepting incoming requests.

            Console.WriteLine("Orleans Silo is running.\nPress Enter to terminate...");
            Console.ReadLine();

            //hostDomain.DoCallBack(ShutdownSilo);
        }

        static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper(args);

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        static void ShutdownSilo()
        {
            if (hostWrapper != null)
            {
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }

        private static OrleansHostWrapper hostWrapper;
    }
}
