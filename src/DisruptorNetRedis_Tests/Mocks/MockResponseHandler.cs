using Disruptor;
using DisruptorNetRedis.DisruptorRedis;
using System.Text;

namespace DisruptorNetRedis.Tests
{
    internal class MockResponseHandler : IEventHandler<RingBufferSlot>
    {
        private StringBuilder _log = new StringBuilder();

        public void OnEvent(RingBufferSlot slot, long sequence, bool endOfBatch)
        {
            lock (_log)
            {
                _log.AppendLine(Encoding.UTF8.GetString(slot.Response).Replace("\r\n", "|").TrimEnd('|'));
            }
        }

        public override string ToString()
        {
            lock (_log)
            {
                return _log.ToString();
            }
        }
    }
}