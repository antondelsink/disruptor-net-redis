using System;
using System.Collections.Generic;
using System.Text;

using RedisServerProtocol;

namespace DisruptorNetRedis.DotNetRedis.Commands
{
    internal class ServerCommands
    {
        DotNetRedisServer _server = null;

        public ServerCommands(DotNetRedisServer server)
        {
            _server = server;
        }

        public byte[] Exec_CLIENT_SETNAME(List<byte[]> data)
        {
            var clientID = 0;// slot.Session.RemoteEndPoint.Address.Address;

            _server.Client_SetName(clientID, data[2]);

            return Constants.OK_SimpleStringAsByteArray;
        }

        public byte[] Exec_ECHO(List<byte[]> data)
        {
            return RESP.AsRedisBulkString(data[1]);
        }

        public byte[] Exec_PING(List<byte[]> data)
        {
            if (data.Count == 2)
                return RESP.AsRedisBulkString(data[1]);
            else
                return Constants.PONG_SimpleStringAsBinary;
        }

        public byte[] Exec_INFO(List<byte[]> data)
        {
            return Encoding.UTF8.GetBytes(RESP.AsRedisBulkString("# Server\r\nos:Windows\r\ntcp_port:6379\r\n"));
        }
    }
}
