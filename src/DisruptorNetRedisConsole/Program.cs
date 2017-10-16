using DisruptorNetRedis;
using System;
using System.Net;

namespace DisruptorNetRedisConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 55001;
            var listenOn = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            var s = new Server(listenOn);
            s.Start();

            Console.WriteLine("Server Running...");
            Console.WriteLine($"Go ahead, run 'redis-benchmark -t SET -p {port}' and see what happens...");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            s.Stop();
        }
    }
}