#define USE_STORAGE
//#define USE_CAST
#define COUNT_ACTIVATE_DEACTIVATE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.CodeGeneration;
using Orleans.Runtime;
using Orleans.Runtime.Providers;
using Orleans.Streams;
using UnitTestGrainInterfaces;

namespace UnitTestGrains
{
    public class StreamLifecycleTestGrainState : GrainState
    {
        // For producer and consumer 
        // -- only need to store this because of how we run our unit tests against multiple providers
        public string StreamProviderName { get; set; }

        // For producer only.
        public IAsyncStream<int> Stream { get; set; }
        public bool IsProducer { get; set; }
        public int NumMessagesSent { get; set; }
        public int NumErrors { get; set; }

        // For consumer only.
        public HashSet<StreamSubscriptionHandle<int>> ConsumerSubscriptionHandles { get; set; }

        public override void SetAll(IDictionary<string, object> values)
        {
            base.SetAll(values);
            if(ConsumerSubscriptionHandles == null)
                ConsumerSubscriptionHandles = new HashSet<StreamSubscriptionHandle<int>>();
        }
    }

    [Orleans.Providers.StorageProvider(ProviderName = "MemoryStore")]
    public class StreamLifecycleTestGrainBase : Grain<StreamLifecycleTestGrainState>
    {
        private const string StreamNamespace = UnitTestStreamNamespace.StreamLifecycleTestsNamespace;

        protected Logger logger;
        protected string _lastProviderName;
        protected IStreamProvider _streamProvider;

#if COUNT_ACTIVATE_DEACTIVATE
        private IActivateDeactivateWatcherGrain watcher;
#endif

        protected Task RecordActivate()
        {
#if COUNT_ACTIVATE_DEACTIVATE
            watcher = GrainFactory.GetGrain<IActivateDeactivateWatcherGrain>(0);
            return watcher.RecordActivateCall(Data.ActivationId);
#else
            return TaskDone.Done;
#endif
        }

        protected Task RecordDeactivate()
        {
#if COUNT_ACTIVATE_DEACTIVATE
            return watcher.RecordDeactivateCall(Data.ActivationId);
#else
            return TaskDone.Done;
#endif
        }

        protected void InitStream(Guid streamId, string providerName)
        {
            if (streamId == null) throw new ArgumentNullException("streamId", "Can't have null stream id");
            if (providerName == null) throw new ArgumentNullException("providerName", "Can't have null stream provider name");

            if (State.Stream != null && State.Stream.Guid != streamId)
            {
                if (logger.IsVerbose)
                    logger.Verbose("Stream already exists for StreamId={0} StreamProvider={1} - Resetting", State.Stream, providerName);

                // Note: in this test, we are deliberately not doing Unsubscribe consumers, just discard old stream and let auto-cleanup functions do their thing.
                InternalRemoveConsumerHandles();
                State.ConsumerSubscriptionHandles.Clear();
                State.IsProducer = false;
                State.NumMessagesSent = 0;
                State.NumErrors = 0;
                State.Stream = null;
            }

            if (logger.IsVerbose)
                logger.Verbose("InitStream StreamId={0} StreamProvider={1}", streamId, providerName);

            if (providerName != _lastProviderName)
            {
                _streamProvider = GetStreamProvider(providerName);
                _lastProviderName = providerName;
            }
            IAsyncStream<int> stream = _streamProvider.GetStream<int>(streamId, StreamNamespace);
            State.Stream = stream;
            State.StreamProviderName = providerName;

            if (logger.IsVerbose)
                logger.Verbose("InitStream returning with Stream={0} with ref type = {1}", State.Stream,
                    State.Stream.GetType().FullName);
        }

        private void InternalRemoveConsumerHandles()
        {
            List<StreamSubscriptionHandle<int>> subsHandles = State.ConsumerSubscriptionHandles.ToList();
            var s = State.Stream as StreamImpl<int>;
            var c = s.GetConsumerInterface() as StreamConsumer<int>;
            foreach (var handle in subsHandles)
            {
                c.InternalRemoveObserver(handle);
            }
        }
    }

    [Orleans.Providers.StorageProvider(ProviderName = "MemoryStore")]
    public class StreamLifecycleConsumerGrain : StreamLifecycleTestGrainBase, IStreamLifecycleConsumerGrain
    {
        protected IDictionary<StreamSubscriptionHandle<int>, MyStreamObserver<int>> Observers { get; set; }

        public override async Task OnActivateAsync()
        {
            logger = GetLogger(GetType().Name + "-" + IdentityString);
            if (logger.IsVerbose)
                logger.Verbose("OnActivateAsync");

            await RecordActivate();

            if (Observers == null)
            {
                Observers = new Dictionary<StreamSubscriptionHandle<int>, MyStreamObserver<int>>();
            }

            if (State.Stream != null && State.StreamProviderName != null)
            {
                if (State.ConsumerSubscriptionHandles.Count > 0)
                {
                    var handles = State.ConsumerSubscriptionHandles.ToArray();
                    if (logger.IsVerbose)
                        logger.Verbose("ReconnectConsumerHandles SubscriptionHandles={0} Grain={1}", Utils.EnumerableToString(handles), this.AsReference<IStreamLifecycleConsumerGrain>());
                    foreach (var handle in handles)
                    {
                        var observer = new MyStreamObserver<int>(logger);
                        StreamSubscriptionHandle<int> subsHandle = await handle.ResumeAsync(observer);
                        Observers.Add(subsHandle, observer);
                    }
                }
            }
            else
            {
                if (logger.IsVerbose)
                    logger.Verbose("Not conected to stream yet.");
            }
        }
        public override async Task OnDeactivateAsync()
        {
            if (logger.IsVerbose)
                logger.Verbose("OnDeactivateAsync");
            await RecordDeactivate();
        }

        public Task<int> GetReceivedCount()
        {
            int numReceived = Observers.Sum(o => o.Value.NumItems);
            if (logger.IsVerbose)
                logger.Verbose("ReceivedCount={0}", numReceived);
            return Task.FromResult(numReceived);
        }
        public Task<int> GetErrorsCount()
        {
            int numErrors = Observers.Sum(o => o.Value.NumErrors);
            if (logger.IsVerbose)
                logger.Verbose("ErrorsCount={0}", numErrors);
            return Task.FromResult(numErrors);
        }

        public Task Ping()
        {
            return TaskDone.Done;
        }

        public virtual async Task BecomeConsumer(Guid streamId, string providerName)
        {
            if (logger.IsVerbose)
                logger.Verbose("BecomeConsumer StreamId={0} StreamProvider={1} Grain={2}", streamId, providerName, this.AsReference<IStreamLifecycleConsumerGrain>());
            InitStream(streamId, providerName);
            var observer = new MyStreamObserver<int>(logger);
            var subsHandle = await State.Stream.SubscribeAsync(observer);
            State.ConsumerSubscriptionHandles.Add(subsHandle);
            Observers.Add(subsHandle, observer);
#if USE_STORAGE
            await WriteStateAsync();
#endif
        }

        public virtual async Task TestBecomeConsumerSlim(Guid streamIdGuid, string providerName)
        {
            InitStream(streamIdGuid, providerName);
            var observer = new MyStreamObserver<int>(logger);

            //var subsHandle = await State.Stream.SubscribeAsync(observer);

            IStreamConsumerExtension myExtensionReference;
#if USE_CAST
            myExtensionReference = StreamConsumerExtensionFactory.Cast(this.AsReference());
#else
            var tup = await SiloProviderRuntime.Instance.BindExtension<StreamConsumerExtension, IStreamConsumerExtension>(
                        () => new StreamConsumerExtension(SiloProviderRuntime.Instance));
            StreamConsumerExtension myExtension = tup.Item1;
            myExtensionReference = tup.Item2;
#endif
            string extKey = providerName + "_" + State.Stream.Namespace;
            IPubSubRendezvousGrain pubsub = GrainFactory.GetGrain<IPubSubRendezvousGrain>(streamIdGuid, extKey, null);
            GuidId subscriptionId = GuidId.GetNewGuidId();
            await pubsub.RegisterConsumer(subscriptionId, ((StreamImpl<int>)State.Stream).StreamId, myExtensionReference, null);

            myExtension.SetObserver(subscriptionId, ((StreamImpl<int>)State.Stream), observer, null, null);
        }

        public async Task RemoveConsumer(Guid streamId, string providerName, StreamSubscriptionHandle<int> subsHandle)
        {
            if (logger.IsVerbose)
                logger.Verbose("RemoveConsumer StreamId={0} StreamProvider={1}", streamId, providerName);
            if (State.ConsumerSubscriptionHandles.Count == 0) throw new InvalidOperationException("Not a Consumer");
            await subsHandle.UnsubscribeAsync();
            Observers.Remove(subsHandle);
            State.ConsumerSubscriptionHandles.Remove(subsHandle);
#if USE_STORAGE
            await WriteStateAsync();
#endif
        }

        public async Task ClearGrain()
        {
            if (logger.IsVerbose)
                logger.Verbose("ClearGrain");
            var subsHandles = State.ConsumerSubscriptionHandles.ToArray();
            foreach (var handle in subsHandles)
            {
                await handle.UnsubscribeAsync();
            }
            State.ConsumerSubscriptionHandles.Clear();
            State.Stream = null;
            State.IsProducer = false;
            Observers.Clear();
#if USE_STORAGE
            await ClearStateAsync();
#endif
        }
    }

    [Orleans.Providers.StorageProvider(ProviderName = "MemoryStore")]
    public class FilteredStreamConsumerGrain : StreamLifecycleConsumerGrain, IFilteredStreamConsumerGrain
    {
        private static Logger _logger;

        private const Int32 FilterDataOdd = 1;
        private const Int32 FilterDataEven = 2;

        public override Task BecomeConsumer(Guid streamId, string providerName)
        {
            throw new InvalidOperationException("Should not be calling unfiltered BecomeConsumer method on " + GetType());
        }
        public async Task BecomeConsumer(Guid streamId, string providerName, bool sendEvensOnly)
        {
            _logger = logger;
            if (logger.IsVerbose)
                logger.Verbose("BecomeConsumer StreamId={0} StreamProvider={1} Filter={2} Grain={3}",
                streamId, providerName, sendEvensOnly, this.AsReference<IFilteredStreamConsumerGrain>());
            InitStream(streamId, providerName);

            var observer = new MyStreamObserver<int>(logger);

            StreamFilterPredicate filterFunc;
            object filterData;
            if (sendEvensOnly)
            {
                filterFunc = FilterIsEven;
                filterData = FilterDataEven;
            }
            else
            {
                filterFunc = FilterIsOdd;
                filterData = FilterDataOdd;
            }

            var subsHandle = await State.Stream.SubscribeAsync(observer, null, filterFunc, filterData);

            State.ConsumerSubscriptionHandles.Add(subsHandle);
            Observers.Add(subsHandle, observer);
#if USE_STORAGE
            await WriteStateAsync();
#endif
        }

        public async Task SubscribeWithBadFunc(Guid streamId, string providerName)
        {
            if (logger.IsVerbose)
                logger.Verbose("SubscribeWithBadFunc StreamId={0} StreamProvider={1}Grain={2}",
                streamId, providerName, this.AsReference<IFilteredStreamConsumerGrain>());

            InitStream(streamId, providerName);

            var observer = new MyStreamObserver<int>(logger);

            StreamFilterPredicate filterFunc = BadFunc;

            // This next call should fail because func is not static
            await State.Stream.SubscribeAsync(observer, null, filterFunc);
        }

        public static bool FilterIsEven(IStreamIdentity stream, object filterData, object item)
        {
            Int32 val = (int) item;
            Assert.AreEqual(FilterDataEven, filterData, "Should have got the correct filter data passed in");
            bool result = val % 2 == 0;
            if (_logger != null) _logger.Info("FilterIsEven(Stream={0},FilterData={1},Item={2})={3}", stream, filterData, item, result);
            return result;
        }
        public static bool FilterIsOdd(IStreamIdentity stream, object filterData, object item)
        {
            Int32 val = (int) item;
            Assert.AreEqual(FilterDataOdd, filterData, "Should have got the correct filter data passed in");
            bool result = val % 2 == 1;
            if (_logger != null) _logger.Info("FilterIsOdd(Stream={0},FilterData={1},Item={2})={3}", stream, filterData, item, result);
            return result;
        }
        public bool BadFunc(IStreamIdentity stream, object filterData, object item)
        {
            return true;
        }
    }

    [Orleans.Providers.StorageProvider(ProviderName = "MemoryStore")]
    public class StreamLifecycleProducerGrain : StreamLifecycleTestGrainBase, IStreamLifecycleProducerGrain
    {
        public override async Task OnActivateAsync()
        {
            logger = GetLogger(GetType().Name + "-" + IdentityString);
            if (logger.IsVerbose)
                logger.Verbose("OnActivateAsync");

            await RecordActivate();

            StreamResourceTestControl.TestOnlySuppressStreamCleanupOnDeactivate = false;

            if (State.Stream != null && State.StreamProviderName != null)
            {
                if (logger.IsVerbose)
                    logger.Verbose("Reconnecting to stream {0}", State.Stream);
            }
            else
            {
                if (logger.IsVerbose)
                    logger.Verbose("Not connected to stream yet.");
            }
        }
        public override async Task OnDeactivateAsync()
        {
            if (logger.IsVerbose)
                logger.Verbose("OnDeactivateAsync");
            await RecordDeactivate();
        }

        public Task<int> GetSendCount()
        {
            return Task.FromResult(State.NumMessagesSent);
        }

        public Task<int> GetErrorsCount()
        {
            return Task.FromResult(State.NumErrors);
        }

        public Task Ping()
        {
            return TaskDone.Done;
        }

        public async Task SendItem(int item)
        {
            if (!State.IsProducer || State.Stream == null) throw new InvalidOperationException("Not a Producer");
            if (logger.IsVerbose)
                logger.Verbose("SendItem Item={0}", item);
            Exception error = null;
            try
            {
                await State.Stream.OnNextAsync(item);

                if (logger.IsVerbose)
                    logger.Verbose("Successful SendItem " + item);
                State.NumMessagesSent++;
            }
            catch (Exception exc)
            {
                logger.Error(0, "Error from SendItem " + item, exc);
                State.NumErrors++;
                error = exc;
            }
#if USE_STORAGE
            await WriteStateAsync(); // Update counts in persisted state
#endif

            if (error != null)
            {
                throw new AggregateException(error);
            }
        }

        public async Task BecomeProducer(Guid streamId, string providerName)
        {
            if (logger.IsVerbose)
                logger.Verbose("BecomeProducer StreamId={0} StreamProvider={1}", streamId, providerName);
            InitStream(streamId, providerName);
            State.IsProducer = true;

            // Send an initial message to ensure we are properly initialized as a Producer.
            await State.Stream.OnNextAsync(0);
            State.NumMessagesSent++;
#if USE_STORAGE
            await WriteStateAsync();
#endif
        }

        public async Task TestInternalRemoveProducer(Guid streamId, string providerName)
        {
            if (logger.IsVerbose)
                logger.Verbose("RemoveProducer StreamId={0} StreamProvider={1}", streamId, providerName);
            if (!State.IsProducer) throw new InvalidOperationException("Not a Producer");

            // Whitebox testing
            var cleanup = State.Stream as IStreamControl;
            await cleanup.Cleanup(true, false);

            State.IsProducer = false;
#if USE_STORAGE
            await WriteStateAsync();
#endif
        }

        public async Task ClearGrain()
        {
            if (logger.IsVerbose)
                logger.Verbose("ClearGrain");
            State.IsProducer = false;
            State.Stream = null;
#if USE_STORAGE
            await ClearStateAsync();
#endif
        }

        public async Task DoDeactivateNoClose()
        {
            if (logger.IsVerbose)
                logger.Verbose("DoDeactivateNoClose");

            State.IsProducer = false;
            State.Stream = null;
#if USE_STORAGE
            await WriteStateAsync();
#endif

            if (logger.IsVerbose)
                logger.Verbose("Calling DeactivateOnIdle");
            base.DeactivateOnIdle();
        }

        public async Task DoBadDeactivateNoClose()
        {
            if (logger.IsVerbose)
                logger.Verbose("DoBadDeactivateNoClose");

            if (logger.IsVerbose)
                logger.Verbose("Suppressing Cleanup when Deactivate for stream {0}", State.Stream);
            StreamResourceTestControl.TestOnlySuppressStreamCleanupOnDeactivate = true;

            State.IsProducer = false;
            State.Stream = null;
#if USE_STORAGE
            await WriteStateAsync();
#endif

            if (logger.IsVerbose) logger.Verbose("Calling DeactivateOnIdle");
            base.DeactivateOnIdle();
        }
    }
}