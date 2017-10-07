using Disruptor;
using DisruptorNetRedis.Databases;
using DisruptorNetRedis.DotNetRedis;
using System.Text;
using System;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class ClientRequestHandler : IEventHandler<RingBufferSlot>
    {
        private DotNetRedisCore _core = new DotNetRedisCore();
        private StringsDatabase _dbStrings = null;

        public ClientRequestHandler(DotNetRedisCore core, StringsDatabase dbStrings)
        {
            _core = core;
            _dbStrings = dbStrings;
        }

        public void OnEvent(RingBufferSlot slot, long sequence = -1, bool endOfBatch = false)
        {
            switch (slot.RedisCommand)
            {
                case RedisCommands.GET:
                    Invoke_GET(slot);
                    break;
                case RedisCommands.SET:
                    Invoke_SET(slot);
                    break;
                case RedisCommands.ECHO:
                    Invoke_ECHO(slot);
                    break;
                case RedisCommands.PING:
                    Invoke_PING(slot);
                    break;
                case RedisCommands.SUBSCRIBE:
                    Invoke_SUBSCRIBE(slot);
                    break;
                case RedisCommands.INFO:
                    Invoke_INFO(slot);
                    break;
                case RedisCommands.CLIENT_SETNAME:
                    Invoke_CLIENT_SETNAME(slot);
                    break;
                case RedisCommands.COMMAND:
                    Invoke_COMMAND(slot);
                    break;
                case RedisCommands.Unknown:
                    slot.Response = Constants.UnknownCommandError_Binary;
                    break;
                default:
                    throw new System.InvalidOperationException();
            }
        }

        private void Invoke_INFO(RingBufferSlot slot)
        {
            slot.Response = Encoding.UTF8.GetBytes(RESP.AsRedisBulkString("# Server\r\nos:Windows\r\ntcp_port:6379\r\n"));
        }

        private void Invoke_SUBSCRIBE(RingBufferSlot slot)
        {
            // TODO: implement pub/sub

            var subscribe = RESP.AsRedisBulkString("subscribe");
            var channel = RESP.AsRedisBulkString(Encoding.UTF8.GetString(slot.Data[0]));
            var one = RESP.AsRedisNumber(1);

            slot.Response = Encoding.UTF8.GetBytes(RESP.AsRedisArray(subscribe, channel, one));
        }

        private void Invoke_CLIENT_SETNAME(RingBufferSlot slot)
        {
            var clientID = slot.Session.RemoteEndPoint.Address.Address;

            _core.Client_SetName(clientID, slot.Data[0]);

            slot.Response = Constants.OK_Binary;
        }

        private void Invoke_ECHO(RingBufferSlot slot)
        {
            slot.Response = RESP.AsRedisBulkString(slot.Data[0]);
        }

        private void Invoke_PING(RingBufferSlot slot)
        {
            if (slot.Data == null || slot.Data.Count == 0)
            {
                slot.Response = Constants.PONG_SimpleStringAsBinary;
            }
            else
            {
                slot.Response = RESP.AsRedisBulkString(slot.Data[0]);
            }
        }
        private void Invoke_GET(RingBufferSlot slot)
        {
            var key = new RedisKey(slot.Data[0]);

            slot.Response =
                _dbStrings.Get(key, out RedisValue val)
                ?
                RESP.ToBulkStringAsByteArray(val)
                :
                Constants.NULL_Binary;

            // TODO: if key NOT found, check if exists elsewhere, and if so return an error because GET should only be used on strings.
        }

        private void Invoke_SET(RingBufferSlot slot)
        {
            var key = new RedisKey(slot.Data[0]);
            var val = new RedisValue(slot.Data[1]);

            slot.Response =
                _dbStrings.Set(key, val)
                ?
                Constants.OK_Binary
                :
                Constants.ERR_Binary;

            // TODO: clear Key from non-strings key collections (list, set, etc.)
        }

        private void Invoke_COMMAND(RingBufferSlot slot)
        {
            var ci_get = RESP.CommandInfo("get", 2, new string[] { "readonly" }, 1, 1, 1);
            var ci_set = RESP.CommandInfo("set", -3, new string[] { "write", "denyoom" }, 1, 1, 1);
            var ci_cmd = RESP.CommandInfo("command", 0, new string[] { "loading", "stale" }, 0, 0, 0);

            slot.Response = Encoding.UTF8.GetBytes(RESP.AsRedisArray(ci_cmd, ci_get, ci_set));
        }
    }
}