using Disruptor;
using DisruptorNetRedis.Networking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class DisruptorRedis : IDisposable
    {
        Disruptor.Dsl.Disruptor<RingBufferSlot> _disruptor = null;

        RingBuffer<RingBufferSlot> _ringbuffer = null;

        IEventTranslatorTwoArg<RingBufferSlot, ClientSession, List<byte[]>> _translator = null;

        public DisruptorRedis(
            IEventTranslatorTwoArg<RingBufferSlot, ClientSession, List<byte[]>> translator,
            IEventHandler<RingBufferSlot> requestParser,
            IEventHandler<RingBufferSlot> requestHandler,
            IEventHandler<RingBufferSlot> responseHandler)
        {
            _translator = translator;

            _disruptor = NewDisruptor();

            _disruptor
                .HandleEventsWith(requestParser)
                .Then(requestHandler)
                .Then(responseHandler);
        }

        private Disruptor.Dsl.Disruptor<RingBufferSlot> NewDisruptor()
        {
            return 
                new Disruptor.Dsl.Disruptor<RingBufferSlot>(
                    () => new RingBufferSlot(),
                    1024,
                    TaskScheduler.Current,
                    Disruptor.Dsl.ProducerType.Multi,
                    new SleepingWaitStrategy());
        }

        public void Start()
        {
            _ringbuffer = _disruptor?.Start();
        }

        public void Dispose()
        {
            _disruptor?.Shutdown();

            _disruptor = null;
            _ringbuffer = null;

            GC.SuppressFinalize(this);
        }

        public void OnDataAvailable(ClientSession session, List<byte[]> data)
        {
            _ringbuffer?.PublishEvent(_translator, session, data);
        }
    }
}