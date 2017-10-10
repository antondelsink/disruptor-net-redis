using DisruptorNetRedis;
using System;
using System.Net;

namespace DisruptorNetRedisConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var listenOn = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55001);

            var s = new Server(listenOn);
            s.Start();

            Console.Write("Server Running...");
            Console.ReadKey();
        }
    }
}