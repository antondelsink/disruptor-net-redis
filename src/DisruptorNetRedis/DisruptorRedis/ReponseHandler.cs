using Disruptor;
using System.Diagnostics;
using System.Text;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class ResponseHandler : IWorkHandler<RingBufferSlot>
    {
        public void OnEvent(RingBufferSlot slot)
        {
            if (slot.Response == null)
                return;

            Debug.WriteLine(nameof(ResponseHandler) + " responded with " + Encoding.UTF8.GetString(slot.Response).Replace("\r\n", "|"));

            try
            {
                slot.Session?.ClientDataStream?.Write(slot.Response, 0, slot.Response.Length);
            }
            catch (System.IO.IOException)
            {
            }

            slot.Response = null;
        }
    }
}